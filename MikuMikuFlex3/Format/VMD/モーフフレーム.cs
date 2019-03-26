using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class モーフフレーム : IFrameData
    {
		/// <summary>
		///		"まばたき" など
		/// </summary>
        public string モーフ名;

        public uint フレーム番号 { get; private set; }

        /// <summary>
        ///     スライダーの値(0～1)
        /// </summary>
        public float モーフ値;


        public モーフフレーム()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal モーフフレーム( Stream fs )
        {
            this.モーフ名 = ParserHelper.get_Shift_JISString( fs, 15 );
            this.フレーム番号 = ParserHelper.get_DWORD( fs );
            this.モーフ値 = ParserHelper.get_Float( fs );
        }

        /// <summary>
        ///     比較用メソッド。
        /// </summary>
        public int CompareTo( Object x )
        {
            return (int) フレーム番号 - (int) ( (IFrameData) x ).フレーム番号;
        }
    }
}
