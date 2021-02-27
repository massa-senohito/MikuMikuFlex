using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     DirectX 準拠のパラメータを使って、アニメーションを適用可能なカメラ。
    /// </summary>
    public class MotionCameraDX : StandardCamera
    {
        public class DXAnimeVariables
        {
            public AnimeVariables<Vector3> Position { get; protected set; }

            public AnimeVariables<Vector3> PositionOfGazingPoint { get; protected set; }

            public AnimeVariables<Vector3> Upward { get; protected set; }

            public AnimeVariables<float> ViewingAngledeg { get; protected set; }

            public AnimeVariables<float> NearZ { get; protected set; }

            public AnimeVariables<float> FarSurfaceZ { get; protected set; }

            public AnimeVariables<float> AspectRatio { get; protected set; }


            // 初期化
            public DXAnimeVariables()
            {
                this.Position = new AnimeVariables<Vector3>( new Vector3( 0f, 0f, -0.5f ) );
                this.PositionOfGazingPoint = new AnimeVariables<Vector3>( new Vector3( 0f, 0f, 0f ) );
                this.Upward = new AnimeVariables<Vector3>( new Vector3( 0f, 1f, 0f ) );
                this.ViewingAngledeg = new AnimeVariables<float>( 40f );
                this.NearZ = new AnimeVariables<float>( 1.0f );
                this.FarSurfaceZ = new AnimeVariables<float>( 200f );
                this.AspectRatio = new AnimeVariables<float>( 1.618f );
            }
        }

        public DXAnimeVariables AnimeVariables { get; protected set; } = new DXAnimeVariables();


        public MotionCameraDX()
            : base()
        {
        }

        /// <summary>
        ///     DirectX 形式による初期化。
        /// </summary>
        public MotionCameraDX( Vector3 InitialPosition, Vector3 InitialGaze, Vector3 InitialUpwardDirection )
            : base( InitialPosition, InitialGaze, InitialUpwardDirection )
        {
        }

    
        public override void Update( double CurrentTimesec )
        {
            this.AnimeVariables.Position.Update( CurrentTimesec );
            this.AnimeVariables.PositionOfGazingPoint.Update( CurrentTimesec );
            this.AnimeVariables.Upward.Update( CurrentTimesec );
            this.AnimeVariables.ViewingAngledeg.Update( CurrentTimesec );
            this.AnimeVariables.NearZ.Update( CurrentTimesec );
            this.AnimeVariables.FarSurfaceZ.Update( CurrentTimesec );
            this.AnimeVariables.AspectRatio.Update( CurrentTimesec );

            this.Position = this.AnimeVariables.Position.Value;
            this.GazePoint = this.AnimeVariables.PositionOfGazingPoint.Value;
            this.Upward = this.AnimeVariables.Upward.Value;
            this.ViewingAngledeg = this.AnimeVariables.ViewingAngledeg.Value;
            this.NearZ = this.AnimeVariables.NearZ.Value;
            this.FarSurfaceZ = this.AnimeVariables.FarSurfaceZ.Value;
            this.AspectRatio = this.AnimeVariables.AspectRatio.Value;
        }
    }
}
