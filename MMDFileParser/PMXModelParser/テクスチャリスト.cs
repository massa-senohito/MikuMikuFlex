using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    /// <summary>
    ///     材質内で参照されるテクスチャパスのテーブル。
    /// </summary>
    /// <remarks>
    ///     ※材質からはこちらのIndexを参照。テクスチャ／スフィア／個別Toonで一括して利用
    ///     ※共有Toon -> toon01.bmp～toon10.bmp はテクスチャテーブルには入れないので注意
    ///     ※PMDと同様にモデル位置を基準としてサブフォルダ指定が可能
    ///     　パス区切りについてはシステム側で認識可能な文字('\'or'/'など)とし、PMXとして固定はしない。
    /// </remarks>
    public class テクスチャリスト : List<string>
    {
        public テクスチャリスト()
            : base()
        {
        }
        public テクスチャリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static テクスチャリスト 読み込む( FileStream fs, PMXヘッダ header )
        {
            int テクスチャ数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"テクスチャ数: {テクスチャ数}" );

            var list = new テクスチャリスト( テクスチャ数 );

            for( int i = 0; i < テクスチャ数; i++ )
            {
                list.Add( ParserHelper.get_TextBuf( fs, header.エンコード方式 ) );
            }

            return list;
        }
    }
}
