using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <remarks>
    ///     格納可能なモーフは大別して、
    ///         VertexMorph
    ///         UVMorph
    ///         BoneMorph
    ///         MaterialMorph
    ///         GroupMorph
    ///     の5種類。さらにUVモーフは、UV／AddToUV1～4の計5種類に分類される。
    ///     ※AddToUV数によっては不要なUVモーフが格納されることがあるが、モーフ側は特に削除などは行わないので注意。
    /// </remarks>
    public class Morph
    {
        public string MorphName { get; private set; }

        public string MorphName_English { get; private set; }

        /// <summary>
        ///     0: システム予約
        ///     1: まゆ（左下）
        ///     2: 目（左上）
        ///     3: 口（右上）
        ///     4: その他（右下）
        /// </summary>
        /// <remarks>
        ///     特に使用しないので、enum 定義もしない。
        /// </remarks>
        public byte ControlPanel { get; private set; }

        public MorphType MorphType { get; private set; }

        public int NumberOfMorphOffsets { get; private set; }

        public MorphOffset[] MorphOffsetList { get; private set; }


        public Morph()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Morph( Stream st, Header header )
        {
            this.MorphName = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.MorphName_English = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.ControlPanel = ParserHelper.get_Byte( st );
            byte Morphtype = ParserHelper.get_Byte( st );
            this.NumberOfMorphOffsets = ParserHelper.get_Int( st );
            this.MorphOffsetList = new MorphOffset[ this.NumberOfMorphOffsets ];

            for( int i = 0; i < this.NumberOfMorphOffsets; i++ )
            {
                switch( Morphtype )
                {
                    case 0:
                        //Group Morph
                        this.MorphType = MorphType.Group;
                        this.MorphOffsetList[ i ] = new GroupMorphOffset( st, header );
                        break;
                    case 1:
                        //Vertex Morph
                        this.MorphType = MorphType.Vertex;
                        this.MorphOffsetList[ i ] = new VertexMorphOffset( st, header );
                        break;
                    case 2:
                        this.MorphType = MorphType.Bourne;
                        this.MorphOffsetList[ i ] = new BoneMorphOffset( st, header );
                        break;
                    //3~7はすべてUVMorph
                    case 3:
                        this.MorphType = MorphType.UV;
                        this.MorphOffsetList[ i ] = new UVMorphOffset( st, header, MorphType.UV );
                        break;
                    case 4:
                        this.MorphType = MorphType.AddToUV1;
                        this.MorphOffsetList[ i ] = new UVMorphOffset( st, header, MorphType.AddToUV1 );
                        break;
                    case 5:
                        this.MorphType = MorphType.AddToUV2;
                        this.MorphOffsetList[ i ] = new UVMorphOffset( st, header, MorphType.AddToUV2 );
                        break;
                    case 6:
                        this.MorphType = MorphType.AddToUV3;
                        this.MorphOffsetList[ i ] = new UVMorphOffset( st, header, MorphType.AddToUV3 );
                        break;
                    case 7:
                        this.MorphType = MorphType.AddToUV4;
                        this.MorphOffsetList[ i ] = new UVMorphOffset( st, header, MorphType.AddToUV4 );
                        break;
                    case 8:
                        //Material Morph
                        this.MorphType = MorphType.Material;
                        this.MorphOffsetList[ i ] = new MaterialMorphOffset( st, header );
                        break;
                    case 9:
                        if( header.PMXVersion < 2.1 ) throw new InvalidDataException( "FlipモーフはPMX2.1以降でサポートされています。" );
                        this.MorphType = MorphType.Flip;
                        this.MorphOffsetList[ i ] = new FlipMorphOffset( st, header );
                        break;
                    case 10:
                        if( header.PMXVersion < 2.1 ) throw new InvalidDataException( "ImpulseモーフはPMX2.1以降でサポートされています。" );
                        this.MorphType = MorphType.Impulse;
                        this.MorphOffsetList[ i ] = new ImpulseMorphOffset( st, header );
                        break;
                }
            }
        }
    }
}
