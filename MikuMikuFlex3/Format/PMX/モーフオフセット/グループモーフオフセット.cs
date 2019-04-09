using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class グループモーフオフセット : モーフオフセット
    {
        /// <summary>
        ///     ※仕様上グループモーフのグループ化は非対応とする
        /// </summary>
        public int モーフインデックス { get; private set; }

        /// <summary>
        ///     対象モーフのモーフ値 ＝ グループモーフのモーフ値 × 対象モーフの影響度
        /// </summary>
        public float 影響度 { get; private set; }


        public グループモーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal グループモーフオフセット( Stream st, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.グループ;
            this.モーフインデックス = ParserHelper.get_Index( st, header.モーフインデックスサイズ );
            this.影響度 = ParserHelper.get_Float( st );
        }
    }
}
