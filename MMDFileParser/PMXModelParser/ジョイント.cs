    using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMDFileParser.PMXModelParser.JointParam;

namespace MMDFileParser.PMXModelParser
{
    public class ジョイント
    {
        public string ジョイント名 { get; private set; }

        public string ジョイント名_英 { get; private set; }

        public enum ジョイント種別
        {
            ばね付き6DOF = 0,  // PMX2.0では 0 のみ(拡張用)
            基本6DOF = 1,
            P2P = 2,
            円錐回転 = 3,
            スライダー = 5,
            ヒンジ = 6,
        }
        public ジョイント種別 種別 { get; private set; }

        public ジョイントパラメータ パラメータ { get; private set; }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ジョイント 読み込む( Stream fs, PMXヘッダ header )
        {
            var joint = new ジョイント();

            joint.ジョイント名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            joint.ジョイント名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            joint.種別 = (ジョイント種別) ParserHelper.get_Byte( fs );
            joint.パラメータ = ジョイントパラメータ.読み込む( fs, header, joint.種別 );

            return joint;
        }
    }
}
