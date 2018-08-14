using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MMF.Utility;
using SharpDX.Direct3D11;

namespace MMF.モデル
{
	internal class PMXトゥーンテクスチャ管理 : トゥーンテクスチャ管理
	{
		public ShaderResourceView[] このアバターのトゥーンの配列
			=> _シェーダーリソースビューのリスト.ToArray();


		public void 初期化する( サブリソースローダー subresourceLoader )
		{
			_サブリソースローダー = subresourceLoader;

            // toom0.bmp ～ toom10.bmp を読み込む。
            for( int i = 0; i <= 10; i++ )
			{
                string path = Path.Combine( CGHelper.Toonリソースフォルダ + $"toon{i}.bmp" );

                if( File.Exists( path ) )
                {
                    _シェーダーリソースビューのリスト.Add( MMFShaderResourceView.FromFile( RenderContext.Instance.DeviceManager.D3DDevice, path ) );
                }
                else
                {
                    // プリセットのはずのファイルがないファイルがない
                    Debug.WriteLine( $"toom{i}.bmp が存在しません。無視します。" );
                }
			}
		}

		public int トゥーンを追加で読み込み現在の最後のトゥーンインデックスを返す( string path )
		{
			using( Stream stream = _サブリソースローダー.リソースのストリームを返す( path ) )
			{
				if( stream == null )
                    return 0;

				_シェーダーリソースビューのリスト.Add( MMFShaderResourceView.FromStream( RenderContext.Instance.DeviceManager.D3DDevice, stream ) );

                return _シェーダーリソースビューのリスト.Count - 1;
			}
		}

		public void Dispose()
		{
			foreach( ShaderResourceView shaderResourceView in _シェーダーリソースビューのリスト )
				shaderResourceView.Dispose();

            _シェーダーリソースビューのリスト.Clear();
		}


        private readonly List<ShaderResourceView> _シェーダーリソースビューのリスト = new List<ShaderResourceView>();

        private サブリソースローダー _サブリソースローダー;
    }
}