using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ニコニ立体ちゃんサンプル
{
    public partial class Form1 : SharpDX.Windows.RenderForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        // アプリの初期化
        protected override void OnLoad( EventArgs e )
        {
            this.ClientSize = new Size( 1280, 720 );
            this.Text = "ニコニ立体ちゃんサンプル for MikuMikuFlex 3";

            this._Direct3Dを初期化する();


            // シーンを作成。

            this._シーン = new MikuMikuFlex3.シーン( this._D3D11Device, this._既定のD3D11DepthStencil, this._既定のD3D11RenderTarget );


            // モデル１・アリシアを作成してシーンに追加。

            this._アリシア = new MikuMikuFlex3.PMXモデル( this._D3D11Device, @"サンプルデータ/Alicia/MMD/Alicia_solid.pmx" );

            foreach( var mat in this._アリシア.材質リスト )
                mat.テッセレーション係数 = 5;

            MikuMikuFlex3.VMDアニメーションビルダ.アニメーションを追加する( @"サンプルデータ/Alicia/MMD Motion/2分ループステップ1.vmd", this._アリシア );

            this._シーン.追加する( this._アリシア );


            // モデル２・ニコニ立体ステージを作成してシーンに追加。

            this._舞台 = new MikuMikuFlex3.PMXモデル( this._D3D11Device, @"サンプルデータ/nicosolid_stage/nicosolid_stage.pmx" );

            this._シーン.追加する( this._舞台 );


            // カメラを作成してシーンに追加。

            var カメラ = new MikuMikuFlex3.マウスモーションカメラ( 45f );
            this.MouseDown += カメラ.OnMouseDown;
            this.MouseUp += カメラ.OnMouseUp;
            this.MouseMove += カメラ.OnMouseMove;
            this.MouseWheel += カメラ.OnMouseWheel;
            this._シーン.追加する( カメラ );


            // 照明を作成してシーンに追加。

            this._シーン.追加する( new MikuMikuFlex3.照明() );
            

            this.Activate(); // ウィンドウが後ろに隠れることがあるので、念のため。
            base.OnLoad( e );


            // 処理のごたごたが落ち着いたらメインループへ。
            Application.Idle += this.Run;
        }

        // アプリのメインループ
        private void Run( object sender, EventArgs e )
        {
            var timer = Stopwatch.StartNew();
            var fps = new MikuMikuFlex3.Utility.FPS();

            this._D3D11Device.ImmediateContext.OutputMerger.BlendState = this._BlendState通常合成;
            this._D3D11Device.ImmediateContext.OutputMerger.DepthStencilState = null;

            SharpDX.Windows.RenderLoop.Run( this, () => {

                if( fps.FPSをカウントする() )
                    Debug.WriteLine( $"{fps.現在のFPS} fps" );

                this._シーン.描画する( timer.ElapsedMilliseconds / 1000.0, this._D3D11Device.ImmediateContext, new SharpDX.Color4( 0.2f, 0.4f, 0.8f, 1.0f ) );

                this._DXGISwapChain.Present( 0, SharpDX.DXGI.PresentFlags.None );

            } );
        }

        // アプリの終了
        protected override void OnClosing( CancelEventArgs e )
        {
            this._アリシア?.Dispose();

            this._シーン?.Dispose();

            this._Direct3Dを解放する();

            base.OnClosing( e );
        }


        // MikuMikuFlex 3

        private MikuMikuFlex3.シーン _シーン;

        private MikuMikuFlex3.PMXモデル _アリシア;

        private MikuMikuFlex3.PMXモデル _舞台;


        // Direct3D11

        private SharpDX.Direct3D11.Device _D3D11Device;

        private SharpDX.DXGI.SwapChain _DXGISwapChain;

        private SharpDX.Direct3D11.Texture2D _既定のD3D11RenderTarget;

        private SharpDX.Direct3D11.Texture2D _既定のD3D11DepthStencil;

        private SharpDX.Direct3D11.BlendState _BlendState通常合成;


        private void _Direct3Dを初期化する()
        {
            // D3DDevice, DXGISwapChain を生成する。

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1 },   // 機能レベル 11.1
                new SharpDX.DXGI.SwapChainDescription {
                    BufferCount = 1,
                    Flags = SharpDX.DXGI.SwapChainFlags.AllowModeSwitch,
                    IsWindowed = true,
                    ModeDescription = new SharpDX.DXGI.ModeDescription {
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Width = this.ClientSize.Width,
                        Height = this.ClientSize.Height,
                        Scaling = SharpDX.DXGI.DisplayModeScaling.Stretched,
                    },
                    OutputHandle = this.Handle,
                    SampleDescription = new SharpDX.DXGI.SampleDescription( 4, 0 ), // MSAA x4
                    SwapEffect = SharpDX.DXGI.SwapEffect.Discard,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                },
                out this._D3D11Device,
                out this._DXGISwapChain );


            // 既定のRenderTarget と 既定のDepthStencil を作成する。

            this._既定のD3D11RenderTarget = this._DXGISwapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>( 0 );

            this._既定のD3D11DepthStencil = new SharpDX.Direct3D11.Texture2D(
                this._D3D11Device,
                new SharpDX.Direct3D11.Texture2DDescription {
                    Width = this._既定のD3D11RenderTarget.Description.Width,
                    Height = this._既定のD3D11RenderTarget.Description.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                    SampleDescription = this._既定のD3D11RenderTarget.Description.SampleDescription,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                } );


            // ブレンドステートを作成する。

            var blendStateNorm = new SharpDX.Direct3D11.BlendStateDescription() {
                AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
            };
            blendStateNorm.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
            blendStateNorm.RenderTarget[ 0 ].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;

            blendStateNorm.RenderTarget[ 0 ].SourceBlend = SharpDX.Direct3D11.BlendOption.SourceAlpha;                  // 色値のブレンディング
            blendStateNorm.RenderTarget[ 0 ].DestinationBlend = SharpDX.Direct3D11.BlendOption.InverseSourceAlpha;
            blendStateNorm.RenderTarget[ 0 ].BlendOperation = SharpDX.Direct3D11.BlendOperation.Add;

            blendStateNorm.RenderTarget[ 0 ].SourceAlphaBlend = SharpDX.Direct3D11.BlendOption.One;                     // アルファ値のブレンディング
            blendStateNorm.RenderTarget[ 0 ].DestinationAlphaBlend = SharpDX.Direct3D11.BlendOption.Zero;
            blendStateNorm.RenderTarget[ 0 ].AlphaBlendOperation = SharpDX.Direct3D11.BlendOperation.Add;

            this._BlendState通常合成 = new SharpDX.Direct3D11.BlendState( this._D3D11Device, blendStateNorm );
        }

        private void _Direct3Dを解放する()
        {
            this._BlendState通常合成?.Dispose();

            this._既定のD3D11DepthStencil?.Dispose();

            this._既定のD3D11RenderTarget?.Dispose();

            this._DXGISwapChain?.Dispose();

            this._D3D11Device?.Dispose();
        }
    }
}
