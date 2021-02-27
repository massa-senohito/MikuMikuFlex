using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class MaterialMorphOffset : MorphOffset
    {
        /// <summary>
        ///     -1 なら 全材質対象。
        /// </summary>
        public int MaterialIndex { get; private set; }

        /// <summary>
        ///     0:Multiply、1:Addition
        /// </summary>
        public byte OffsetCalculationFormat { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 DiffuseColor { get; private set; }

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 ReflectiveColor { get; private set; }

        public float ReflectionIntensity { get; private set; }

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 EnvironmentalColor { get; private set; }

        public float EdgeSize { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 EdgeColor { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 TextureCoefficient { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 SphereTextureCoefficient { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 ToonTextureCoefficient { get; private set; }


        public MaterialMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal MaterialMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Material;
            this.MaterialIndex = ParserHelper.get_Index( st, header.MaterialIndexSize );
            this.OffsetCalculationFormat = ParserHelper.get_Byte( st );
            this.DiffuseColor = ParserHelper.get_Float4( st );
            this.ReflectiveColor = ParserHelper.get_Float3( st );
            this.ReflectionIntensity = ParserHelper.get_Float( st );
            this.EnvironmentalColor = ParserHelper.get_Float3( st );
            this.EdgeColor = ParserHelper.get_Float4( st );
            this.EdgeSize = ParserHelper.get_Float( st );
            this.TextureCoefficient = ParserHelper.get_Float4( st );
            this.SphereTextureCoefficient = ParserHelper.get_Float4( st );
            this.ToonTextureCoefficient = ParserHelper.get_Float4( st );
        }
    }
}
