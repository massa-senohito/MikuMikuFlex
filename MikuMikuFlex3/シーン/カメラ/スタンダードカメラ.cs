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
    public class スタンダードカメラ : カメラ
    {

        // ビュー関連


        public Vector3 位置 { get; set; } = new Vector3( 0f, 0f, -0.5f );

        public Vector3 注視点 { get; set; } = new Vector3( 0f, 0f, 0f );

        public Vector3 上方向 { get; set; } = new Vector3( 0f, 1f, 0f );

        public override Matrix ビュー変換行列
        {
            get
            {
                base.ビュー変換行列 = Matrix.LookAtLH( this.位置, this.注視点, this.上方向 );

                return base.ビュー変換行列;
            }
        }



        // 射影関連


        public float 視野角deg { get; set; } = 30f;

        public float 近面Z { get; set; } = 1f;

        public float 遠面Z { get; set; } = 200f;

        public float アスペクト比 { get; set; } = 1.618f;

        public override Matrix 射影変換行列
        {
            get
            {
                base.射影変換行列 = this.射影変換行列 = Matrix.PerspectiveFovLH(
                    MathUtil.Pi * this.視野角deg / 180.0f,
                    this.アスペクト比,
                    this.近面Z,
                    this.遠面Z );

                return base.射影変換行列;
            }
        }



        // 生成と終了


        public スタンダードカメラ()
        {
            this.VMD方式で更新する( 45.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) );    // MMD での初期値。
            //this.VMD方式で更新する( 50.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, MathUtil.Pi, 0f ) );    // MMM での初期値。
        }

        /// <summary>
        ///     DirectX 形式による初期化。
        /// </summary>
        public スタンダードカメラ( Vector3 初期位置, Vector3 初期注視点, Vector3 初期上方向 )
            : this()
        {
            this.位置 = 初期位置;
            this.注視点 = 初期注視点;
            this.上方向 = 初期上方向;
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public スタンダードカメラ( float 注視点からの距離, Vector3 注視点の位置, Quaternion 回転 )
            : this()
        {
            this.VMD方式で更新する( 注視点からの距離, 注視点の位置, 回転 );
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public スタンダードカメラ( float 注視点からの距離, Vector3 注視点の位置, Vector3 回転rad )
            : this( 注視点からの距離, 注視点の位置, Quaternion.RotationYawPitchRoll( 回転rad.Y, 回転rad.X, 回転rad.Z ) )
        {
        }



        // 更新


        public override void 更新する( double 現在時刻sec )
        {
        }

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        public void VMD方式で更新する( float 注視点からの距離, Vector3 注視点の位置, Quaternion 回転 )
        {
            this.注視点 = 注視点の位置;
            this.位置 = this.注視点 + 注視点からの距離 *
                Vector3.TransformCoordinate( new Vector3( 0, 0, 1 ), Matrix.RotationQuaternion( 回転 ) ); // カメラのある方向
            this.上方向 = Vector3.TransformCoordinate( new Vector3( 0, 1, 0 ), Matrix.RotationQuaternion( 回転 ) );
        }

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        /// <param name="回転rad">
        ///     ラジアン単位。VMDファイルの値はすべて符号を反転する必要があるので注意。
        /// </param>
        public void VMD方式で更新する( float 注視点からの距離, Vector3 注視点の位置, Vector3 回転rad )
            => this.VMD方式で更新する( 注視点からの距離, 注視点の位置, Quaternion.RotationYawPitchRoll( 回転rad.Y, 回転rad.X, 回転rad.Z ) );
    }
}
