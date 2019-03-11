using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public class ジョイント
    {
        public string ジョイント名 { get; private set; }

        public string ジョイント名_英 { get; private set; }

        public ジョイント種別 種別 { get; private set; }

        public ジョイントパラメータ パラメータ { get; private set; }


        public ジョイント()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ジョイント( Stream fs, ヘッダ header )
        {
            this.ジョイント名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.ジョイント名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.種別 = (ジョイント種別) ParserHelper.get_Byte( fs );
            this.パラメータ = ジョイントパラメータ.読み込む( fs, header, this.種別 );
        }
    }
}
