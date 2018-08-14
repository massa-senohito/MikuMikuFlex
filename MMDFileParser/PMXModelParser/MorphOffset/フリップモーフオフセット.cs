using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class フリップモーフオフセット : モーフオフセット
    {
        public int モーフインデックス { get; private set; }

        public float モーフ値 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static フリップモーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new フリップモーフオフセット();

            offset.モーフ種類 = モーフ種類.フリップ;
            offset.モーフインデックス = ParserHelper.get_Index( fs, header.モーフインデックスサイズ );
            offset.モーフ値 = ParserHelper.get_Float( fs );

            return offset;
        }
    }
}
