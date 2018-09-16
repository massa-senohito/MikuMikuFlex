using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     use_spheremap (bool型)
    ///     スフィアマップ使用フラグ。
    ///     描画中のマテリアルがスフィアマップを使用する場合にtrue。
    ///     なお、PMXモデルのサブテクスチャを使用する場合もtrueとなる。
    /// </summary>
	internal class use_spheremap変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "use_spheremap";

		public override 変数型 変数型 => 変数型.Bool;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.材質.スフィアマップ != null );
		}
	}
}
