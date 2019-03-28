using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class モデル情報
    {
        public string モデル名 { get; private set; }

        public string モデル名_英 { get; private set; }

        public string コメント { get; private set; }

        public string コメント_英 { get; private set; }


        public モデル情報()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal モデル情報( Stream st, ヘッダ header )
        {
            this.モデル名 = ParserHelper.get_TextBuf( st, header.エンコード方式 );
            this.モデル名_英 = ParserHelper.get_TextBuf( st, header.エンコード方式 );
            this.コメント = ParserHelper.get_TextBuf( st, header.エンコード方式 );
            this.コメント_英 = ParserHelper.get_TextBuf( st, header.エンコード方式 );
        }
    }
}
