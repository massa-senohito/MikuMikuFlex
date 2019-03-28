using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class ジョイントリスト : List<ジョイント>
    {
        public ジョイントリスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ジョイントリスト( Stream st, ヘッダ header )
        {
            int ジョイント数 = ParserHelper.get_Int( st );
            Debug.WriteLine( $"ジョイント数: {ジョイント数}" );

            this.Capacity = ジョイント数;

            for( int i = 0; i < ジョイント数; i++ )
                this.Add( new ジョイント( st, header ) );
        }
    }
}
