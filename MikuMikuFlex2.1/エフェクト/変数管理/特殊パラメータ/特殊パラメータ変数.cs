using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理.特殊パラメータ
{
    internal abstract class 特殊パラメータ変数
	{
		public abstract string 変数名 { get; }

		public abstract 変数型 変数型 { get; }


        public abstract void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 );
	}
}
