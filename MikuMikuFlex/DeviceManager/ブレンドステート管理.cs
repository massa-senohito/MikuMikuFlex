using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.DeviceManager
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
                AlphaToCoverageEnable = true,
                IndependentBlendEnable = true
            };

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                AlphaBlendOperation = BlendOperation.Add,
                DestinationBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.Zero,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.One,
                SourceAlphaBlend = BlendOption.One,
            };
            var disableState = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Disable, disableState );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                AlphaBlendOperation = BlendOperation.Add,
                DestinationBlend = BlendOption.InverseDestinationAlpha,
                DestinationAlphaBlend = BlendOption.InverseDestinationAlpha,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceAlphaBlend = BlendOption.SourceAlpha
            };
            var alignment = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Alignment, alignment );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                AlphaBlendOperation = BlendOperation.Add,
                DestinationBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceAlphaBlend = BlendOption.SourceAlpha
            };
            var add = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Add, add );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.ReverseSubtract,
                AlphaBlendOperation = BlendOperation.ReverseSubtract,
                DestinationBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceAlphaBlend = BlendOption.SourceAlpha
            };
            var rsubtract = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.ReverseSubtruct, rsubtract );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Subtract,
                AlphaBlendOperation = BlendOperation.Subtract,
                DestinationBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.SourceAlpha,
                SourceAlphaBlend = BlendOption.SourceAlpha
            };
            var subtruct = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Subtruct, subtruct );

            defaultDesc.RenderTarget[ 0 ] = new RenderTargetBlendDescription() {
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                AlphaBlendOperation = BlendOperation.Add,
                DestinationBlend = BlendOption.SourceColor,
                DestinationAlphaBlend = BlendOption.SourceAlpha,
                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                SourceBlend = BlendOption.Zero,
                SourceAlphaBlend = BlendOption.Zero
            };
            var multiply = new BlendState( device, defaultDesc );
            _ブレンドステートマップ.Add( BlendStates.Multiply, multiply );
        }


        private Dictionary<BlendStates, BlendState> _ブレンドステートマップ = new Dictionary<BlendStates, BlendState>();
    }
}
