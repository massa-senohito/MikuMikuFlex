using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     PMX仕様521行目参照、枠内要素にあたるクラス
    /// </summary>
    public class 枠内要素
    {
        /// <summary>
        ///     true なら <see cref="要素対象インデックス"/> は モーフインデックス であり、
        ///     false なら <see cref="要素対象インデックス"/> は ボーンインデックス である。
        /// </summary>
        public bool 要素対象 { get; private set; }

        public int 要素対象インデックス { get; private set; }


        public 枠内要素()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 枠内要素( Stream fs, ヘッダ header )
        {
            this.要素対象 = ( ParserHelper.get_Byte( fs ) == 1 );

            if( this.要素対象 )
            {
                this.要素対象インデックス = ParserHelper.get_Index( fs, header.モーフインデックスサイズ );
            }
            else
            {
                this.要素対象インデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            }
        }
    }
}
