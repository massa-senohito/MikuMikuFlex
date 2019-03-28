using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class 剛体リスト : List<剛体>
    {
        public 剛体リスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 剛体リスト( Stream st, ヘッダ header )
        {
            int 剛体数 = ParserHelper.get_Int( st );
            Debug.WriteLine( $"剛体数: {剛体数}" );

            this.Capacity = 剛体数;

            for( int i = 0; i < 剛体数; i++ )
                this.Add( new 剛体( st, header ) );
        }
    }
}
