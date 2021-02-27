using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class BoneList : List<Bourne>
    {
        public BoneList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal BoneList( Stream st, Header header )
        {
            int NumberOfBones = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfBones: {NumberOfBones}" );

            this.Capacity = NumberOfBones;

            for( int i = 0; i < NumberOfBones; i++ )
                this.Add( new Bourne( st, header ) );
        }
    }
}
