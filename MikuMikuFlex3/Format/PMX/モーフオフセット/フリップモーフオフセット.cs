using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class FlipMorphOffset : MorphOffset
    {
        public int MorphIndex { get; private set; }

        public float MorphValue { get; private set; }


        public FlipMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal FlipMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Flip;
            this.MorphIndex = ParserHelper.get_Index( st, header.MorphIndexSize );
            this.MorphValue = ParserHelper.get_Float( st );
        }
    }
}
