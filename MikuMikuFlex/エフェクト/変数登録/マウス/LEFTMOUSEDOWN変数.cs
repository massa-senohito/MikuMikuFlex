using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.マウス
{
    /// <summary>
    ///     マウスの左ボタンに関する情報。
    ///     型はfloat4。
    ///     　
    ///     取得できる値は、以下の4成分からなる。
    ///     ・最後にボタンが押されたときのマウスの座標（xとy）
    ///     ・現在ボタンが押されているか （0 or 1）
    ///     ・最後にボタンが押された時点のTIME値( 秒)
    ///     
    ///     なお、マウスの座標の取り方は、MOUSEPOSITIONと同じである。
    ///     
    ///     ○アノテーション
    ///         なし
    ///         
    ///     ○使用例
    ///         float4 mouse_down : LEFTMOUSEDOWN;
    ///         static float2 pos = mouse_down.xy;
    ///         static bool is_pressed = ( mouse_down.z != 0 );
    /// </summary>
	internal sealed class LEFTMOUSEDOWN変数 : マウス変数
	{
		public override string セマンティクス => "LEFTMOUSEDOWN";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float4 };


        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
        {
            return new LEFTMOUSEDOWN変数();
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsVector().Set( RenderContext.Instance.パネル監視.LeftMouseDown );
		}
	}
}
