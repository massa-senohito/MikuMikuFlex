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
    public class MouseMotionCamera : StandardCamera
    {
        public void OnMouseDown( object sender, MouseEventArgs e )
        {
            lock( this._lockExclusive )
            {
                if( e.Button == MouseButtons.Right )
                    this.RightMouseButtonIsPressed = true;

                if( e.Button == MouseButtons.Middle )
                    this.MiddleMouseButtonIsPressed = true;
            }
        }

        public void OnMouseUp( object sender, MouseEventArgs e )
        {
            lock( this._lockExclusive )
            {
                if( e.Button == MouseButtons.Right )
                    this.RightMouseButtonIsPressed = false;

                if( e.Button == MouseButtons.Middle )
                    this.MiddleMouseButtonIsPressed = false;
            }
        }

        public void OnMouseMove( object sender, MouseEventArgs e )
        {
            lock( this._lockExclusive )
            {
                int x = e.Location.X - this._PreviousPositionOfMouse.X;
                int y = e.Location.Y - this._PreviousPositionOfMouse.Y;

                if( this.RightMouseButtonIsPressed )
                {
                    this._AmountOfRotationAroundTheGazingPointOfTheCamera *=    // 回転量については累積で記録
                        Quaternion.RotationAxis( _YAxis, RightMouseButtonSensitivity * x ) *
                        Quaternion.RotationAxis( _XAxis, RightMouseButtonSensitivity * ( -y ) );

                    this._AmountOfRotationAroundTheGazingPointOfTheCamera.Normalize();
                }

                if( this.MiddleMouseButtonIsPressed )
                {
                    this._DeformationMatrixOfTheCamerasGazePoint +=    // 変化量を記録しておく
                        new Vector2( x, y ) * MouseMiddleButtonSensitivity;
                }

                this._PreviousPositionOfMouse = e.Location;
            }
        }

        public void OnMouseWheel( object sender, MouseEventArgs e )
        {
            lock( this._lockExclusive )
            {
                if( e.Delta > 0 )
                {
                    this._Distance -= this.MouseWheelSensitivity;

                    if( this._Distance <= 0 )
                        this._Distance = 0.0001f;
                }
                else
                {
                    this._Distance += this.MouseWheelSensitivity;
                }
            }
        }


        public float MouseWheelSensitivity { get; set; } = 2.0f;

        public float RightMouseButtonSensitivity { get; set; } = 0.005f;

        public float MouseMiddleButtonSensitivity { get; set; } = 0.01f;


        public MouseMotionCamera( float InitialDistance )
            : base( InitialDistance, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) )
        {
            this._Distance = InitialDistance;
            this._AmountOfRotationAroundTheGazingPointOfTheCamera = Quaternion.Identity;
        }

        public override void Update( double CurrentTimesec )
        {
            lock( this._lockExclusive )
            {
                var camera2la = Vector3.TransformCoordinate(
                    new Vector3( 0, 0, 1 ),
                    Matrix.RotationQuaternion( this._AmountOfRotationAroundTheGazingPointOfTheCamera ) );

                this._XAxis = Vector3.Cross( camera2la, this.Upward );
                this._XAxis.Normalize();

                this._YAxis = Vector3.Cross( this._XAxis, camera2la );
                this._YAxis.Normalize();

                this.GazePoint += this._XAxis * this._DeformationMatrixOfTheCamerasGazePoint.X + this._YAxis * this._DeformationMatrixOfTheCamerasGazePoint.Y;
                this.Position = this.GazePoint + this._Distance * ( -camera2la );

                this._DeformationMatrixOfTheCamerasGazePoint = Vector2.Zero;
            }
        }


        private float _Distance;

        private Vector3 _XAxis = new Vector3( 1, 0, 0 );

        private Vector3 _YAxis = new Vector3( 0, 1, 0 );

        private Quaternion _AmountOfRotationAroundTheGazingPointOfTheCamera;

        private Vector2 _DeformationMatrixOfTheCamerasGazePoint;

        private bool RightMouseButtonIsPressed;

        private bool MiddleMouseButtonIsPressed;

        private System.Drawing.Point _PreviousPositionOfMouse;

        private readonly object _lockExclusive = new object();
    }
}
