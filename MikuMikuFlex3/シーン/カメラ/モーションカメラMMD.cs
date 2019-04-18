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
    public class モーションカメラMMD : カメラ
    {
        public class MMDアニメ変数
        {
            public アニメ変数<float> 注視点からの距離 { get; protected set; }

            public アニメ変数<Vector3> 注視点の位置 { get; protected set; }

            public アニメ変数<Vector3> 回転rad { get; protected set; }

            public アニメ変数<float> 視野角deg { get; protected set; }

            public アニメ変数<float> 近面Z { get; protected set; }

            public アニメ変数<float> 遠面Z { get; protected set; }

            public アニメ変数<float> アスペクト比 { get; protected set; }


            // 初期化
            public MMDアニメ変数()
            {
                this.注視点からの距離 = new アニメ変数<float>( 45f );
                this.注視点の位置 = new アニメ変数<Vector3>( new Vector3( 0f, 10f, 0f ) );
                this.回転rad = new アニメ変数<Vector3>( new Vector3( 0f, MathUtil.Pi, 0f ) );
                this.視野角deg = new アニメ変数<float>( 40f );
                this.近面Z = new アニメ変数<float>( 1.0f );
                this.遠面Z = new アニメ変数<float>( 200f );
                this.アスペクト比 = new アニメ変数<float>( 1.618f );
            }
        }

        public MMDアニメ変数 アニメ変数 { get; protected set; } = new MMDアニメ変数();


        public モーションカメラMMD()
            : base()
        {
        }

        /// <summary>
        ///     MMD 形式による初期化。
        /// </summary>
        public モーションカメラMMD( float 注視点からの初期距離, Vector3 初期注視点, Vector3 初期回転rad )
            : base( 注視点からの初期距離, 初期注視点, 初期回転rad )
        {
            this.アニメ変数.注視点からの距離.値 = 注視点からの初期距離;
            this.アニメ変数.注視点の位置.値 = 初期注視点;
            this.アニメ変数.回転rad.値 = 初期回転rad;
        }


        public override void 更新する( double 現在時刻sec )
        {
            this.アニメ変数.注視点からの距離.更新する( 現在時刻sec );
            this.アニメ変数.注視点の位置.更新する( 現在時刻sec );
            this.アニメ変数.回転rad.更新する( 現在時刻sec );
            this.アニメ変数.視野角deg.更新する( 現在時刻sec );
            this.アニメ変数.近面Z.更新する( 現在時刻sec );
            this.アニメ変数.遠面Z.更新する( 現在時刻sec );
            this.アニメ変数.アスペクト比.更新する( 現在時刻sec );

            this.VMD方式で更新する( this.アニメ変数.注視点からの距離.値, this.アニメ変数.注視点の位置.値, this.アニメ変数.回転rad.値 );

            this.視野角deg = this.アニメ変数.視野角deg.値;
            this.近面Z = this.アニメ変数.近面Z.値;
            this.遠面Z = this.アニメ変数.遠面Z.値;
            this.アスペクト比 = this.アニメ変数.アスペクト比.値;
        }
    }
}
