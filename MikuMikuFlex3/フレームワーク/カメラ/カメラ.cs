using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class カメラ
    {

        // ビュー関連


        public Vector3 位置 { get; set; } = new Vector3( 0f, 0f, -0.5f );

        public Vector3 注視点 { get; set; } = new Vector3( 0f, 0f, 0f );

        public Vector3 上方向 { get; set; } = new Vector3( 0f, 1f, 0f );



        // 射影関連


        public float 視野角deg { get; set; } = 30f;

        public float 近面Z { get; set; } = 1f;

        public float 遠面Z { get; set; } = 200f;

        public float アスペクト比 { get; set; } = 1.618f;



        // 生成と終了


        public カメラ()
        {
            this.VMD方式で更新する( 45.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, 0f, 0f ) );    // MMD での初期値。
            //this.VMD方式で更新する( 50.0f, new Vector3( 0f, 10f, 0f ), new Vector3( 0f, 0f, 0f ) );    // MMM での初期値。
        }

        /// <summary>
        ///     DirectX 形式による初期化。
        /// </summary>
        public カメラ( Vector3 初期位置, Vector3 初期注視点, Vector3 初期上方向 )
            : this()
        {
            this.位置 = 初期位置;
            this.注視点 = 初期注視点;
            this.上方向 = 初期上方向;
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public カメラ( float 注視点からの距離, Vector3 注視点の位置, Quaternion 回転 )
            : this()
        {
            this.VMD方式で更新する( 注視点からの距離, 注視点の位置, 回転 );
        }

        /// <summary>
        ///     MMD 方式による初期化。
        /// </summary>
        public カメラ( float 注視点からの距離, Vector3 注視点の位置, Vector3 回転rad )
            : this( 注視点からの距離, 注視点の位置, Quaternion.RotationYawPitchRoll( 回転rad.Y, 回転rad.X, 回転rad.Z ) )
        {
        }



        // 行列の取得


        /// <summary>
        ///     現在の位置、注視点、上方向から得られるビュー行列を返す。
        /// </summary>
        public Matrix ビュー行列を取得する()
        {
            return Matrix.LookAtLH( this.位置, this.注視点, this.上方向 );
        }

        /// <summary>
        ///     現在の視野角、近面/遠面Z、アスペクト比から得られる射影行列を返す。
        /// </summary>
        public Matrix 射影行列を取得する()
        {
            return Matrix.PerspectiveFovLH(
                MathUtil.Pi * this.視野角deg / 180.0f,
                this.アスペクト比,
                this.近面Z,
                this.遠面Z );
        }



        // VMD互換


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
