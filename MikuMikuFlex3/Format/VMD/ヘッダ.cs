using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class Header
    {
        /// <summary>
        ///     "Vocaloid Motion Data 0002"
        /// </summary>
        public string FileSignature;

        /// <summary>
        ///     "初音ミク" など
        /// </summary>
        public string ModelName;


        public Header()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Header( Stream fs )
        {
            this.FileSignature = ParserHelper.get_Shift_JISString( fs, 30 );
            this.ModelName = ParserHelper.get_Shift_JISString( fs, 20 );
        }
    }
}
