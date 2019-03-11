using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理.特殊パラメータ
{
    /// <summary>
    ///     opadd (bool型)
    ///     加算合成フラグ。
    ///     オブジェクトの描画が加算合成モードに設定されている場合にtrue。
    /// </summary>
	internal class opadd変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "opadd";

		public override 変数型 変数型 => 変数型.Bool;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.材質.加算合成モードである );
		}
	}
}
