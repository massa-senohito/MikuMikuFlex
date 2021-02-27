using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class VertexList : List<Vertex>
    {
        public VertexList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal VertexList( Stream st, Header header )
        {
            int NumberOfVertices = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfVertices: {NumberOfVertices}" );

            this.Capacity = NumberOfVertices;

            for( int i = 0; i < NumberOfVertices; i++ )
                this.Add( new Vertex( st, header ) );
#if DEBUG
            int[] NumberOfAppearances = new int[ 5 ];
            for( int i = 0; i < NumberOfVertices; i++ )
            {
                var WeightConversionMethod = (byte) this[ i ].WeightDeformationMethod;
                NumberOfAppearances[ WeightConversionMethod ]++;
            }
            Debug.WriteLine( $"   " +
                $"BDEF1:{NumberOfAppearances[(int) BoneWeightType.BDEF1]}, "+
                $"BDEF2:{NumberOfAppearances[ (int) BoneWeightType.BDEF2 ]}, "+
                $"BDEF4:{NumberOfAppearances[ (int) BoneWeightType.BDEF4 ]}, "+
                $"SDEF:{NumberOfAppearances[ (int) BoneWeightType.SDEF ]}, "+
                $"QDEF:{NumberOfAppearances[ (int) BoneWeightType.QDEF ]}" );
#endif
        }
    }
}
