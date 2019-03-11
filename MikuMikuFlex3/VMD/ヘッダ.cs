using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMD
{
    public class ヘッダ
    {
        /// <summary>
        ///     "Vocaloid Motion Data 0002"
        /// </summary>
        public string ファイルシグネチャ;

        /// <summary>
        ///     "初音ミク" など
        /// </summary>
        public string モデル名;


        public ヘッダ()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ヘッダ( Stream fs )
        {
            this.ファイルシグネチャ = ParserHelper.get_Shift_JISString( fs, 30 );
            this.モデル名 = ParserHelper.get_Shift_JISString( fs, 20 );
        }
    }
}
