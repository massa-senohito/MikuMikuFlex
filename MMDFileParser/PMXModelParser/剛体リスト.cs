using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class 剛体リスト : List<剛体>
    {
        public 剛体リスト()
            : base()
        {
        }
        public 剛体リスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 剛体リスト 読み込む( Stream fs, PMXヘッダ header )
        {
            int 剛体数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"剛体数: {剛体数}" );

            var list = new 剛体リスト( 剛体数 );

            for( int i = 0; i < 剛体数; i++ )
                list.Add( 剛体.読み込む( fs, header ) );

            return list;
        }
    }
}
