using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
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

        public ボーンウェイト種別 ウェイト変形方式;

        public ボーンウェイト ボーンウェイト;

        /// <summary>
        ///     材質のエッジサイズに対しての倍率値。
        /// </summary>
        public float エッジ倍率;


        public 頂点()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 頂点( Stream st, ヘッダ header )
        {
            this.位置 = ParserHelper.get_Float3( st );
            this.法線 = ParserHelper.get_Float3( st );
            this.UV = ParserHelper.get_Float2( st );
            this.追加UV = new Vector4[ header.追加UV数 ];

            for( int i = 0; i < header.追加UV数; i++ )
                this.追加UV[ i ] = ParserHelper.get_Float4( st );

            switch( (ボーンウェイト種別) ParserHelper.get_Byte( st ) )
            {
                case ボーンウェイト種別.BDEF1:
                    this.ウェイト変形方式 = ボーンウェイト種別.BDEF1;
                    this.ボーンウェイト = new BDEF1() {
                        boneReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                    };
                    break;

                case ボーンウェイト種別.BDEF2:
                    this.ウェイト変形方式 = ボーンウェイト種別.BDEF2;
                    this.ボーンウェイト = new BDEF2() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone1Weight = ParserHelper.get_Float( st ),
                    };
                    break;

                case ボーンウェイト種別.BDEF4:
                    this.ウェイト変形方式 = ボーンウェイト種別.BDEF4;
                    this.ボーンウェイト = new BDEF4() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Weights = ParserHelper.get_Float4( st ),
                    };
                    break;

                case ボーンウェイト種別.SDEF:
                    this.ウェイト変形方式 = ボーンウェイト種別.SDEF;
                    this.ボーンウェイト = new SDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone1Weight = ParserHelper.get_Float( st ),
                        SDEF_C = ParserHelper.get_Float3( st ),
                        SDEF_R0 = ParserHelper.get_Float3( st ),
                        SDEF_R1 = ParserHelper.get_Float3( st ),
                    };
                    break;

                case ボーンウェイト種別.QDEF:
                    if( header.PMXバージョン < 2.1f )
                        throw new InvalidDataException( "QDEFはPMX2.1以降でのみサポートされます。" );
                    this.ウェイト変形方式 = ボーンウェイト種別.QDEF;
                    this.ボーンウェイト = new QDEF() {
                        Bone1ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone2ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone3ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Bone4ReferenceIndex = ParserHelper.get_Index( st, header.ボーンインデックスサイズ ),
                        Weights = ParserHelper.get_Float4( st ),
                    };
                    break;

                default:
                    throw new InvalidDataException();
            }

            this.エッジ倍率 = ParserHelper.get_Float( st );
        }
    }
}
