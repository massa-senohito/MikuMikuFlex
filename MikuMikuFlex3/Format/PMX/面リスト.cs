using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class FaceList : List<Surface>
    {
        public FaceList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal FaceList( Stream st, Header header )
        {
            int NumberOfFaces = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfFaces: {NumberOfFaces / 3}" );

            this.Capacity = NumberOfFaces / 3;

            for( int i = 0; i < NumberOfFaces / 3; i++ )
            {
                this.Add(
                    new Surface(
                        ParserHelper.get_VertexIndex( st, header.VertexIndexSize ),
                        ParserHelper.get_VertexIndex( st, header.VertexIndexSize ),
                        ParserHelper.get_VertexIndex( st, header.VertexIndexSize ) ) );
            }
        }
    }
}
