using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class MorphFrame : IFrameData
    {
		/// <summary>
		///		"まばたき" など
		/// </summary>
        public string MorphName;

        public uint FrameNumber { get; private set; }

        /// <summary>
        ///     スライダーの値(0～1)
        /// </summary>
        public float MorphValue;


        public MorphFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal MorphFrame( Stream fs )
        {
            this.MorphName = ParserHelper.get_Shift_JISString( fs, 15 );
            this.FrameNumber = ParserHelper.get_DWORD( fs );
            this.MorphValue = ParserHelper.get_Float( fs );
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
