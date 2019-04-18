using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class 表示枠リスト : List<表示枠>
    {
        public 表示枠リスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 表示枠リスト( Stream st, ヘッダ header )
        {
            int 表示枠数 = ParserHelper.get_Int( st );
            Debug.WriteLine( $"表示枠数: {表示枠数}" );

            this.Capacity = 表示枠数;

            for( int i = 0; i < 表示枠数; i++ )
                this.Add( new 表示枠( st, header ) );
        }
    }
}
