using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class MorphList : List<Morph>
    {
        public MorphList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal MorphList( Stream st, Header header )
        {
            int NumberOfMorphs = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfMorphs: {NumberOfMorphs}" );

            this.Capacity = NumberOfMorphs;

            for( int i = 0; i < NumberOfMorphs; i++ )
                this.Add( new Morph( st, header ) );
        }
    }
}
