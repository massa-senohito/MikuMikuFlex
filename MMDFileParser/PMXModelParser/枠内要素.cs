using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
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


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 枠内要素 読み込む( Stream fs, PMXヘッダ header )
        {
            var data = new 枠内要素();

            data.要素対象 = ( ParserHelper.get_Byte( fs ) == 1 );

            if( data.要素対象 )
            {
                data.要素対象インデックス = ParserHelper.get_Index( fs, header.モーフインデックスサイズ );
            }
            else
            {
                data.要素対象インデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            }

            return data;
        }
    }
}
