using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.VMD
{
    public class カメラフレーム : IComparer<カメラフレーム>
    {
        public uint フレーム番号;

		/// <summary>
		///		目標点とカメラの距離
		///		目標点がカメラの前にあるとき負数
		/// </summary>
        public float 距離;

		/// <summary>
		///		目標点の位置
		/// </summary>
        public Vector3 位置;

		/// <summary>
		///		カメラの回転量[ラジアン]
		///		X, Y, Z 軸回転。
		/// </summary>
        public Vector3 回転;

        /// <summary>
        ///     X軸, Y軸, Z軸, 回転, 距離, 視野角 のベジェ補間曲線。
        ///     [6][4] の 24bytes。
        /// </summary>
        /// <remarks>
        ///     補間パラメータは4点のベジェ曲線(0,0),(x1,y1),(x2,y2),(127,127)で表している。
        ///     各軸のパラメータを
        ///         X軸の補間パラメータ   (X_x1, X_y1), (X_x2, X_y2)
        ///         Y軸の補間パラメータ   (Y_x1, Y_y1), (Y_x2, Y_y2)
        ///         Z軸の補間パラメータ   (Z_x1, Z_y1), (Z_x2, Z_y2)
        ///         回転の補間パラメータ  (R_x1, R_y1), (R_x2, R_y2)
        ///         距離の補間パラメータ  (L_x1, L_y1), (L_x2, L_y2)
        ///         視野角の補間パラメータ(V_x1, V_y1), (V_x2, V_y2)
        ///     とした時、補間パラメータは以下の通り。
        ///         X_x1 X_x2 X_y1 X_y2
        ///         Y_x1 Y_x2 Y_y1 Y_y2
        ///         Z_x1 Z_x2 Z_y1 Z_y2
        ///         R_x1 R_x2 R_y1 R_y2
        ///         L_x1 L_x2 L_y1 L_y2
        ///         V_x1 V_x2 V_y1 V_y2
        /// </remarks>
        /// <seealso cref="https://harigane.at.webry.info/201103/article_1.html"/>
        public byte[][] 補間データ;

        /// <summary>
        ///     [0]X軸, [1]Y軸, [2]Z軸, [3]回転, [4]距離, [5]視野角 のベジェ補間曲線。
        ///     回転は４次元だが曲線は１つなので注意。
        /// </summary>
        /// <remarks>
        ///     読み込み後、<see cref="補間データ"/> から自動生成される。
        /// </remarks>
        public ベジェ曲線[] ベジェ曲線;

		/// <summary>
		///		視野角[度]
		/// </summary>
        public uint 視野角;

        /// <summary>
        ///     true:ON, false:OFF
        /// </summary>
        public bool パースペクティブ;


        public カメラフレーム()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal カメラフレーム( Stream fs )
        {
            this.フレーム番号 = ParserHelper.get_DWORD( fs );
            this.距離 = ParserHelper.get_Float( fs );
            this.位置 = ParserHelper.get_Float3( fs );
            this.回転 = ParserHelper.get_Float3( fs );
            this.回転.X = -this.回転.X;   // カメラの回転量は正負が逆であるため、ここで符号を反転しておく。
            this.回転.Y = -this.回転.Y;
            this.回転.Z = -this.回転.Z;

            this.補間データ = new byte[ 6 ][];
            for( int i = 0; i < 6; i++ )
            {
                this.補間データ[ i ] = new byte[ 4 ];
                for( int j = 0; j < 4; j++ )
                    this.補間データ[ i ][ j ] = ParserHelper.get_Byte( fs );
            }

            this.視野角 = ParserHelper.get_DWORD( fs );
            this.パースペクティブ = ( 0 == ParserHelper.get_Byte( fs ) ) ? true : false;    // 0 が ON で 1 が OFF なので注意

            // 補間データからベジェ曲線を生成する。
            this.ベジェ曲線 = new ベジェ曲線[ 6 ];
            for( int i = 0; i < 6; i++ )
            {
                var curve = new ベジェ曲線 {
                    //v0 = new Vector2( 0, 0 ),
                    v1 = new Vector2( this.補間データ[ i ][ 0 ] / 128f, this.補間データ[ i ][ 1 ] / 128f ),
                    v2 = new Vector2( this.補間データ[ i ][ 2 ] / 128f, this.補間データ[ i ][ 3 ] / 128f ),
                    //v3 = new Vector2( 1, 1 ),
                };
                this.ベジェ曲線[ i ] = curve;
            }
        }

        /// <summary>
        ///     比較メソッド。
        /// </summary>
        public int Compare( カメラフレーム x, カメラフレーム y )
        {
            return (int) ( x.フレーム番号 - y.フレーム番号 );
        }
    }
}