using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMX
{
    public class 材質モーフオフセット : モーフオフセット
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


        public 材質モーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 材質モーフオフセット( FileStream fs, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.材質;
            this.材質インデックス = ParserHelper.get_Index( fs, header.材質インデックスサイズ );
            this.オフセット演算形式 = ParserHelper.get_Byte( fs );
            this.拡散色 = ParserHelper.get_Float4( fs );
            this.反射色 = ParserHelper.get_Float3( fs );
            this.反射強度 = ParserHelper.get_Float( fs );
            this.環境色 = ParserHelper.get_Float3( fs );
            this.エッジ色 = ParserHelper.get_Float4( fs );
            this.エッジサイズ = ParserHelper.get_Float( fs );
            this.テクスチャ係数 = ParserHelper.get_Float4( fs );
            this.スフィアテクスチャ係数 = ParserHelper.get_Float4( fs );
            this.Toonテクスチャ係数 = ParserHelper.get_Float4( fs );
        }
    }
}
