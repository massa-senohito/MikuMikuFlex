using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class GroupMorphOffset : MorphOffset
    {
        /// <summary>
        ///     ※仕様上グループモーフのグループ化は非対応とする
        /// </summary>
        public int MorphIndex { get; private set; }

        /// <summary>
        ///     対象モーフのモーフ値 ＝ グループモーフのモーフ値 × 対象モーフの影響度
        /// </summary>
        public float Impact { get; private set; }


        public GroupMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal GroupMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Group;
            this.MorphIndex = ParserHelper.get_Index( st, header.MorphIndexSize );
            this.Impact = ParserHelper.get_Float( st );
        }
    }
}
