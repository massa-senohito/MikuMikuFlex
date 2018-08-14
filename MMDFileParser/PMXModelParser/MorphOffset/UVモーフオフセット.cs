using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class UVモーフオフセット : モーフオフセット
    {
        public uint 頂点インデックス { get; private set; }

        /// <summary>
        ///     通常、UV では x,y だけが必要で z,w が不要項目になるが、モーフとしてのデータ値は記録しておく。
        /// </summary>
        public Vector4 UVオフセット量 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static UVモーフオフセット 読み込む( FileStream fs, PMXヘッダ header, モーフ種類 type )
        {
            var offset = new UVモーフオフセット();

            offset.頂点インデックス = ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ );
            offset.UVオフセット量 = ParserHelper.get_Float4( fs );
            offset.モーフ種類 = type;

            return offset;
        }
    }
}
