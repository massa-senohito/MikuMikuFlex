using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     すべてのジョイントパラメータの基本クラス。
    ///     詳細は別クラスで実装すること。
    /// </summary>
    public class Joint
    {
        public string JointName { get; private set; }

        public string JointName_English { get; private set; }

        public JointType Type { get; private set; }

        public JointParameters Parameters { get; private set; }


        public Joint()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Joint( Stream fs, Header header )
        {
            this.JointName = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.JointName_English = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.Type = (JointType) ParserHelper.get_Byte( fs );
            this.Parameters = JointParameters.Read( fs, header, this.Type );
        }
    }
}
