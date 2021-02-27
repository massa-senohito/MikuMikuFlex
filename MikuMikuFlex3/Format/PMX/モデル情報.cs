using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class ModelInformation
    {
        public string ModelName { get; private set; }

        public string ModelName_English { get; private set; }

        public string Comment { get; private set; }

        public string Comment_English { get; private set; }


        public ModelInformation()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ModelInformation( Stream st, Header header )
        {
            this.ModelName = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.ModelName_English = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.Comment = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.Comment_English = ParserHelper.get_TextBuf( st, header.EncodingMethod );
        }
    }
}
