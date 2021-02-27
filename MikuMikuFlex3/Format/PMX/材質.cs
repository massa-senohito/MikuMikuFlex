using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class Material
    {
        public string MaterialName;

        public string MaterialName_English;

        
        // 材質色

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 DiffuseColor;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 ReflectiveColor;

        public float ReflectionIntensity;

        /// <summary>
        ///     (R, B, B)
        /// </summary>
        public Vector3 EnvironmentalColor;

        
        // Drawing

        public DrawingFlag DrawingFlag;

        /// <summary>
        ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 EdgeColor;

        /// <summary>
        ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
        ///     Point 描画時は Point Size(※2.1拡張)。
        /// </summary>
        public float EdgeSize;


        // Texture／Note

        public int ReferenceIndexOfNormalTexture;

        public int SphereTextureReferenceIndex;

        public SphereMode SphereMode;

        /// <summary>
        ///     0 or 1  。
        ///     <see cref="ShareToonのテクスチャ参照インデックス"/> のサマリを参照のこと。
        /// </summary>
        public byte ShareToonFlag;

        /// <summary>
        ///     <see cref="ShareToonFlag"/> が 0 の時は、Toonテクスチャテクスチャテーブルの参照インデックス。
        ///     <see cref="ShareToonFlag"/> が 1 の時は、ShareToonTexture[0~9]がそれぞれ toon01.bmp~toon10.bmp に対応。
        /// </summary>
        public int ShareToonのテクスチャ参照インデックス;

        /// <summary>
        ///     自由欄／スクリプト記述／エフェクトへのパラメータ配置など
        /// </summary>
        public String Note;


        /// <summary>
        ///     材質に対応する面数（頂点数で示す）。
        ///     １面は３頂点なので、必ず３の倍数になる。
        /// </summary>
        public int NumberOfVertices;

        public int StartingIndex;


        public Material()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Material( Stream fs, Header header, int index )
        {
            this.StartingIndex = index;
            this.MaterialName = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.MaterialName_English = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.DiffuseColor = ParserHelper.get_Float4( fs );
            this.ReflectiveColor = ParserHelper.get_Float3( fs );
            this.ReflectionIntensity = ParserHelper.get_Float( fs );
            this.EnvironmentalColor = ParserHelper.get_Float3( fs );
            this.DrawingFlag = (DrawingFlag) ParserHelper.get_Byte( fs );
            this.EdgeColor = ParserHelper.get_Float4( fs );
            this.EdgeSize = ParserHelper.get_Float( fs );
            this.ReferenceIndexOfNormalTexture = ParserHelper.get_Index( fs, header.TextureIndexSize );
            this.SphereTextureReferenceIndex = ParserHelper.get_Index( fs, header.TextureIndexSize );

            switch( ParserHelper.get_Byte( fs ) )
            {
                case 0:
                    this.SphereMode = SphereMode.Invalid;
                    break;

                case 1:
                    this.SphereMode = SphereMode.Multiply;
                    break;

                case 2:
                    this.SphereMode = SphereMode.Addition;
                    break;

                case 3:
                    this.SphereMode = SphereMode.Subtexture;
                    break;

                default:
                    throw new InvalidDataException( "SphereModeValueIsAbnormal。" );
            }

            this.ShareToonFlag = ParserHelper.get_Byte( fs );
            this.ShareToonのテクスチャ参照インデックス = this.ShareToonFlag == 0 ? ParserHelper.get_Index( fs, header.TextureIndexSize ) : ParserHelper.get_Byte( fs );
            this.Note = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.NumberOfVertices = ParserHelper.get_Int( fs );
            if( this.NumberOfVertices % 3 != 0 )
                throw new InvalidDataException();   // 3 の倍数じゃなければエラー。
        }
    }
}
