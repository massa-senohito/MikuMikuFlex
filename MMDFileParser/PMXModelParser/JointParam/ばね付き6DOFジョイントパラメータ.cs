using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.JointParam
{
    public class ばね付き6DOFジョイントパラメータ : ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }

        public Vector3 移動制限の下限 { get; private set; }

        public Vector3 移動制限の上限 { get; private set; }

        public Vector3 回転制限の下限rad { get; private set; }

        public Vector3 回転制限の上限rad { get; private set; }

        public Vector3 バネ移動定数 { get; private set; }

        public Vector3 バネ回転定数 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void 読み込む( Stream fs, PMXヘッダ header )
        {
            関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            位置 = ParserHelper.get_Float3( fs );
            回転rad = ParserHelper.get_Float3( fs );
            移動制限の下限 = ParserHelper.get_Float3( fs );
            移動制限の上限 = ParserHelper.get_Float3( fs );
            回転制限の下限rad = ParserHelper.get_Float3( fs );
            回転制限の上限rad = ParserHelper.get_Float3( fs );
            バネ移動定数 = ParserHelper.get_Float3( fs );
            バネ回転定数 = ParserHelper.get_Float3( fs );
        }
    }
}
