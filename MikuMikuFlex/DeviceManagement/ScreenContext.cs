using System.Windows.Forms;
using MikuMikuFlex.行列;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace MikuMikuFlex
{
	public class ScreenContext : TargetContext
	{
		public ScreenContext( Control owner, 行列管理 manager )
            : base()
		{
            // SwapChain の生成
			var d3ddevice = RenderContext.Instance.DeviceManager.D3DDevice;
            var sampleDesc = new SampleDescription( 1, 0 );
            this.SwapChain = new SwapChain( RenderContext.Instance.DeviceManager.DXGIFactory, d3ddevice, getSwapChainDescription( owner, sampleDesc ) );
            
			//ラスタライザの設定

			//深度ステンシルバッファの初期化
			using( var depthBuffer = new Texture2D( d3ddevice, getDepthBufferTexture2DDescription( owner, sampleDesc ) ) )
			{
				this.深度ステンシルビュー = new DepthStencilView( d3ddevice, depthBuffer );
			}
			//レンダーターゲットの初期化
			using( Texture2D renderTexture = Resource.FromSwapChain<Texture2D>( SwapChain, 0 ) )
			{
				this.D3Dレンダーターゲットビュー = new RenderTargetView( d3ddevice, renderTexture );
			}

            // その他

			this.ワールド空間 = new ワールド空間();
			this.BindedControl = owner;
			this.行列管理 = manager;
			this.パネル監視 = new マウス監視( owner );
			this.ビューポートを設定する();
		}

		public 行列管理 行列管理 { get; set; }

		public SwapChain SwapChain { get; private set; }

		public Control BindedControl { get; private set; }

		public カメラモーション カメラモーション { get; set; }

		/// <summary>
		///     このスクリーンに結び付けられているワールド空間
		/// </summary>
		public ワールド空間 ワールド空間 { get; set; }

		public マウス監視 パネル監視 { get; private set; }

        public RenderTargetView D3Dレンダーターゲットビュー { get; protected set; }

        public DepthStencilView 深度ステンシルビュー { get; protected set; }

        public bool IsSelfShadowMode1 { get; protected set; }

        public bool IsEnabledTransparent { get; protected set; }


        public ScreenContext()
        {
        }

        public void Dispose()
        {
            ワールド空間?.Dispose();

            D3Dレンダーターゲットビュー?.Dispose();
            D3Dレンダーターゲットビュー = null;

            深度ステンシルビュー?.Dispose();
            深度ステンシルビュー = null;

            SwapChain?.Dispose();
            SwapChain = null;
        }

        public void ビューポートを設定する()
		{
			RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.SetViewports( new SharpDX.Mathematics.Interop.RawViewportF[] { getViewport() } );
		}

		public void RenderContextにマウス監視を登録する()
		{
			RenderContext.Instance.パネル監視 = パネル監視;
		}

		public void スワップチェーンをリサイズする()
		{
			if( BindedControl.ClientSize.Width == 0 || BindedControl.ClientSize.Height == 0 )
                return; //フォームがフロート状態になった時一瞬だけ来て、デバイスが作れなくなるのでこの時はなしにする。

            // デバイスリソースの解放
			レンダーターゲットと深度ステンシルターゲットをDisposeする();

            // スワップチェーンのリサイズ
            var desc = SwapChain.Description;
            SwapChain.ResizeBuffers( desc.BufferCount, BindedControl.ClientSize.Width, BindedControl.ClientSize.Height, desc.ModeDescription.Format, desc.Flags );

            // デバイスリソースの作成
            using( Texture2D depthBuffer = new Texture2D( RenderContext.Instance.DeviceManager.D3DDevice, getDepthBufferTexture2DDescription( BindedControl, desc.SampleDescription ) ) )
			{
				深度ステンシルビュー = new DepthStencilView( RenderContext.Instance.DeviceManager.D3DDevice, depthBuffer );
			}
			using( Texture2D renderTexture = Resource.FromSwapChain<Texture2D>( SwapChain, 0 ) )
			{
				D3Dレンダーターゲットビュー = new RenderTargetView( RenderContext.Instance.DeviceManager.D3DDevice, renderTexture );
			}
		}

		public void カメラを移動する()
		{
			カメラモーション?.モーションを更新する( 行列管理.ビュー行列管理, 行列管理.射影行列管理 );
		}

        public void ステージングテクスチャにレンダーターゲット全体をコピーする( Texture2D stagingTexture )
        {
            RenderContext.Instance.DeviceManager.D3DDeviceContext.CopyResource( D3Dレンダーターゲットビュー.Resource, stagingTexture );
        }

        public void ステージングテクスチャにレンダーターゲットの一部をコピーする( Texture2D stagingTexture, int left, int top, int height, int width )
        {
            RenderContext.Instance.DeviceManager.D3DDeviceContext.CopySubresourceRegion( D3Dレンダーターゲットビュー.Resource, 0, new ResourceRegion( left, top, 0, left + width, top + height, 1 ), stagingTexture, 0, 0, 0, 0 );
        }


        protected void レンダーターゲットと深度ステンシルターゲットをDisposeする()
        {
            if( D3Dレンダーターゲットビュー != null && !D3Dレンダーターゲットビュー.IsDisposed )
                D3Dレンダーターゲットビュー.Dispose();

            if( 深度ステンシルビュー != null && !深度ステンシルビュー.IsDisposed )
                深度ステンシルビュー.Dispose();
        }

        /// <summary>
        ///     スワップチェーンの設定を取得します。
        ///     スワップチェーンの設定を変えたい場合は、オーバーライドしてください。
        /// </summary>
        /// <param name="control">適応するコントロールへの参照</param>
        /// <returns>スワップチェーンの設定</returns>
        protected virtual SwapChainDescription getSwapChainDescription( Control control, SampleDescription sampDesc )
		{
			return new SwapChainDescription {
				BufferCount = 2,
				Flags = SwapChainFlags.AllowModeSwitch,
				IsWindowed = true,
				ModeDescription = new ModeDescription {
					Format = Format.R8G8B8A8_UNorm,
					Height = control.ClientSize.Height,
					Width = control.ClientSize.Width,
					RefreshRate = new Rational( 60, 1 )
				},
				OutputHandle = control.Handle,
				SampleDescription = sampDesc,
				SwapEffect = SwapEffect.Discard,
				Usage = Usage.RenderTargetOutput
			};
		}

		/// <summary>
		///     深度ステンシルバッファの設定を取得します。
		///     深度ステンシルバッファの設定を変えたい場合はオーバーライドしてください。
		/// </summary>
		/// <param name="control">適用するコントロールへの参照</param>
		/// <returns>深度ステンシルバッファ用のTexture2Dの設定</returns>
		protected virtual Texture2DDescription getDepthBufferTexture2DDescription( Control control, SampleDescription desc )
		{
			return new Texture2DDescription {
				ArraySize = 1,
				BindFlags = BindFlags.DepthStencil,
				Format = Format.D32_Float,
				Width = control.ClientSize.Width,
				Height = control.ClientSize.Height,
				MipLevels = 1,
				SampleDescription = desc
			};
		}

		/// <summary>
		///     ビューポートの内容を取得します。
		/// </summary>
		/// <param name="control">適用するコントロールへの参照</param>
		/// <returns>設定するビューポート</returns>
		protected virtual Viewport getViewport()
		{
			return new Viewport {
				Width = BindedControl.Width,
				Height = BindedControl.Height,
				MaxDepth = 1
			};
		}
	}
}
