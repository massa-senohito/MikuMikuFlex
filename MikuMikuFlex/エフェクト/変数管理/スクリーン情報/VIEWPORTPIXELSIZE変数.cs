using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
	public class VIEWPORTPIXELSIZE変数 : 変数管理
	{
		public override string セマンティクス => "VIEWPORTPIXELSIZE";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float2 };


		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			return new VIEWPORTPIXELSIZE変数();
		}

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			var viewport = RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.GetViewports<ViewportF>()[ 0 ];
			変数.AsVector().Set( new Vector2( viewport.Width, viewport.Height ) );
		}
	}
}
