using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.DeviceManagement
{
    public class ブレンドステート管理 : IDisposable
    {
        public enum BlendStates
        {
            Disable,
            Alignment,
            Add,
            ReverseSubtruct,
            Subtruct,
            Multiply
        }

        public ブレンドステート管理( RenderContext context )
        {
            context.Disposables.Add( this );

            ブレンドステートを生成する( context.DeviceManager.D3DDevice );
        }

        public void ブレンドステートを設定する( DeviceContext deviceContext, BlendStates state )
        {
            deviceContext.OutputMerger.BlendState = _ブレンドステートマップ[ state ];
        }

        public void Dispose()
        {
            foreach( var blendingState in _ブレンドステートマップ )
            {
                if( blendingState.Value != null && !blendingState.Value.IsDisposed )
                    blendingState.Value.Dispose();
            }
        }


        protected virtual void ブレンドステートを生成する( Device device )
        {
            var defaultDesc = new BlendStateDescription() {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = true
            };

            _ブレンドステートマップ.Add( BlendStates.Disable, null );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                // RGB = (Rs,Gs,Bs)×As + (Rd,Gd,Bd)×(1-As)
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                // A = As
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
            };
            var alignment = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Alignment, alignment );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                // RGB = (Rs,Gs,Bs)×As + (Rd,Gd,Bd)
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                // A = As
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
            };
            var add = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Add, add );

            // ここまで修正完了。
            // TODO: 以下の設定が正しいか確認する。

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.ReverseSubtract,
            };
            var rsubtract = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.ReverseSubtruct, rsubtract );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Subtract,
                SourceAlphaBlend = BlendOption.SourceAlpha,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Subtract,
            };
            var subtruct = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Subtruct, subtruct );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.SourceColor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.SourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
            };
            var multiply = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Multiply, multiply );
        }


        private Dictionary<BlendStates, BlendState> _ブレンドステートマップ = new Dictionary<BlendStates, BlendState>();
    }
}
