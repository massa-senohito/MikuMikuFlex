using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMX
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
        internal ジョイントリスト( Stream fs, ヘッダ header )
        {
            int ジョイント数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"ジョイント数: {ジョイント数}" );

            this.Capacity = ジョイント数;

            for( int i = 0; i < ジョイント数; i++ )
                this.Add( new ジョイント( fs, header ) );
        }
    }
}
