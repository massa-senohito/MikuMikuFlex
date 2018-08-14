using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class 材質モーフオフセット:モーフオフセット
    {
        /// <summary>
        ///     -1 なら 全材質対象。
        /// </summary>
        public int 材質インデックス { get; private set; }

        /// <summary>
        ///     0:乗算、1:加算
        /// </summary>
        public byte オフセット演算形式 { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 拡散色 { get; private set; }

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 反射色 { get; private set; }

        public float 反射強度 { get; private set; }

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 環境色 { get; private set; }

        public float エッジサイズ { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 エッジ色 { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 テクスチャ係数 { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 スフィアテクスチャ係数 { get; private set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 Toonテクスチャ係数 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 材質モーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new 材質モーフオフセット();

            offset.モーフ種類 = モーフ種類.材質;
            offset.材質インデックス = ParserHelper.get_Index( fs, header.材質インデックスサイズ );
            offset.オフセット演算形式 = ParserHelper.get_Byte( fs );
            offset.拡散色 = ParserHelper.get_Float4( fs );
            offset.反射色 = ParserHelper.get_Float3( fs );
            offset.反射強度 = ParserHelper.get_Float( fs );
            offset.環境色 = ParserHelper.get_Float3( fs );
            offset.エッジ色 = ParserHelper.get_Float4( fs );
            offset.エッジサイズ = ParserHelper.get_Float( fs );
            offset.テクスチャ係数 = ParserHelper.get_Float4( fs );
            offset.スフィアテクスチャ係数 = ParserHelper.get_Float4( fs );
            offset.Toonテクスチャ係数 = ParserHelper.get_Float4( fs );

            return offset;
        }
    }
}
