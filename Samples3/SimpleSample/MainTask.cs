using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SharpDX;
using MikuMikuFlex3;
using MikuMikuFlex3.Utility;

namespace SimpleSample
{
    class MainTask
    {
        public ManualResetEventSlim 終了指示通知 = new ManualResetEventSlim( false );
        public ManualResetEventSlim 終了完了通知 = new ManualResetEventSlim( false );

        public MainTask( Control parent, string[] args )
        {
            this._Parent = parent.Handle;
            this._ParentClientSize = new Size2( parent.ClientSize.Width, parent.ClientSize.Height );
            this._PMXファイルパス = args[ 0 ];
            this._VMDファイルパス = args[ 1 ];
            this._カメラVMDファイルパス = args[ 2 ];

            parent.MouseDown += this._Parent_MouseDown;
            parent.MouseUp += this._Parent_MouseUp;
            parent.MouseMove += this._Parent_MouseMove;
            parent.MouseWheel += this._Parent_MouseWheel;
        }

        public void Dispose()
        {
            // モデルを解放する。

            this._PMXモデル?.Dispose();
            this._DefaultMaterialShader?.Dispose();


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
            #region " 初期化 "
            //----------------

            #region " D3DDevice, DXGISwapChain を生成する。"
            //----------------
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1 },
                new SharpDX.DXGI.SwapChainDescription {
                    BufferCount = 1,
                    Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
                    IsWindowed = true,
                    ModeDescription = new SharpDX.DXGI.ModeDescription {
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Width = this._ParentClientSize.Width,
                        Height = this._ParentClientSize.Height,
                        Scaling = SharpDX.DXGI.DisplayModeScaling.Stretched,
                    },
                    OutputHandle = this._Parent,
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
                    Width = this._ParentClientSize.Width,
                    Height = this._ParentClientSize.Height,
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


            // PMX モデルを生成。

            this._DefaultMaterialShader = new MikuMikuFlex3.DefaultMaterialShader( this._D3D11Device );
            this._PMXモデル = new PMXモデル( this._D3D11Device, this._PMXファイルパス, 既定の材質シェーダー: this._DefaultMaterialShader );


            // VMD アニメーションを設定。
            using( var fs = new FileStream( this._VMDファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                var vmd = new MikuMikuFlex3.VMDFormat.モーション( fs );
                VMDアニメーションビルダ.ボーンモーションを追加する( vmd.ボーンフレームリスト, this._PMXモデル, true );
                VMDアニメーションビルダ.モーフを追加する( vmd.モーフフレームリスト, this._PMXモデル );
            }


            // カメラを生成。

            this._カメラ = new マウスモーションカメラ( 45f );

            //this._カメラ = new モーションカメラMMD(
            //    注視点からの初期距離: 40f,
            //    初期注視点: new Vector3( 0f, 10f, 0f ),
            //    初期回転rad: new Vector3( 0f, MathUtil.Pi, 0f ) );
            //using( var fs = new FileStream( this._カメラVMDファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            //{
            //    var vmd = new MikuMikuFlex3.VMDFormat.モーション( fs );
            //    VMDアニメーションビルダ.カメラモーションを追加する( vmd.カメラフレームリスト, this._カメラ );
            //}


            // 照明を生成。

            this._照明 = new 照明();


            // その他を生成。

            var timer = new QPCTimer();

            this._FPS = new FPS();
            //----------------
            #endregion
                        

            // メインループ

            while( !this.終了指示通知.IsSet )
            {
                var now = timer.現在のリアルタイムカウントsec;


                // カメラの進行。

                this._カメラ.更新する( now );


                // モデル単位のパイプライン設定。

                this._D3D11Device.ImmediateContext.ClearRenderTargetView( this._既定のD3D11RenderTargetView, Color4.Black );
                this._D3D11Device.ImmediateContext.ClearDepthStencilView( this._既定のD3D11DepthStencilView, SharpDX.Direct3D11.DepthStencilClearFlags.Depth, 1f, 0 );


                // モデルの進行描画。

                var world = Matrix.Identity;
                this._PMXモデル.描画する( now, this._D3D11Device.ImmediateContext, world, this._カメラ, this._照明, this._D3DViewport );

                if( this._FPS.FPSをカウントする() )
                    Trace.WriteLine( $"{_FPS.現在のFPS} fps" );


                // 画面の表示。

                this._DXGISwapChain.Present( 0, SharpDX.DXGI.PresentFlags.None );
            }


            // 終了

            this.Dispose();
            this.終了完了通知.Set();
        }


        private string _PMXファイルパス;
        private string _VMDファイルパス;
        private string _カメラVMDファイルパス;

        private IntPtr _Parent;

        private Size2 _ParentClientSize;

        private PMXモデル _PMXモデル;

        private マウスモーションカメラ _カメラ;

        private 照明 _照明;

        private FPS _FPS;

        private SharpDX.Direct3D11.Device _D3D11Device;

        private SharpDX.DXGI.SwapChain _DXGISwapChain;

        private SharpDX.Direct3D11.RenderTargetView _既定のD3D11RenderTargetView;

        private SharpDX.Direct3D11.Texture2D _既定のD3D11DepthStencil;

        private SharpDX.Direct3D11.DepthStencilView _既定のD3D11DepthStencilView;

        private SharpDX.Direct3D11.DepthStencilState _既定のD3D11DepthStencilState;

        private SharpDX.Direct3D11.BlendState _BlendState通常合成;

        private SharpDX.Mathematics.Interop.RawViewportF _D3DViewport;

        private IMaterialShader _DefaultMaterialShader;


        private void _Parent_MouseWheel( object sender, MouseEventArgs e )
        {
            this._カメラ?.OnMouseWheel( sender, e );
        }

        private void _Parent_MouseMove( object sender, MouseEventArgs e )
        {
            this._カメラ?.OnMouseMove( sender, e );
        }

        private void _Parent_MouseUp( object sender, MouseEventArgs e )
        {
            this._カメラ?.OnMouseUp( sender, e );
        }

        private void _Parent_MouseDown( object sender, MouseEventArgs e )
        {
            this._カメラ?.OnMouseDown( sender, e );
        }
    }
}
