using MMDFileParser.PMXModelParser;
using MikuMikuFlex.物理演算;

namespace MikuMikuFlex
{
	internal class PMXスケルトン物理変形付き : PMXスケルトン
	{
		public PMXスケルトン物理変形付き( PMXモデル model ) 
            : base( model )
		{
			_物理変形管理 = new PMX物理変形管理( ボーン配列, model.剛体リスト, model.ジョイントリスト );

			変形更新リスト.Add( _物理変形管理 );
		}

		public override void Dispose()
		{
			_物理変形管理?.Dispose();
            _物理変形管理 = null;
        }


        private PMX物理変形管理 _物理変形管理;
    }
}
