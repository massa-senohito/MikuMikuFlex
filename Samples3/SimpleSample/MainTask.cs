using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using MikuMikuFlex3;

namespace SimpleSample
{
    class MainTask
    {
        public ManualResetEventSlim 終了指示通知 = new ManualResetEventSlim( false );
        public ManualResetEventSlim 終了完了通知 = new ManualResetEventSlim( false );

        public MainTask( Control parent, string pmxファイルパス )
        {
            this._Parent = parent;

            #region " D3DDevice, DXGISwapChain を生成する。"
            //----------------
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1 },
                new SharpDX.DXGI.SwapChainDescription {
                    BufferCount = 2,
                    Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
                    IsWindowed = true,
                    ModeDescription = new SharpDX.DXGI.ModeDescription {
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Width = parent.ClientSize.Width,
                        Height = parent.ClientSize.Height,
                        Scaling = SharpDX.DXGI.DisplayModeScaling.Stretched,
                    },
                    OutputHandle = parent.Handle,
                    SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                    SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                },
                out this._D3D11Device,
                out this._DXGISwapChain );
            //----------------
            #endregion

            #region " 既定のRenderTargetView, 既定のDepthStencilView を作成する。"
            //----------------
            using( var backbufferTexture2D = this._DXGISwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>( 0 ) )
            {
                this._既定のD3D11RenderTargetView = new SharpDX.Direct3D11.RenderTargetView( this._D3D11Device, backbufferTexture2D );

                this._既定のD3D11DepthStencil = new SharpDX.Direct3D11.Texture2D(
                    this._D3D11Device,
                    new SharpDX.Direct3D11.Texture2DDescription {
                        Width = backbufferTexture2D.Description.Width,              // バックバッファと同じサイズ
                        Height = backbufferTexture2D.Description.Height,            // 
                        MipLevels = 1,
                        ArraySize = 1,
                        Format = SharpDX.DXGI.Format.D32_Float,                     // 32bit Depth
                        SampleDescription = backbufferTexture2D.Description.SampleDescription,  // バックバッファと同じサンプル記述
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,    // CPUからはアクセスしない
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    } );

                this._既定のD3D11DepthStencilView = new SharpDX.Direct3D11.DepthStencilView(
                    this._D3D11Device,
                    this._既定のD3D11DepthStencil,
                    new SharpDX.Direct3D11.DepthStencilViewDescription {
                        Format = this._既定のD3D11DepthStencil.Description.Format,
                        Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D,
                        Flags = SharpDX.Direct3D11.DepthStencilViewFlags.None,
                        Texture2D = new SharpDX.Direct3D11.DepthStencilViewDescription.Texture2DResource() {
                            MipSlice = 0,
                        },
                    } );

                this._既定のD3D11DepthStencilState = null;
            }
            //----------------
            #endregion

            #region " ビューポートを作成する。"
            //----------------
            {
                this._D3DViewport = new SharpDX.Mathematics.Interop.RawViewportF {
                    X = 0,
                    Y = 0,
                    Width = this._Parent.ClientSize.Width,
                    Height = this._Parent.ClientSize.Height,
                    MinDepth = 0.0f,
                    MaxDepth = 1.0f,
                };
            }
            //----------------
            #endregion

            #region " ブレンドステート通常版を生成する。"
            //----------------
            {
                var blendStateNorm = new SharpDX.Direct3D11.BlendStateDescription() {
                    AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                    IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
                };
                blendStateNorm.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
                blendStateNorm.RenderTarget[ 0 ].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。

                // アルファ値のブレンディング設定 ... 特になし
                blendStateNorm.RenderTarget[ 0 ].SourceAlphaBlend = SharpDX.Direct3D11.BlendOption.One;
                blendStateNorm.RenderTarget[ 0 ].DestinationAlphaBlend = SharpDX.Direct3D11.BlendOption.Zero;
                blendStateNorm.RenderTarget[ 0 ].AlphaBlendOperation = SharpDX.Direct3D11.BlendOperation.Add;

                // 色値のブレンディング設定 ... アルファ強度に応じた透明合成（テクスチャのアルファ値は、テクスチャのアルファ×ピクセルシェーダでの全体アルファとする（HLSL参照））
                blendStateNorm.RenderTarget[ 0 ].SourceBlend = SharpDX.Direct3D11.BlendOption.SourceAlpha;
                blendStateNorm.RenderTarget[ 0 ].DestinationBlend = SharpDX.Direct3D11.BlendOption.InverseSourceAlpha;
                blendStateNorm.RenderTarget[ 0 ].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;

                // ブレンドステートを作成する。
                this._BlendState通常合成 = new SharpDX.Direct3D11.BlendState( this._D3D11Device, blendStateNorm );
            }
            //----------------
            #endregion


            // ステージ単位のパイプライン設定。

            this._D3D11Device.ImmediateContext.OutputMerger.SetTargets( this._既定のD3D11DepthStencilView, this._既定のD3D11RenderTargetView );
            this._D3D11Device.ImmediateContext.OutputMerger.SetBlendState( this._BlendState通常合成, new Color4( 0f, 0f, 0f, 0f ), -1 );
            this._D3D11Device.ImmediateContext.OutputMerger.SetDepthStencilState( this._既定のD3D11DepthStencilState, 0 );


            // モデルその他を生成。

            this._PMXモデル = new PMXモデル( this._D3D11Device, pmxファイルパス );

            this._カメラ = new カメラ(
                初期位置: new Vector3( 0f, 0f, -45f ),
                初期注視点: new Vector3( 0f, 10f, 0f ),
                初期上方向: new Vector3( 0f, 1f, 0f ) );

            this._照明 = new 照明();
        }

        public void Dispose()
        {
            // モデルを解放する。

            this._PMXモデル?.Dispose();


            // D3D関連リソースを解放する。

            this._BlendState通常合成?.Dispose();
            this._既定のD3D11DepthStencilState?.Dispose();
            this._既定のD3D11DepthStencilView?.Dispose();
            this._既定のD3D11DepthStencil?.Dispose();
            this._既定のD3D11RenderTargetView?.Dispose();
            this._DXGISwapChain?.Dispose();
            this._D3D11Device?.Dispose();
        }

        public void MainLoop()
        {
            while( !this.終了指示通知.IsSet )
            {
                // モデル単位のパイプライン設定。

                this._D3D11Device.ImmediateContext.ClearRenderTargetView( this._既定のD3D11RenderTargetView, Color4.Black );
                this._D3D11Device.ImmediateContext.ClearDepthStencilView( this._既定のD3D11DepthStencilView, SharpDX.Direct3D11.DepthStencilClearFlags.Depth, 1f, 0 );


                // モデルの進行描画。

                var world = Matrix.Identity;

                this._PMXモデル.進行する();
                this._PMXモデル.描画する( this._D3D11Device.ImmediateContext, world, this._カメラ, this._照明, this._D3DViewport );


                // 画面の表示。

                this._DXGISwapChain.Present( 1, SharpDX.DXGI.PresentFlags.None );
            }

            this.Dispose();
            this.終了完了通知.Set();
        }


        private PMXモデル _PMXモデル;

        private Control _Parent;

        private カメラ _カメラ;

        private 照明 _照明;

        private SharpDX.Direct3D11.Device _D3D11Device;

        private SharpDX.DXGI.SwapChain _DXGISwapChain;

        private SharpDX.Direct3D11.RenderTargetView _既定のD3D11RenderTargetView;

        private SharpDX.Direct3D11.Texture2D _既定のD3D11DepthStencil;

        private SharpDX.Direct3D11.DepthStencilView _既定のD3D11DepthStencilView;

        private SharpDX.Direct3D11.DepthStencilState _既定のD3D11DepthStencilState;

        private SharpDX.Direct3D11.BlendState _BlendState通常合成;

        private SharpDX.Mathematics.Interop.RawViewportF _D3DViewport;
    }
}
