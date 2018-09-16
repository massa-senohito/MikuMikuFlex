using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理.特殊パラメータ
{
    /// <summary>
    ///     parthf (bool型)
    ///     セルフシャドウフラグ。
    ///     セルフシャドウのmode1/mode2に対応（falseでmode1）。
    /// </summary>
	internal class parthf変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "parthf";

		public override 変数型 変数型 => 変数型.Bool;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( RenderContext.Instance.描画ターゲットコンテキスト.IsSelfShadowMode1 );
		}
	}
}
