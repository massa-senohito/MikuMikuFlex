using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     MMD 準拠のパラメータを使って、アニメーションを適用可能なカメラ。
    /// </summary>
    public class MotionCameraMMD : StandardCamera
    {
        public class MMDAnimeVariables
        {
            public AnimeVariables<float> DistanceFromTheGazingPoint { get; protected set; }

            public AnimeVariables<Vector3> PositionOfGazingPoint { get; protected set; }

            public AnimeVariables<Vector3> Rotationrad { get; protected set; }

            public AnimeVariables<float> ViewingAngledeg { get; protected set; }

            public AnimeVariables<float> NearZ { get; protected set; }

            public AnimeVariables<float> FarSurfaceZ { get; protected set; }

            public AnimeVariables<float> AspectRatio { get; protected set; }


            // 初期化
            public MMDAnimeVariables()
            {
                this.DistanceFromTheGazingPoint = new AnimeVariables<float>( 45f );
                this.PositionOfGazingPoint = new AnimeVariables<Vector3>( new Vector3( 0f, 10f, 0f ) );
                this.Rotationrad = new AnimeVariables<Vector3>( new Vector3( 0f, MathUtil.Pi, 0f ) );
                this.ViewingAngledeg = new AnimeVariables<float>( 40f );
                this.NearZ = new AnimeVariables<float>( 1.0f );
                this.FarSurfaceZ = new AnimeVariables<float>( 200f );
                this.AspectRatio = new AnimeVariables<float>( 1.618f );
            }
        }

        public MMDAnimeVariables AnimeVariables { get; protected set; } = new MMDAnimeVariables();


        public MotionCameraMMD()
            : base()
        {
        }

        /// <summary>
        ///     MMD 形式による初期化。
        /// </summary>
        public MotionCameraMMD( float InitialDistanceFromTheGazingPoint, Vector3 InitialGaze, Vector3 InitialRotationrad )
            : base( InitialDistanceFromTheGazingPoint, InitialGaze, InitialRotationrad )
        {
            this.AnimeVariables.DistanceFromTheGazingPoint.Value = InitialDistanceFromTheGazingPoint;
            this.AnimeVariables.PositionOfGazingPoint.Value = InitialGaze;
            this.AnimeVariables.Rotationrad.Value = InitialRotationrad;
        }


        public override void Update( double CurrentTimesec )
        {
            this.AnimeVariables.DistanceFromTheGazingPoint.Update( CurrentTimesec );
            this.AnimeVariables.PositionOfGazingPoint.Update( CurrentTimesec );
            this.AnimeVariables.Rotationrad.Update( CurrentTimesec );
            this.AnimeVariables.ViewingAngledeg.Update( CurrentTimesec );
            this.AnimeVariables.NearZ.Update( CurrentTimesec );
            this.AnimeVariables.FarSurfaceZ.Update( CurrentTimesec );
            this.AnimeVariables.AspectRatio.Update( CurrentTimesec );

            this.VMDUpdateByMethod( this.AnimeVariables.DistanceFromTheGazingPoint.Value, this.AnimeVariables.PositionOfGazingPoint.Value, this.AnimeVariables.Rotationrad.Value );

            this.ViewingAngledeg = this.AnimeVariables.ViewingAngledeg.Value;
            this.NearZ = this.AnimeVariables.NearZ.Value;
            this.FarSurfaceZ = this.AnimeVariables.FarSurfaceZ.Value;
            this.AspectRatio = this.AnimeVariables.AspectRatio.Value;
        }
    }
}
