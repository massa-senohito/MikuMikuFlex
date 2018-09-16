using System.IO;
using MMDFileParser.PMXModelParser;

namespace MikuMikuFlex.モデル.PMX
{
    /// <summary>
    ///     スキニングに <see cref="PMXスケルトン物理変形付き"/> を使う PMXModel
    /// </summary>
	public class PMXModel物理変形付き : PMXModel
	{
		public PMXModel物理変形付き( PMXモデル modeldata, サブリソースローダー subResourceLoader, string filename )
			: base( modeldata, subResourceLoader, filename )
		{
		}

        public override void Dispose()
        {
            スキニング?.Dispose();
            スキニング = null;

            base.Dispose();
        }

        /// <summary>
        ///     モデルファイルを開く
        /// </summary>
        /// <param name="filePath">PMXのファイルパス、テクスチャは同じフォルダから読み込まれる</param>
        /// <returns>MMDModelのインスタンス</returns>
        public new static PMXModel ファイルから読み込む( string filePath )
        {
            var model = (PMXModel物理変形付き) ファイルから開く( filePath );
            model.モデルを初期化する();
            return model;
        }

        public new static PMXModel ファイルから開く( string filePath )
		{
            string folder = Path.GetDirectoryName( filePath );
            return ファイルから開く( filePath, folder );
        }

        public new static PMXModel ファイルから開く( string filePath, string textureFolder )
		{
			return (PMXModel物理変形付き) ファイルから開く( filePath, new サブリソースローダー( textureFolder ) );
		}

		public new static PMXModel ファイルから開く( string filePath, サブリソースローダー loader )
		{
			using( FileStream fs = File.OpenRead( filePath ) )
			{
				return new PMXModel物理変形付き( PMXモデル.読み込む( fs ), loader, Path.GetFileName( filePath ) );
			}
		}

		protected override スキニング スキニングを初期化して返す()
		{
			return new PMXスケルトン物理変形付き( モデル );
		}
	}
}