using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class 頂点モーフオフセット:モーフオフセット
    {
        public uint 頂点インデックス { get; private set; }

        public Vector3 座標オフセット量 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 頂点モーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new 頂点モーフオフセット();

            offset.モーフ種類 = モーフ種類.頂点;
            offset.頂点インデックス = ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ );
            offset.座標オフセット量 = ParserHelper.get_Float3( fs );

            return offset;
        }
    }
}
