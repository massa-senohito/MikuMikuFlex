using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class ジョイントリスト : List<ジョイント>
    {
        public ジョイントリスト()
            : base()
        {
        }
        public ジョイントリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ジョイントリスト 読み込む( Stream fs, PMXヘッダ header )
        {
            int ジョイント数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"ジョイント数: {ジョイント数}" );

            var list = new ジョイントリスト( ジョイント数 );

            for( int i = 0; i < ジョイント数; i++ )
            {
                list.Add( ジョイント.読み込む( fs, header ) );
            }

            return list;
        }
    }
}
