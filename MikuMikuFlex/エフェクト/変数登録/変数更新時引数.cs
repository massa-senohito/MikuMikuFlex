using MikuMikuFlex.エフェクト.変数管理.材質;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.エフェクト.変数管理
{
	public class 変数更新時引数
	{
        public IDrawable モデル { get; private set; }

        public エフェクト用材質情報 材質 { get; private set; }


        public 変数更新時引数( IDrawable model )
		{
			モデル = model;
		}

		public 変数更新時引数( エフェクト用材質情報 info, IDrawable model )
		{
			材質 = info;
			モデル = model;
		}

	}
}
