using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     マウスで動かせるカメラ。
    /// </summary>
    public class マウスモーションカメラ : スタンダードカメラ
    {
        public void OnMouseDown( object sender, MouseEventArgs e )
        {
            lock( this._lock排他 )
            {
                if( e.Button == MouseButtons.Right )
                    this._マウスの右ボタンが押されている = true;

                if( e.Button == MouseButtons.Middle )
                    this._マウスの中ボタンが押されている = true;
            }
        }

        public void OnMouseUp( object sender, MouseEventArgs e )
        {
            lock( this._lock排他 )
            {
                if( e.Button == MouseButtons.Right )
                    this._マウスの右ボタンが押されている = false;

                if( e.Button == MouseButtons.Middle )
                    this._マウスの中ボタンが押されている = false;
            }
        }

        public void OnMouseMove( object sender, MouseEventArgs e )
        {
            lock( this._lock排他 )
            {
                int x = e.Location.X - this._マウスの前回の位置.X;
                int y = e.Location.Y - this._マウスの前回の位置.Y;

                if( this._マウスの右ボタンが押されている )
                {
                    this._カメラの注視点を中心とする回転量 *=    // 回転量については累積で記録
                        Quaternion.RotationAxis( _Y軸, マウスの右ボタンの感度 * x ) *
                        Quaternion.RotationAxis( _X軸, マウスの右ボタンの感度 * ( -y ) );

                    this._カメラの注視点を中心とする回転量.Normalize();
                }

                if( this._マウスの中ボタンが押されている )
                {
                    this._カメラの注視点の変形行列 +=    // 変化量を記録しておく
                        new Vector2( x, y ) * マウスの中ボタンの感度;
                }

                this._マウスの前回の位置 = e.Location;
            }
        }

        public void OnMouseWheel( object sender, MouseEventArgs e )
        {
            lock( this._lock排他 )
            {
                if( e.Delta > 0 )
                {
                    this._距離 -= this.マウスホイールの感度;

                    if( this._距離 <= 0 )
                        this._距離 = 0.0001f;
                }
                else
                {
                    this._距離 += this.マウスホイールの感度;
                }
            }
        }


        public float マウスホイールの感度 { get; set; } = 2.0f;

        public float マウスの右ボタンの感度 { get; set; } = 0.005f;

        public float マウスの中ボタンの感度 { get; set; } = 0.01f;


        public マウスモーションカメラ( float 初期距離 )
            : base( 初期距離, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) )
        {
            this._距離 = 初期距離;
            this._カメラの注視点を中心とする回転量 = Quaternion.Identity;
        }

        public override void 更新する( double 現在時刻sec )
        {
            lock( this._lock排他 )
            {
                var camera2la = Vector3.TransformCoordinate(
                    new Vector3( 0, 0, 1 ),
                    Matrix.RotationQuaternion( this._カメラの注視点を中心とする回転量 ) );

                this._X軸 = Vector3.Cross( camera2la, this.上方向 );
                this._X軸.Normalize();

                this._Y軸 = Vector3.Cross( this._X軸, camera2la );
                this._Y軸.Normalize();

                this.注視点 += this._X軸 * this._カメラの注視点の変形行列.X + this._Y軸 * this._カメラの注視点の変形行列.Y;
                this.位置 = this.注視点 + this._距離 * ( -camera2la );

                this._カメラの注視点の変形行列 = Vector2.Zero;
            }
        }


        private float _距離;

        private Vector3 _X軸 = new Vector3( 1, 0, 0 );

        private Vector3 _Y軸 = new Vector3( 0, 1, 0 );

        private Quaternion _カメラの注視点を中心とする回転量;

        private Vector2 _カメラの注視点の変形行列;

        private bool _マウスの右ボタンが押されている;

        private bool _マウスの中ボタンが押されている;

        private System.Drawing.Point _マウスの前回の位置;

        private readonly object _lock排他 = new object();
    }
}
