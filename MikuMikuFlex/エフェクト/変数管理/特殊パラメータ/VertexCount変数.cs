using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     VertexCount (int型)
    ///     オブジェクトの頂点数。
    /// </summary>
	internal class VertexCount変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "VertexCount";

		public override 変数型 変数型 => 変数型.Int;


		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.モデル.頂点数 );
		}
	}
}
