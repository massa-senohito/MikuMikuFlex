using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 UV;

        /// <summary>
        ///     PMXの頂点には追加UV(中身は4Dベクトル)を最大4つまで格納することが可能。
        ///     追加数は <see cref="PMXHeader.AddToUVNumber"/>で指定される。
        /// </summary>
        public Vector4[] AddToUV;

        public BoneWeightType WeightDeformationMethod;

        public BoneWeight BoneWeight;

        /// <summary>
        ///     材質のエッジサイズに対しての倍率値。
        /// </summary>
        public float EdgeMagnification;


        public Vertex()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Vertex( Stream st, Header header )
        {
            this.Position = ParserHelper.get_Float3( st );
            this.Normal = ParserHelper.get_Float3( st );
            this.UV = ParserHelper.get_Float2( st );
            this.AddToUV = new Vector4[ header.AddToUVNumber ];

            for( int i = 0; i < header.AddToUVNumber; i++ )
                this.AddToUV[ i ] = ParserHelper.get_Float4( st );

            switch( (BoneWeightType) ParserHelper.get_Byte( st ) )
            {
                case BoneWeightType.BDEF1:
                    this.WeightDeformationMethod = BoneWeightType.BDEF1;
                    this.BoneWeight = new BDEF1() {
                        boneReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                    };
                    break;

                case BoneWeightType.BDEF2:
                    this.WeightDeformationMethod = BoneWeightType.BDEF2;
                    this.BoneWeight = new BDEF2() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone1Weight = ParserHelper.get_Float( st ),
                    };
                    break;

                case BoneWeightType.BDEF4:
                    this.WeightDeformationMethod = BoneWeightType.BDEF4;
                    this.BoneWeight = new BDEF4() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Weights = ParserHelper.get_Float4( st ),
                    };
                    break;

                case BoneWeightType.SDEF:
                    this.WeightDeformationMethod = BoneWeightType.SDEF;
                    this.BoneWeight = new SDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone1Weight = ParserHelper.get_Float( st ),
                        SDEF_C = ParserHelper.get_Float3( st ),
                        SDEF_R0 = ParserHelper.get_Float3( st ),
                        SDEF_R1 = ParserHelper.get_Float3( st ),
                    };
                    break;

                case BoneWeightType.QDEF:
                    if( header.PMXVersion < 2.1f )
                        throw new InvalidDataException( "QDEFはPMX2.1以降でのみサポートされます。" );
                    this.WeightDeformationMethod = BoneWeightType.QDEF;
                    this.BoneWeight = new QDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( st, header.BoneIndexSize ),
                        Weights = ParserHelper.get_Float4( st ),
                    };
                    break;

                default:
                    throw new InvalidDataException();
            }

            this.EdgeMagnification = ParserHelper.get_Float( st );
        }
    }
}
