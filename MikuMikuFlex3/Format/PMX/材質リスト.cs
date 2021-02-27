using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class MaterialList : List<Material>
    {
        public MaterialList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal MaterialList( Stream st, Header header )
        {
            int NumberOfMaterials = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfMaterials: {NumberOfMaterials}" );

            this.Capacity = NumberOfMaterials;

            int Startindex = 0;
            for( int i = 0; i < NumberOfMaterials; i++ )
            {
                var mat = new Material( st, header, Startindex );
                this.Add( mat );
                Startindex += mat.NumberOfVertices;

            }
        }

        public Material ReturnsTheMaterialAtTheSpecifiedPosition( int index )
        {
            int NumberOfFaces = 0;

            for( int i = 0; i < this.Count; i++ )
            {
                NumberOfFaces += this[ i ].NumberOfVertices / 3;

                if( index < NumberOfFaces )
                    return this[ i ];
            }

            throw new InvalidDataException();
        }
    }
}
