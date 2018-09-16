using System.Diagnostics;
using System.IO;

namespace MikuMikuFlex
{
	/// <summary>
	///     標準的なリソース読み込みの実装。
    ///     指定したディレクトリをベースとしてリソースファイルを読み込む。
	/// </summary>
	public class サブリソースローダー
	{
        public string ベースディレクトリ { get; set; }


        public サブリソースローダー( string baseDir )
		{
			ベースディレクトリ = string.IsNullOrEmpty( baseDir ) ?  @".\" : Path.GetFullPath( baseDir );
		}

        public Stream リソースのストリームを返す( string name )
        {
            string path = Path.Combine( ベースディレクトリ, name );

            if( File.Exists( path ) )
            {
                // (A) リソースがTGAファイルの場合
                if( Path.GetExtension( name ).ToUpper().Equals( ".TGA" ) )
                {
                    return TargaSolver.LoadTargaImage( Path.Combine( ベースディレクトリ, name ) );
                }

                // (B) その他のファイルの場合
                return File.OpenRead( Path.Combine( ベースディレクトリ, name ) );
            }
            else
            {
                Debug.WriteLine( $"\"{path}\"は見つかりませんでした。" );
                return null;
            }
        }
	}
}