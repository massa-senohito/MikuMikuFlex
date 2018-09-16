using System.Windows.Forms;
using MikuMikuFlex.モーション;
using SharpDX;

namespace MikuMikuFlex
{
	/// <summary>
	/// 主にマウスなどのパネル上の動きを監視するクラス
	/// MMEエフェクトのため
	/// </summary>
	public class マウス監視
	{
        /// <summary>
        ///     マウスの現在位置。
        ///     (-1, -1) ～ (+1, +1) で正規化される。
        /// </summary>
		public Vector2 MousePosition { get; private set; }

		public Vector4 LeftMouseDown { get; private set; }

		public Vector4 MiddleMouseDown { get; private set; }

		public Vector4 RightMouseDown { get; private set; }

		public bool MMEのマウス機能が有効である { get; set; }

		public マウス監視( Control control )
		{
			_監視対象 = control;

			MMEのマウス機能が有効である = false;

			control.MouseMove += MouseHandler;
			control.MouseDown += MouseHandler;
		}

		void MouseHandler( object sender, System.Windows.Forms.MouseEventArgs e )
		{
			if( MMEのマウス機能が有効である )
			{
                // MousePosition 算出
                float x = 0, y = 0;
                if( e.X != 0 )
				{
					x = (float) e.X * 2f / (float) _監視対象.Width - 1f;    // -1～+1 に正規化
				}
				if( e.Y != 0 )
				{
					y = (float) e.Y * 2f / (float) _監視対象.Height - 1f;   // -1～+1 に正規化
				}
				MousePosition = new Vector2( x, y );

                float leftT = LeftMouseDown.W;
                float middleT = MiddleMouseDown.W;
                float rightT = RightMouseDown.W;
                float leftP = 0f;
                float middleP = 0f;
                float rightP = 0f;
                if( e.Button.HasFlag( MouseButtons.Left ) )
				{
					leftT = モーションタイマ.stopWatch.ElapsedMilliseconds / 1000f;
					leftP = 1;
				}
				if( e.Button.HasFlag( MouseButtons.Middle ) )
				{
					middleT = モーションタイマ.stopWatch.ElapsedMilliseconds / 1000f;
					middleP = 1;
				}
				if( e.Button.HasFlag( MouseButtons.Right ) )
				{
					rightT = モーションタイマ.stopWatch.ElapsedMilliseconds / 1000f;
					rightP = 1;
				}
				LeftMouseDown = new Vector4( x, y, leftP, leftT );
				MiddleMouseDown = new Vector4( x, y, middleP, middleT );
				RightMouseDown = new Vector4( x, y, rightP, rightT );
			}
		}


        private Control _監視対象 { get; set; }
    }
}
