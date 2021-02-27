using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     ビュー変換行列と射影変換行列を、いくつかのパラメータから生成するカメラ。
    /// </summary>
    public class StandardCamera : Camera
    {

        // ビュー関連


        public Vector3 Position { get; set; } = new Vector3( 0f, 0f, -0.5f );

        public Vector3 GazePoint { get; set; } = new Vector3( 0f, 0f, 0f );

        public Vector3 Upward { get; set; } = new Vector3( 0f, 1f, 0f );

        public override Matrix ViewTransformationMatrix
        {
            get
            {
                base.ViewTransformationMatrix = Matrix.LookAtLH( this.Position, this.GazePoint, this.Upward );

                return base.ViewTransformationMatrix;
            }
        }



        // 射影関連


        public float ViewingAngledeg { get; set; } = 30f;

        public float NearZ { get; set; } = 1f;

        public float FarSurfaceZ { get; set; } = 200f;

        public float AspectRatio { get; set; } = 1.618f;

        public override Matrix HomographicTransformationMatrix
        {
            get
            {
                base.HomographicTransformationMatrix = this.HomographicTransformationMatrix = Matrix.PerspectiveFovLH(
                    MathUtil.Pi * this.ViewingAngledeg / 180.0f,
                    this.AspectRatio,
                    this.NearZ,
                    this.FarSurfaceZ );

                return base.HomographicTransformationMatrix;
            }
        }



        // 生成と終了


        public StandardCamera()
        {
            this.VMDUpdateByMethod( 45.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) );    // MMD での初期値。
            //this.VMDUpdateByMethod( 50.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) );    // MMM での初期値。
        }

        /// <summary>
        ///     DirectX 形式による初期化。
        /// </summary>
        public StandardCamera( Vector3 InitialPosition, Vector3 InitialGaze, Vector3 InitialUpwardDirection )
            : this()
        {
            this.Position = InitialPosition;
            this.GazePoint = InitialGaze;
            this.Upward = InitialUpwardDirection;
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public StandardCamera( float DistanceFromTheGazingPoint, Vector3 PositionOfGazingPoint, Quaternion Rotation )
            : this()
        {
            this.VMDUpdateByMethod( DistanceFromTheGazingPoint, PositionOfGazingPoint, Rotation );
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public StandardCamera( float DistanceFromTheGazingPoint, Vector3 PositionOfGazingPoint, Vector3 Rotationrad )
            : this( DistanceFromTheGazingPoint, PositionOfGazingPoint, Quaternion.RotationYawPitchRoll( Rotationrad.Y, Rotationrad.X, Rotationrad.Z ) )
        {
        }



        // 更新


        public override void Update( double CurrentTimesec )
        {
        }

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        public void VMDUpdateByMethod( float DistanceFromTheGazingPoint, Vector3 PositionOfGazingPoint, Quaternion Rotation )
        {
            this.GazePoint = PositionOfGazingPoint;
            this.Position = this.GazePoint + DistanceFromTheGazingPoint *
                Vector3.TransformCoordinate( new Vector3( 0, 0, 1 ), Matrix.RotationQuaternion( Rotation ) ); // カメラのある方向
            this.Upward = Vector3.TransformCoordinate( new Vector3( 0, 1, 0 ), Matrix.RotationQuaternion( Rotation ) );
        }

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        /// <param name="Rotationrad">
        ///     ラジアン単位。VMDファイルの値はすべて符号を反転する必要があるので注意。
        /// </param>
        public void VMDUpdateByMethod( float DistanceFromTheGazingPoint, Vector3 PositionOfGazingPoint, Vector3 Rotationrad )
            => this.VMDUpdateByMethod( DistanceFromTheGazingPoint, PositionOfGazingPoint, Quaternion.RotationYawPitchRoll( Rotationrad.Y, Rotationrad.X, Rotationrad.Z ) );
    }
}
