using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser
{
    public class 頂点
    {
        public Vector3 位置;

        public Vector3 法線;

        public Vector2 UV;

        /// <summary>
        ///     PMXの頂点には追加UV(中身は4Dベクトル)を最大4つまで格納することが可能。
        ///     追加数は <see cref="PMXヘッダ.追加UV数"/>で指定される。
        /// </summary>
        public Vector4[] 追加UV;

        public BoneWeight.変形方式 ウェイト変形方式;

        /// <summary>
        ///     ボーンウェイト。
        ///     実インスタンスは以下のいずれかとなる。
        ///     <list type="bullet">
        ///         <see cref="BoneWeight.BDEF1"/>
        ///         <see cref="BoneWeight.BDEF2"/>
        ///         <see cref="BoneWeight.BDEF4"/>
        ///         <see cref="BoneWeight.SDEF"/>
        ///         <see cref="BoneWeight.QDEF"/>
        ///     </list>
        /// </summary>
        public BoneWeight.ボーンウェイト ボーンウェイト;

        /// <summary>
        ///     材質のエッジサイズに対しての倍率値。
        /// </summary>
        public float エッジ倍率;


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 頂点 読み込む( FileStream fs, PMXヘッダ header )
        {
            var vertex = new 頂点();

            vertex.位置 = ParserHelper.get_Float3( fs );
            vertex.法線 = ParserHelper.get_Float3( fs );
            vertex.UV = ParserHelper.get_Float2( fs );
            vertex.追加UV = new Vector4[ header.追加UV数 ];

            for( int i = 0; i < header.追加UV数; i++ )
                vertex.追加UV[ i ] = ParserHelper.get_Float4( fs );

            switch( (BoneWeight.変形方式) ParserHelper.get_Byte( fs ) )
            {
                case BoneWeight.変形方式.BDEF1:
                    vertex.ウェイト変形方式 = BoneWeight.変形方式.BDEF1;
                    vertex.ボーンウェイト = new BoneWeight.BDEF1() {
                        boneReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                    };
                    break;

                case BoneWeight.変形方式.BDEF2:
                    vertex.ウェイト変形方式 = BoneWeight.変形方式.BDEF2;
                    vertex.ボーンウェイト = new BoneWeight.BDEF2() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone1Weight = ParserHelper.get_Float( fs ),
                    };
                    break;

                case BoneWeight.変形方式.BDEF4:
                    vertex.ウェイト変形方式 = BoneWeight.変形方式.BDEF4;
                    vertex.ボーンウェイト = new BoneWeight.BDEF4() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Weights = ParserHelper.get_Float4( fs ),
                    };
                    break;

                case BoneWeight.変形方式.SDEF:
                    vertex.ウェイト変形方式 = BoneWeight.変形方式.SDEF;
                    vertex.ボーンウェイト = new BoneWeight.SDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone1Weight = ParserHelper.get_Float( fs ),
                        SDEF_C = ParserHelper.get_Float3( fs ),
                        SDEF_R0 = ParserHelper.get_Float3( fs ),
                        SDEF_R1 = ParserHelper.get_Float3( fs ),
                    };
                    break;

                case BoneWeight.変形方式.QDEF:
                    if( header.PMXバージョン != 2.1f )
                        throw new InvalidDataException( "QDEFはPMX2.1でのみサポートされます。" );
                    vertex.ウェイト変形方式 = BoneWeight.変形方式.QDEF;
                    vertex.ボーンウェイト = new BoneWeight.QDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ ),
                        Weights = ParserHelper.get_Float4( fs ),
                    };
                    break;

                default:
                    throw new InvalidDataException();
            }

            vertex.エッジ倍率 = ParserHelper.get_Float( fs );

            return vertex;
        }
    }
}
