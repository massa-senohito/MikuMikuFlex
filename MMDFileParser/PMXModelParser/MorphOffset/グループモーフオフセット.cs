using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser.MorphOffset
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


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static グループモーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new グループモーフオフセット();

            offset.モーフ種類 = モーフ種類.グループ;
            offset.モーフインデックス = ParserHelper.get_Index( fs, header.モーフインデックスサイズ );
            offset.影響度 = ParserHelper.get_Float( fs );

            return offset;
        }
    }
}
