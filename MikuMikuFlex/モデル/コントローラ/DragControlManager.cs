using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex.モデル.コントローラ
{
	class DragControlManager
	{
        public bool IsDragging { get; private set; }

        public Vector2 Delta { get; private set; }


        public DragControlManager( ILockableController locker )
		{
			this._locker = locker;
		}

        public bool checkNeedHighlight( bool result )
		{
			return IsDragging || ( result && !_locker.ロック中 );
		}

		public void checkBegin( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			if( result && _lastState && !_lastMouseState && mouseState && !IsDragging && !_locker.ロック中 )
			{
				_locker.ロック中 = true;
				IsDragging = true;
			}
			Delta = new Vector2( mousePosition.X - _lastPoint.X, mousePosition.Y - _lastPoint.Y );
		}

		public void checkEnd( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			if( !mouseState && IsDragging )
			{
				_locker.ロック中 = false;
				IsDragging = false;
			}

			_lastState = result;
			_lastMouseState = mouseState;
			_lastPoint = mousePosition;
		}


        private bool _lastState;

        private bool _lastMouseState;

        private System.Drawing.Point _lastPoint;

        private ILockableController _locker;
    }
}
