using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMX
{
    public class UVモーフオフセット : モーフオフセット
    {
        public uint 頂点インデックス { get; private set; }

        /// <summary>
        ///     通常、UV では x,y だけが必要で z,w が不要項目になるが、モーフとしてのデータ値は記録しておく。
        /// </summary>
        public Vector4 UVオフセット量 { get; private set; }


        public UVモーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal UVモーフオフセット( FileStream fs, ヘッダ header, モーフ種別 type )
        {
            this.モーフ種類 = type;
            this.頂点インデックス = ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ );
            this.UVオフセット量 = ParserHelper.get_Float4( fs );
        }
    }
}
