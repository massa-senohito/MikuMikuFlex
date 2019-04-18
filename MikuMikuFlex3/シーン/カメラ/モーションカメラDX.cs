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
    public class モーションカメラDX : カメラ
    {
        public class DXアニメ変数
        {
            public アニメ変数<Vector3> 位置 { get; protected set; }

            public アニメ変数<Vector3> 注視点の位置 { get; protected set; }

            public アニメ変数<Vector3> 上方向 { get; protected set; }

            public アニメ変数<float> 視野角deg { get; protected set; }

            public アニメ変数<float> 近面Z { get; protected set; }

            public アニメ変数<float> 遠面Z { get; protected set; }

            public アニメ変数<float> アスペクト比 { get; protected set; }


            // 初期化
            public DXアニメ変数()
            {
                this.位置 = new アニメ変数<Vector3>( new Vector3( 0f, 0f, -0.5f ) );
                this.注視点の位置 = new アニメ変数<Vector3>( new Vector3( 0f, 0f, 0f ) );
                this.上方向 = new アニメ変数<Vector3>( new Vector3( 0f, 1f, 0f ) );
                this.視野角deg = new アニメ変数<float>( 40f );
                this.近面Z = new アニメ変数<float>( 1.0f );
                this.遠面Z = new アニメ変数<float>( 200f );
                this.アスペクト比 = new アニメ変数<float>( 1.618f );
            }
        }

        public DXアニメ変数 アニメ変数 { get; protected set; } = new DXアニメ変数();


        public モーションカメラDX()
            : base()
        {
        }

        /// <summary>
        ///     DirectX 形式による初期化。
        /// </summary>
        public モーションカメラDX( Vector3 初期位置, Vector3 初期注視点, Vector3 初期上方向 )
            : base( 初期位置, 初期注視点, 初期上方向 )
        {
        }

    
        public override void 更新する( double 現在時刻sec )
        {
            this.アニメ変数.位置.更新する( 現在時刻sec );
            this.アニメ変数.注視点の位置.更新する( 現在時刻sec );
            this.アニメ変数.上方向.更新する( 現在時刻sec );
            this.アニメ変数.視野角deg.更新する( 現在時刻sec );
            this.アニメ変数.近面Z.更新する( 現在時刻sec );
            this.アニメ変数.遠面Z.更新する( 現在時刻sec );
            this.アニメ変数.アスペクト比.更新する( 現在時刻sec );

            this.位置 = this.アニメ変数.位置.値;
            this.注視点 = this.アニメ変数.注視点の位置.値;
            this.上方向 = this.アニメ変数.上方向.値;
            this.視野角deg = this.アニメ変数.視野角deg.値;
            this.近面Z = this.アニメ変数.近面Z.値;
            this.遠面Z = this.アニメ変数.遠面Z.値;
            this.アスペクト比 = this.アニメ変数.アスペクト比.値;
        }
    }
}
