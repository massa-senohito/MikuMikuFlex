using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class UVMorphOffset : MorphOffset
    {
        public uint VertexIndex { get; private set; }

        /// <summary>
        ///     通常、UV では x,y だけが必要で z,w が不要項目になるが、モーフとしてのデータ値は記録しておく。
        /// </summary>
        public Vector4 UVOffsetAmount { get; private set; }


        public UVMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal UVMorphOffset( Stream st, Header header, MorphType type )
        {
            this.MorphType = type;
            this.VertexIndex = ParserHelper.get_VertexIndex( st, header.VertexIndexSize );
            this.UVOffsetAmount = ParserHelper.get_Float4( st );
        }
    }
}
