using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.MotionParser
{
    public class カメラフレーム : IComparer<カメラフレーム>
    {
        public uint フレーム番号;

        public float 距離;

        public Vector3 位置;

        public Vector3 回転;

        /// <summary>
        ///     [6][4] の 24bytes
        /// </summary>
        public byte[][] 補間データ;

        /// <summary>
        ///     読み込み後、<see cref="補間データ"/> から生成される。
        /// </summary>
        public ベジェ曲線[] ベジェ曲線;

        public uint 視野角;

        /// <summary>
        ///     true:ON, false:OFF
        /// </summary>
        public bool パースペクティブ;


        /// <summary>
        ///     比較メソッド。
        /// </summary>
        public int Compare( カメラフレーム x, カメラフレーム y )
        {
            return (int) ( x.フレーム番号 - y.フレーム番号 );
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static カメラフレーム 読み込む( Stream fs )
        {
            var frame = new カメラフレーム();

            frame.フレーム番号 = ParserHelper.get_DWORD( fs );
            frame.距離 = ParserHelper.get_Float( fs );
            frame.位置 = ParserHelper.get_Float3( fs );
            frame.回転 = ParserHelper.get_Float3( fs );
            frame.回転.X = -frame.回転.X;   // カメラのX軸回転は正負が逆であるため、ここで符号を反転しておく。

            frame.補間データ = new byte[ 6 ][];
            for( int i = 0; i < 6; i++ )
            {
                frame.補間データ[ i ] = new byte[ 4 ];
                for( int j = 0; j < 4; j++ )
                    frame.補間データ[ i ][ j ] = ParserHelper.get_Byte( fs );
            }

            frame.視野角 = ParserHelper.get_DWORD( fs );
            frame.パースペクティブ = ( 0 == ParserHelper.get_Byte( fs ) ) ? true : false;    // 0 が ON で 1 が OFF なので注意

            // 補間データからベジェ曲線を生成する。
            frame.ベジェ曲線 = new ベジェ曲線[ 6 ];
            for( int i = 0; i < frame.ベジェ曲線.Length; i++ )
            {
                var curve = new ベジェ曲線 {
                    v1 = new Vector2( frame.補間データ[ i ][ 0 ] / 128f, frame.補間データ[ i ][ 1 ] / 128f ),
                    v2 = new Vector2( frame.補間データ[ i ][ 2 ] / 128f, frame.補間データ[ i ][ 3 ] / 128f ),
                };
                frame.ベジェ曲線[ i ] = curve;
            }

            return frame;
        }
    }
}