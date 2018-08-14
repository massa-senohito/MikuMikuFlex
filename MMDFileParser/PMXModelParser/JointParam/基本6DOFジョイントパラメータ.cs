using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.JointParam
{
    public class 基本6DOFジョイントパラメータ : ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        /// <summary>
        ///     (x,y,z) -> ラジアン角
        /// </summary>
        public Vector3 回転 { get; private set; }

        public Vector3 移動制限の下限 { get; private set; }

        public Vector3 移動制限の上限 { get; private set; }

        /// <summary>
        ///     (x,y,z) -> ラジアン角
        /// </summary>
        public Vector3 回転制限の下限 { get; private set; }

        /// <summary>
        ///     (x,y,z) -> ラジアン角
        /// </summary>
        public Vector3 回転制限の上限 { get; private set; }


        internal override void 読み込む( Stream fs, PMXヘッダ header )
        {
            関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            位置 = ParserHelper.get_Float3( fs );
            回転 = ParserHelper.get_Float3( fs );
            移動制限の下限 = ParserHelper.get_Float3( fs );
            移動制限の上限 = ParserHelper.get_Float3( fs );
            回転制限の下限 = ParserHelper.get_Float3( fs );
            回転制限の上限 = ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );   //6DOF は、 回転・平行移動バネ定数が無効なので読み取ってシーク
            ParserHelper.get_Float3( fs );
        }
    }
}
