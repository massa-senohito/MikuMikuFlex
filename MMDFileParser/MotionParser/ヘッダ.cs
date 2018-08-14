using MMDFileParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class ヘッダ
    {
        /// <summary>
        ///     "Vocaloid Motion Data 0002"
        /// </summary>
        public String ファイルシグネチャ;

        /// <summary>
        ///     "初音ミク" など
        /// </summary>
        public String モデル名;


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ヘッダ 読み込む( Stream fs )
        {
            var header = new ヘッダ();

            header.ファイルシグネチャ = ParserHelper.get_Shift_JISString( fs, 30 );
            header.モデル名 = ParserHelper.get_Shift_JISString( fs, 20 );

            return header;
        }
    }
}
