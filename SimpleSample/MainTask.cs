using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MikuMikuFlex3;

namespace SimpleSample
{
    class MainTask
    {
        public ManualResetEventSlim 終了指示通知 = new ManualResetEventSlim( false );
        public ManualResetEventSlim 終了完了通知 = new ManualResetEventSlim( false );

        public MainTask( Control parent, string pmxファイルパス )
        {
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

            this._PMXモデル = new PMXモデル( this._D3D11Device, pmxファイルパス );
        }

        public void Dispose()
        {
            this._PMXモデル?.Dispose();

            this._DXGISwapChain?.Dispose();
            this._D3D11Device?.Dispose();
        }

        public void MainLoop()
        {
            while( !this.終了指示通知.IsSet )
            {
                this._PMXモデル.進行する();
                this._PMXモデル.描画する( this._D3D11Device.ImmediateContext );

                this._DXGISwapChain.Present( 1, SharpDX.DXGI.PresentFlags.None );
            }

            this.Dispose();
            this.終了完了通知.Set();
        }


        private PMXモデル _PMXモデル;

        private SharpDX.Direct3D11.Device _D3D11Device;

        private SharpDX.DXGI.SwapChain _DXGISwapChain;
    }
}
