using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.VMDFormat
{
    public class BoneFrame : IFrameData
    {
        /// <summary>
        ///     "センター"、"左肩" など
        /// </summary>
        public string BoneName;

        public uint FrameNumber { get; private set; }

		/// <summary>
		///		位置指定がない場合は(0,0,0)
		/// </summary>
        public Vector3 BonePosition;

		/// <summary>
		///		回転指定がない場合は(0,0,0,0)
		/// </summary>
        public Quaternion BoneRotation;

        /// <summary>
        ///     XAxis, YAxis, ZAxis, Rotation の補間曲線を表すデータ。
        ///     [4][4][4] の 64bytes。
        /// </summary>
        /// <remarks>
        ///     補間パラメータは4点のベジェ曲線(0,0),(x1,y1),(x2,y2),(127,127)で表している。
        ///     各軸のパラメータを
        ///         X軸の補間パラメータ( X_x1, X_y1),(X_x2, X_y2)
        ///         Y軸の補間パラメータ( Y_x1, Y_y1),(Y_x2, Y_y2)
        ///         Z軸の補間パラメータ( Z_x1, Z_y1),(Z_x2, Z_y2)
        ///         回転の補間パラメータ( R_x1, R_y1),(R_x2, R_y2)
        ///     とした時、補間パラメータは以下の通り。
        ///         X_x1,Y_x1,Z_x1,R_x1,X_y1,Y_y1,Z_y1,R_y1,
        ///         X_x2,Y_x2,Z_x2,R_x2,X_y2,Y_y2,Z_y2,R_y2,
        ///         Y_x1,Z_x1,R_x1,X_y1,Y_y1,Z_y1,R_y1,X_x2,
        ///         Y_x2,Z_x2,R_x2,X_y2,Y_y2,Z_y2,R_y2, 01,
        ///         Z_x1,R_x1,X_y1,Y_y1,Z_y1,R_y1,X_x2,Y_x2,
        ///         Z_x2,R_x2,X_y2,Y_y2,Z_y2,R_y2, 01, 00,
        ///         R_x1,X_y1,Y_y1,Z_y1,R_y1,X_x2,Y_x2,Z_x2,
        ///         R_x2,X_y2,Y_y2,Z_y2,R_y2, 01, 00, 00
        /// </remarks>
        /// <seealso cref="https://harigane.at.webry.info/201103/article_1.html"/>
        public byte[][][] InterpolatedData;

        /// <summary>
        ///     [0]XAxis, [1]YAxis, [2]ZAxis, [3]Rotation のベジェ補間曲線。
        ///     回転は４次元だが曲線は１つなので注意。
        /// </summary>
        /// <remarks>
        ///     読み込み後、<see cref="InterpolatedData"/> から自動生成される。
        /// </remarks>
        public BezierCurve[] BezierCurve;


        public BoneFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal BoneFrame( Stream fs )
        {
            this.BoneName = ParserHelper.get_Shift_JISString( fs, 15 );
            this.FrameNumber = ParserHelper.get_DWORD( fs );
            this.BonePosition = ParserHelper.get_Float3( fs );
            this.BoneRotation = ParserHelper.get_Quaternion( fs );

            // InterpolatedData[4][4][4]
            this.InterpolatedData = new byte[ 4 ][][];
            for( int i = 0; i < 4; i++ )
            {
                this.InterpolatedData[ i ] = new byte[ 4 ][];
                for( int j = 0; j < 4; j++ )
                {
                    this.InterpolatedData[ i ][ j ] = new byte[ 4 ];
                    for( int k = 0; k < 4; k++ )
                        this.InterpolatedData[ i ][ j ][ k ] = ParserHelper.get_Byte( fs );
                }
            }

            // 補間データからベジェ曲線を生成する。
            // 補間データは、[0][0～3][0～4] のみ使用。
            this.BezierCurve = new BezierCurve[ 4 ];
            for( int i = 0; i < 4; i++ )
            {
                var curve = new BezierCurve() {
                    //v0 = new Vector2( 0, 0 ),
                    v1 = new Vector2( (float) this.InterpolatedData[ 0 ][ 0 ][ i ] / 128f, (float) this.InterpolatedData[ 0 ][ 1 ][ i ] / 128f ),   // (0,0)側の方向点
                    v2 = new Vector2( (float) this.InterpolatedData[ 0 ][ 2 ][ i ] / 128f, (float) this.InterpolatedData[ 0 ][ 3 ][ i ] / 128f ),   // (1,1)側の方向点
                    //v3 = new Vector2( 1, 1 ),
                };
                this.BezierCurve[ i ] = curve;
            }
        }

        /// <summary>
        ///     比較用メソッド。
        /// </summary>
        public int CompareTo( Object x )
        {
            return (int) FrameNumber - (int) ( (IFrameData) x ).FrameNumber;
        }
    }
}
