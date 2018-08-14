using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.モデル.Shape.Overlay;
using SharpDX;

namespace MMF.モデル.Controller.ControllerComponent
{
	class ScalingCubeController : OverlayCubeShape
	{
		public event EventHandler<ScalingChangedEventArgs> OnScalingChanged = delegate { };


		public ScalingCubeController( ILockableController parent, Vector4 color, Vector4 overlayColor )
            : base( color, overlayColor )
		{
			_dragController = new DragControlManager( parent );
		}

		public override void HitTestResult( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			base.HitTestResult( _dragController.checkNeedHighlight( result ), mouseState, mousePosition );
			_dragController.checkBegin( result, mouseState, mousePosition );
			if( _dragController.IsDragging )
			{
				OnScalingChanged?.Invoke( this, new ScalingChangedEventArgs( _dragController.Delta.X / 10f ) );
			}
			_dragController.checkEnd( result, mouseState, mousePosition );
		}

		public class ScalingChangedEventArgs : EventArgs
		{
            public float Delta { get; }

            public ScalingChangedEventArgs( float delta )
			{
				this.Delta = delta;
			}
		}


        private DragControlManager _dragController;
    }
}
