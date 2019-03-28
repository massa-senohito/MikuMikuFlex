using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class 材質
    {
        public string 材質名;

        public string 材質名_英;

        
        // 材質色

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 拡散色;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 反射色;

        public float 反射強度;

        /// <summary>
        ///     (R, B, B)
        /// </summary>
        public Vector3 環境色;

        
        // 描画

        public 描画フラグ 描画フラグ;

        /// <summary>
        ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 エッジ色;

        /// <summary>
        ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
        ///     Point 描画時は Point サイズ(※2.1拡張)。
        /// </summary>
        public float エッジサイズ;


        // テクスチャ／メモ

        public int 通常テクスチャの参照インデックス;

        public int スフィアテクスチャの参照インデックス;

        public スフィアモード スフィアモード;

        /// <summary>
        ///     0 or 1  。
        ///     <see cref="共有Toonのテクスチャ参照インデックス"/> のサマリを参照のこと。
        /// </summary>
        public byte 共有Toonフラグ;

        /// <summary>
        ///     <see cref="共有Toonフラグ"/> が 0 の時は、Toonテクスチャテクスチャテーブルの参照インデックス。
        ///     <see cref="共有Toonフラグ"/> が 1 の時は、共有Toonテクスチャ[0~9]がそれぞれ toon01.bmp~toon10.bmp に対応。
        /// </summary>
        public int 共有Toonのテクスチャ参照インデックス;

        /// <summary>
        ///     自由欄／スクリプト記述／エフェクトへのパラメータ配置など
        /// </summary>
        public String メモ;


        /// <summary>
        ///     材質に対応する面数（頂点数で示す）。
        ///     １面は３頂点なので、必ず３の倍数になる。
        /// </summary>
        public int 頂点数;

        public int 開始インデックス;


        public 材質()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 材質( Stream fs, ヘッダ header, int index )
        {
            this.開始インデックス = index;
            this.材質名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.材質名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.拡散色 = ParserHelper.get_Float4( fs );
            this.反射色 = ParserHelper.get_Float3( fs );
            this.反射強度 = ParserHelper.get_Float( fs );
            this.環境色 = ParserHelper.get_Float3( fs );
            this.描画フラグ = (描画フラグ) ParserHelper.get_Byte( fs );
            this.エッジ色 = ParserHelper.get_Float4( fs );
            this.エッジサイズ = ParserHelper.get_Float( fs );
            this.通常テクスチャの参照インデックス = ParserHelper.get_Index( fs, header.テクスチャインデックスサイズ );
            this.スフィアテクスチャの参照インデックス = ParserHelper.get_Index( fs, header.テクスチャインデックスサイズ );

            switch( ParserHelper.get_Byte( fs ) )
            {
                case 0:
                    this.スフィアモード = スフィアモード.無効;
                    break;

                case 1:
                    this.スフィアモード = スフィアモード.乗算;
                    break;

                case 2:
                    this.スフィアモード = スフィアモード.加算;
                    break;

                case 3:
                    this.スフィアモード = スフィアモード.サブテクスチャ;
                    break;

                default:
                    throw new InvalidDataException( "スフィアモード値が異常です。" );
            }

            this.共有Toonフラグ = ParserHelper.get_Byte( fs );
            this.共有Toonのテクスチャ参照インデックス = this.共有Toonフラグ == 0 ? ParserHelper.get_Index( fs, header.テクスチャインデックスサイズ ) : ParserHelper.get_Byte( fs );
            this.メモ = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.頂点数 = ParserHelper.get_Int( fs );
            if( this.頂点数 % 3 != 0 )
                throw new InvalidDataException();   // 3 の倍数じゃなければエラー。
        }
    }
}
