using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     材質内で参照されるテクスチャパスのテーブル。
    /// </summary>
    /// <remarks>
    ///     ※材質からはこちらのIndexを参照。Texture／スフィア／個別Toonで一括して利用
    ///     ※ShareToon -> toon01.bmp～toon10.bmp はテクスチャテーブルには入れないので注意
    ///     ※PMDと同様にモデル位置を基準としてサブフォルダ指定が可能
    ///     　パス区切りについてはシステム側で認識可能な文字('\'or'/'など)とし、PMXとして固定はしない。
    /// </remarks>
    public class TextureList : List<string>
    {
        public TextureList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal TextureList( Stream st, Header header )
        {
            int NumberOfTextures = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfTextures: {NumberOfTextures}" );

            this.Capacity = NumberOfTextures;

            for( int i = 0; i < NumberOfTextures; i++ )
                this.Add( ParserHelper.get_TextBuf( st, header.EncodingMethod ) );
        }
    }
}
