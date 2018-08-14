using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class 表示枠リスト : List<表示枠>
    {
        public 表示枠リスト()
            : base()
        {
        }
        public 表示枠リスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 表示枠リスト 読み込む( Stream fs, PMXヘッダ header )
        {
            int 表示枠数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"表示枠数: {表示枠数}" );

            var list = new 表示枠リスト( 表示枠数 );

            for( int i = 0; i < 表示枠数; i++ )
                list.Add( 表示枠.読み込む( fs, header ) );

            return list;
        }
    }
}
