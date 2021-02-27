using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.VMDFormat
{
    public class CameraFrame : IComparer<CameraFrame>
    {
        public uint FrameNumber;

		/// <summary>
		///		目標点とカメラの距離
		///		目標点がカメラの前にあるとき負数
		/// </summary>
        public float Distance;

		/// <summary>
		///		目標点の位置
		/// </summary>
        public Vector3 Position;

		/// <summary>
		///		カメラの回転量[ラジアン]
		///		X, Y, Z AxleRotation。
		/// </summary>
        public Vector3 Rotation;

        /// <summary>
        ///     XAxis, YAxis, ZAxis, Rotation, Distance, ViewingAngle のベジェ補間曲線。
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
        public byte[][] InterpolatedData;

        /// <summary>
        ///     [0]XAxis, [1]YAxis, [2]ZAxis, [3]Rotation, [4]Distance, [5]ViewingAngle のベジェ補間曲線。
        ///     回転は４次元だが曲線は１つなので注意。
        /// </summary>
        /// <remarks>
        ///     読み込み後、<see cref="InterpolatedData"/> から自動生成される。
        /// </remarks>
        public BezierCurve[] BezierCurve;

		/// <summary>
		///		ViewingAngle[度]
		/// </summary>
        public uint ViewingAngle;

        /// <summary>
        ///     true:ON, false:OFF
        /// </summary>
        public bool Perspective;


        public CameraFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal CameraFrame( Stream fs )
        {
            this.FrameNumber = ParserHelper.get_DWORD( fs );
            this.Distance = ParserHelper.get_Float( fs );
            this.Position = ParserHelper.get_Float3( fs );
            this.Rotation = ParserHelper.get_Float3( fs );
            this.Rotation.X = -this.Rotation.X;   // カメラの回転量は正負が逆であるため、ここで符号を反転しておく。
            this.Rotation.Y = -this.Rotation.Y;
            this.Rotation.Z = -this.Rotation.Z;

            this.InterpolatedData = new byte[ 6 ][];
            for( int i = 0; i < 6; i++ )
            {
                this.InterpolatedData[ i ] = new byte[ 4 ];
                for( int j = 0; j < 4; j++ )
                    this.InterpolatedData[ i ][ j ] = ParserHelper.get_Byte( fs );
            }

            this.ViewingAngle = ParserHelper.get_DWORD( fs );
            this.Perspective = ( 0 == ParserHelper.get_Byte( fs ) ) ? true : false;    // 0 が ON で 1 が OFF なので注意

            // 補間データからベジェ曲線を生成する。
            this.BezierCurve = new BezierCurve[ 6 ];
            for( int i = 0; i < 6; i++ )
            {
                var curve = new BezierCurve {
                    //v0 = new Vector2( 0, 0 ),
                    v1 = new Vector2( this.InterpolatedData[ i ][ 0 ] / 128f, this.InterpolatedData[ i ][ 1 ] / 128f ),
                    v2 = new Vector2( this.InterpolatedData[ i ][ 2 ] / 128f, this.InterpolatedData[ i ][ 3 ] / 128f ),
                    //v3 = new Vector2( 1, 1 ),
                };
                this.BezierCurve[ i ] = curve;
            }
        }

        /// <summary>
        ///     比較メソッド。
        /// </summary>
        public int Compare( CameraFrame x, CameraFrame y )
        {
            return (int) ( x.FrameNumber - y.FrameNumber );
        }
    }
}
