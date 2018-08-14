using System.Windows.Forms;
using SharpDX;

namespace MMF.行列.CameraMotion
{
    /// <summary>
    ///     マウスでカメラを移動できる。
    /// </summary>
	public class マウスカメラモーション : カメラモーション
    {
        public float マウスホイールの感度 { get; set; }

        public float マウスの右ボタンの感度 { get; set; }

        public float マウスの中ボタンの感度 { get; set; }


        public マウスカメラモーション( Control コントロール, Control ホイール取得コントロール, float 初期距離 = 45f )
        {
            _距離 = 初期距離;
            _カメラの注視点を中心とする回転量 = Quaternion.Identity;

            コントロール.MouseDown += _コントロール_MouseDown;
            コントロール.MouseMove += _コントロール_MouseMove;
            コントロール.MouseUp += _コントロール_MouseUp;

            ホイール取得コントロール.MouseWheel += _ホイール取得コントロール_MouseWheel;

            this.マウスホイールの感度 = 2.0f;
            this.マウスの右ボタンの感度 = 0.005f;
            this.マウスの中ボタンの感度 = 0.01f;
        }

        void カメラモーション.モーションを更新する( カメラ cp1, 射影 proj )
        {
            var camera2la = Vector3.TransformCoordinate(
                new Vector3( 0, 0, 1 ),
                Matrix.RotationQuaternion( _カメラの注視点を中心とする回転量 ) );

            _X軸 = Vector3.Cross( camera2la, cp1.カメラの上方向ベクトル );
            _X軸.Normalize();

            _Y軸 = Vector3.Cross( _X軸, camera2la );
            _Y軸.Normalize();

            cp1.カメラの注視点 += _X軸 * _カメラの注視点の変形行列.X + _Y軸 * _カメラの注視点の変形行列.Y;

            cp1.カメラの位置 = cp1.カメラの注視点 + _距離 * ( -camera2la );

            _カメラの注視点の変形行列 = Vector2.Zero;
        }


        private Vector3 _X軸 = new Vector3( 1, 0, 0 );

        private Vector3 _Y軸 = new Vector3( 0, 1, 0 );

        private System.Drawing.Point _マウスの前回の位置 { get; set; }

        private bool _マウスの右ボタンが押されている { get; set; }

        private bool _マウスの中ボタンが押されている { get; set; }

        private Quaternion _カメラの注視点を中心とする回転量 { get; set; }

        private Vector2 _カメラの注視点の変形行列 { get; set; }

        private float _距離 { get; set; }


        private void _コントロール_MouseMove( object sender, MouseEventArgs e )
        {
            int x = e.Location.X - _マウスの前回の位置.X;
            int y = e.Location.Y - _マウスの前回の位置.Y;

            if( _マウスの右ボタンが押されている )
            {
                _カメラの注視点を中心とする回転量 *=    // 回転量については累積で記録
                    Quaternion.RotationAxis( _Y軸, マウスの右ボタンの感度 * x ) *
                    Quaternion.RotationAxis( _X軸, マウスの右ボタンの感度 * ( -y ) );

                _カメラの注視点を中心とする回転量.Normalize();
            }

            if( _マウスの中ボタンが押されている )
            {
                _カメラの注視点の変形行列 +=    // 変化量を記録しておく
                    new Vector2( x, y ) * マウスの中ボタンの感度;
            }

            _マウスの前回の位置 = e.Location;
        }

        private void _コントロール_MouseUp( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Right )
                _マウスの右ボタンが押されている = false;

            if( e.Button == MouseButtons.Middle )
                _マウスの中ボタンが押されている = false;
        }

        private void _コントロール_MouseDown( object sender, MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Right )
                _マウスの右ボタンが押されている = true;

            if( e.Button == MouseButtons.Middle )
                _マウスの中ボタンが押されている = true;
        }

        private void _ホイール取得コントロール_MouseWheel( object sender, MouseEventArgs e )
        {
            if( e.Delta > 0 )
            {
                _距離 -= マウスホイールの感度;

                if( _距離 <= 0 )
                    _距離 = 0.0001f;
            }
            else
            {
                _距離 += マウスホイールの感度;
            }
        }
    }
}

