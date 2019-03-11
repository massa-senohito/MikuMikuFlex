using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理.特殊パラメータ
{
    /// <summary>
    ///     SubsetCount (int型)
    ///     オブジェクトのサブセット数。
    /// </summary>
	internal class SubsetCount変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "SubsetCount";

		public override 変数型 変数型 => 変数型.Int;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.モデル.サブセット数 );
		}
	}
}
