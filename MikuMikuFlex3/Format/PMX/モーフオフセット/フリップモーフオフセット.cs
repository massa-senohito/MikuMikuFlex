using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class フリップモーフオフセット : モーフオフセット
    {
        public int モーフインデックス { get; private set; }

        public float モーフ値 { get; private set; }


        public フリップモーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal フリップモーフオフセット( FileStream fs, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.フリップ;
            this.モーフインデックス = ParserHelper.get_Index( fs, header.モーフインデックスサイズ );
            this.モーフ値 = ParserHelper.get_Float( fs );
        }
    }
}
