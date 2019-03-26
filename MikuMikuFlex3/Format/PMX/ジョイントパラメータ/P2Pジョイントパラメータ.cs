using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     点結合ジョイント。
    /// </summary>
    public class P2Pジョイントパラメータ : ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void 読み込む( Stream fs, ヘッダ header )
        {
            this.関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.位置 = ParserHelper.get_Float3( fs );
            this.回転rad = ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );   // 使わないフィールドをスキップ
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
        }
    }
}
