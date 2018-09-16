using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     use_selfshadow (bool型)
    ///     セルフ影使用フラグ。
    /// </summary>
	internal class use_selfshadow変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "use_selfshadow";

		public override 変数型 変数型 => 変数型.Bool;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.材質.セルフ影を使用する );
		}
	}
}
