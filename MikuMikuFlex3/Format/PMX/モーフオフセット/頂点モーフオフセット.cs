using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class VertexMorphOffset : MorphOffset
    {
        public uint VertexIndex { get; private set; }

        public Vector3 CoordinateOffsetAmount { get; private set; }


        public VertexMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal VertexMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Vertex;
            this.VertexIndex = ParserHelper.get_VertexIndex( st, header.VertexIndexSize );
            this.CoordinateOffsetAmount = ParserHelper.get_Float3( st );
        }
    }
}
