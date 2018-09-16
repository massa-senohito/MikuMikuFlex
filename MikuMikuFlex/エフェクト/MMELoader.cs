using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using Application = System.Windows.Forms.Application;

namespace MikuMikuFlex
{
	/// <summary>
	///     エフェクトのキャッシュをとっておくクラス
	///     コンパイル回数を少なくするために効率的な初期化が可能
	/// </summary>
	public class MMELoader : IDisposable
	{
        // 唯一のインスタンス
		public static MMELoader Instance
		{
			get
			{
				if( _Instance == null ) _Instance = new MMELoader();
				return _Instance;
			}
			private set { _Instance = value; }
		}

		public event EventHandler<EffectLoaderCompilingEventArgs> Onコンパイル開始 = delegate { };

		public event EventHandler<EffectLoaderCompiledEventArgs> Onコンパイル終了 = delegate { };

		public SQLiteConnection SQLコネクション { get; set; }


        public MMELoader()
        {
            bool isExit = File.Exists( _キャッシュファイル名 );   // ファイルがない場合 SQLiteConnection.Open で作成されてしまうので、先にチェックする。

            SQLコネクション = new SQLiteConnection( _DB接続文字列 );
            SQLコネクション.Open();

            // キャッシュファイルが存在しないならテーブルをもったキャッシュファイル（データソース）を新規作成する
            if( !isExit )
            {
                using( SQLiteCommand command = new SQLiteCommand( _テーブル作成SQL, SQLコネクション ) )
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void キャッシュファイルを削除する()
		{
			if( File.Exists( _キャッシュファイル名 ) )
			{
				File.Delete( _キャッシュファイル名 );
			}
		}

        public void Dispose()
        {
            SQLコネクション?.Close();
        }

        public ShaderBytecode 指定したファイル名からシェーダーバイトコードを取得して返す( string fileName, Stream fileStream )
		{
			string ファイルのハッシュ = _ストリームのハッシュ値を計算して返す( fileStream );

			fileStream.Seek( 0, SeekOrigin.Begin );

			using( var getBlobCommand = new SQLiteCommand( string.Format( _シェーダーバイトコードのクエリSQL, fileName, ファイルのハッシュ ), SQLコネクション ) )
			using( SQLiteDataReader blobReader = getBlobCommand.ExecuteReader() )
			{
				if( blobReader.Read() )
				{
                    // ハッシュとファイル名が等しい場合(ファイルが更新されず、名前も等しい時)はデータベースから取得
                    return new ShaderBytecode( getBytesToMemoryStream( blobReader, 0 ) );
				}
				else
				{
                    // キャッシュから取得できなかった場合はコンパイルが必要と判断しコンパイルする

                    int shaderCodeLength = (int) fileStream.Length;
					byte[] shaderCode = new byte[ shaderCodeLength ];   // シェーダコードは ASCII (またはShiftJIS) で保存されているものとし、（stringではなく）byte[] で読み込む。
                    fileStream.Read( shaderCode, 0, shaderCodeLength );

                    ShaderFlags flags = ShaderFlags.None;
#if DEBUG
                    flags |= ShaderFlags.Debug;
                    flags |= ShaderFlags.SkipOptimization;
                    flags |= ShaderFlags.EnableBackwardsCompatibility;
#endif
                    Debug.WriteLine( $"Start compiling shader:{fileName},please wait..." );

					Onコンパイル開始?.Invoke( this, new EffectLoaderCompilingEventArgs( fileName ) );

					ShaderBytecode shaderBytecode = null;

					Task t = new Task( () => {

                        // エフェクトコードをバイトコードにコンパイルする。
                        // Shader Models vs Shader Profiles <https://docs.microsoft.com/en-us/windows/desktop/direct3dhlsl/dx-graphics-hlsl-models>
                        // Specifying Compiler Targets　<https://docs.microsoft.com/en-us/windows/desktop/direct3dhlsl/specifying-compiler-targets>

                        var compileResult = (CompilationResult) null;

                        try
                        {
                            compileResult = ShaderBytecode.Compile(
                                shaderCode,
                                "fx_5_0",
                                flags,
                                EffectFlags.None,
                                エフェクト.マクロリスト.ToArray(),
                                エフェクト.Include );

                            Debug.WriteLine( $"ResultCode: {compileResult.ResultCode}" );
                            Debug.WriteLine( $"{compileResult.Message}" );

                            if( compileResult?.Bytecode == null )
                                Debug.WriteLine( "このエフェクトファイルには対応していないか、エラーが発生しました。" );
                        }
                        catch( Exception e )
                        {
                            Debug.WriteLine( e.Message );
                        }

                        shaderBytecode = compileResult;


                        // 古いレコードを DB から削除する。
                        using( var getFileByNameCommand = new SQLiteCommand( string.Format( _IDのクエリSQL, fileStream ), SQLコネクション ) )
						using( SQLiteDataReader fileNameReader = getFileByNameCommand.ExecuteReader() )
						{
							while( fileNameReader.Read() )
							{
								using( var deleteSameNameCommand = new SQLiteCommand( string.Format( _削除SQL, fileNameReader.GetInt32( 0 ) ), SQLコネクション ) )
								{
									deleteSameNameCommand.ExecuteNonQuery();
								}
							}
						}
					
                        // 新たなキャッシュとしてエフェクトを記録する。
						byte[] sbcBytes = shaderBytecode.Data;
						using( var blobInsertCommand = new SQLiteCommand( string.Format( _挿入SQL, fileName, ファイルのハッシュ ), SQLコネクション ) )
						{
							blobInsertCommand.Parameters.Add( "@resource", DbType.Binary, sbcBytes.Length ).Value = sbcBytes;
							blobInsertCommand.ExecuteNonQuery();
						}

                    } );

					t.Start();

					while( !t.IsCompleted )
					{
						Application.DoEvents();     // これはなんとかしよう
					}

					Onコンパイル終了?.Invoke( this, new EffectLoaderCompiledEventArgs() );

					return shaderBytecode;
				}
			}
		}

		protected MemoryStream getBytesToMemoryStream( SQLiteDataReader reader, int index )
		{
			var ms = new MemoryStream();
			byte[] buffer = new byte[ 2048 ];
			long readed;
			long offset = 0;
			while( ( readed = reader.GetBytes( index, offset, buffer, 0, buffer.Length ) ) > 0 )
			{
				ms.Write( buffer, 0, (int) readed );
				offset += readed;
			}
			ms.Seek( 0, SeekOrigin.Begin );
			return ms;
		}


        private static MMELoader _Instance;

        private static readonly string _キャッシュファイル名 = "effect.cache";

        private static readonly string _DB接続文字列 = "Data Source=effect.cache";

        private static readonly string _テーブル作成SQL
            = @"CREATE TABLE 'DBHeader' (
             'Id' INTEGER PRIMARY KEY ON CONFLICT FAIL AUTOINCREMENT UNIQUE ON CONFLICT FAIL DEFAULT '',
            'Property' CHAR NOT NULL ON CONFLICT FAIL,
            'Value' CHAR);
            INSERT INTO DBHeader VALUES(NULL,'DBType','EffectCacheDatabase');
            INSERT INTO DBHeader VALUES(NULL,'FileVersion','1.0');
            CREATE TABLE 'EffectCache'(
            'Id' INTEGER PRIMARY KEY ON CONFLICT FAIL AUTOINCREMENT UNIQUE ON CONFLICT FAIL DEFAULT '',
            'FileName' CHAR NOT NULL ON CONFLICT FAIL,
            'HashCode' CHAR NOT NULL ON CONFLICT FAIL,
            'ShaderByteCode' BLOB);";

        private static readonly string _シェーダーバイトコードのクエリSQL
            = "SELECT ShaderByteCode FROM EffectCache WHERE FileName=='{0}' AND HashCode=='{1}';";

        private static readonly string _IDのクエリSQL
            = "SELECT Id FROM EffectCache WHERE FileName=='{0}';";

        private static readonly string _削除SQL
            = "DELETE FROM EffectCache WHERE Id=={0};";

        private static readonly string _挿入SQL
            = "INSERT INTO EffectCache VALUES(NULL,'{0}','{1}',@resource);";


        private string _ファイルのハッシュ値を計算して返す( string filePath )
		{
			using( FileStream fs = File.OpenRead( filePath ) )
				return _ストリームのハッシュ値を計算して返す( fs );
		}

		private string _ストリームのハッシュ値を計算して返す( Stream stream )
		{
			using( MD5 md5 = MD5.Create() )
			{
				byte[] hashBytes = md5.ComputeHash( stream );

                var builder = new StringBuilder();

                foreach( byte hashByte in hashBytes )
				{
					builder.Append( hashByte.ToString( "x2" ) );
				}

                return builder.ToString();
			}
		}
	}


	public class EffectLoaderCompilingEventArgs : EventArgs
	{
        public string TargetFileName { get; }

        public EffectLoaderCompilingEventArgs( string targetFileName )
		{
			this.TargetFileName = targetFileName;
		}
	}


	public class EffectLoaderCompiledEventArgs : EventArgs
	{

	}
}
