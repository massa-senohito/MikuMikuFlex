using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class PMXモデル情報
    {
        public String モデル名 { get; private set; }

        public String モデル名_英 { get; private set; }

        public String コメント { get; private set; }

        public String コメント_英 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static PMXモデル情報 読み込む( FileStream fs, PMXヘッダ header )
        {
            var info = new PMXモデル情報();

            info.モデル名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            info.モデル名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            info.コメント = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            info.コメント_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );

            return info;
        }
    }
}
