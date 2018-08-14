using MMDFileParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.MotionParser
{
    public class ボーンフレーム : IFrameData
    {
        /// <summary>
        ///     "センター"、"左肩" など
        /// </summary>
        public String ボーン名;

        public uint フレーム番号 { get; private set; }

        public Vector3 ボーンの位置;

        public Quaternion ボーンの回転;

        /// <summary>
        ///     [4][4][4] の 64bytes
        /// </summary>
        public byte[][][] 補間データ;

        /// <summary>
        ///     読み込み後、<see cref="補間データ"/> から生成される。
        /// </summary>
        public ベジェ曲線[] ベジェ曲線;


        /// <summary>
        ///     比較用メソッド。
        /// </summary>
        public int CompareTo( Object x )
        {
            return (int) フレーム番号 - (int) ( (IFrameData) x ).フレーム番号;
        }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ボーンフレーム 読み込む( Stream fs )
        {
            var frame = new ボーンフレーム();

            frame.ボーン名 = ParserHelper.get_Shift_JISString( fs, 15 );
            frame.フレーム番号 = ParserHelper.get_DWORD( fs );
            frame.ボーンの位置 = ParserHelper.get_Float3( fs );
            frame.ボーンの回転 = ParserHelper.get_Quaternion( fs );

            frame.補間データ = new byte[ 4 ][][];
            for( int i = 0; i < 4; i++ )
            {
                frame.補間データ[ i ] = new byte[ 4 ][];
                for( int j = 0; j < 4; j++ )
                {
                    frame.補間データ[ i ][ j ] = new byte[ 4 ];
                    for( int k = 0; k < 4; k++ )
                        frame.補間データ[ i ][ j ][ k ] = ParserHelper.get_Byte( fs );
                }
            }

            // 補間データからベジェ曲線を生成する。
            frame.ベジェ曲線 = new ベジェ曲線[ 4 ];
            for( int i = 0; i < frame.ベジェ曲線.Length; i++ )
            {
                var curve = new ベジェ曲線() {
                    v1 = new Vector2( (float) frame.補間データ[ 0 ][ 0 ][ i ] / 128f, (float) frame.補間データ[ 0 ][ 1 ][ i ] / 128f ),
                    v2 = new Vector2( (float) frame.補間データ[ 0 ][ 2 ][ i ] / 128f, (float) frame.補間データ[ 0 ][ 3 ][ i ] / 128f ),
                };
                frame.ベジェ曲線[ i ] = curve;
            }

            return frame;
        }
    }
}
