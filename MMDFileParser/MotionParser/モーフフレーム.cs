using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class モーフフレーム : IFrameData
    {
        public String モーフ名;

        public uint フレーム番号 { get; private set; }

        /// <summary>
        ///     スライダーの値
        /// </summary>
        public float モーフ値;


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
        internal static モーフフレーム 読み込む( Stream fs )
        {
            var frame = new モーフフレーム();

            frame.モーフ名 = ParserHelper.get_Shift_JISString( fs, 15 );
            frame.フレーム番号 = ParserHelper.get_DWORD( fs );
            frame.モーフ値 = ParserHelper.get_Float( fs );

            return frame;
        }
    }
}
