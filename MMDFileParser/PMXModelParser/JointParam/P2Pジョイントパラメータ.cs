using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.JointParam
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
        internal override void 読み込む( Stream fs, PMXヘッダ header )
        {
            関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            位置 = ParserHelper.get_Float3( fs );
            回転rad = ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );   // 使わないフィールドをスキップ
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
        }
    }
}
