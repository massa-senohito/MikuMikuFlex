using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.マウス
{
    /// <summary>
    ///     MOUSEPOSITION
    ///     
    ///     マウスの現在位置。
    ///     型はfloat2。
    ///     
    ///     MMDの描画領域の中心を(0,0)とし、左下隅が(-1,-1)、右上隅が(1,1)になる。
    ///     このxy座標の取り方は、プロジェクション変換した後の、頂点座標と同じである。
    ///     
    ///     ○アノテーション
    ///         なし
    ///     ○使用例
    ///         float2 pos : MOUSEPOSITION;
    /// </summary>
	internal sealed class MOUSEPOSITION変数 : マウス変数
	{
		public override string セマンティクス => "MOUSEPOSITION";

		public override 変数型[] 使える型の配列 => new[] { 変数型.Float2 };


        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
        {
            return new MOUSEPOSITION変数();
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsVector().Set( RenderContext.Instance.パネル監視.MousePosition );
		}
	}
}
