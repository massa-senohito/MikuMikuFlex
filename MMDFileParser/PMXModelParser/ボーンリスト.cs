using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class ボーンリスト : List<ボーン>
    {
        public ボーンリスト()
            : base()
        {
        }
        public ボーンリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ボーンリスト 読み込む( FileStream fs, PMXヘッダ header )
        {
            int ボーン数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"ボーン数: {ボーン数}" );

            var list = new ボーンリスト( ボーン数 );

            for( int i = 0; i < ボーン数; i++ )
                list.Add( ボーン.読み込む( fs, header ) );

            return list;
        }
    }
}
