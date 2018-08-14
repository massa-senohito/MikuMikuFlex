using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.特殊パラメータ
{
    /// <summary>
    ///     transp (bool型)
    ///     半透明フラグ（trueで半透明化）。
    ///     MME の [表示(V)]-[半透明化] に対応。
    /// </summary>
	public class transp変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "transp";

		public override 変数型 変数型 => 変数型.Bool;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( RenderContext.Instance.描画ターゲットコンテキスト.IsEnabledTransparent );
		}
	}
}
