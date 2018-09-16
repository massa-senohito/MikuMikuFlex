using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class 面リスト : List<面>
    {
        public 面リスト()
            : base()
        {
        }
        public 面リスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 面リスト 読み込む( FileStream fs, PMXヘッダ header )
        {
            int 面数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"面数: {面数 / 3}" );

            var list = new 面リスト( 面数 / 3 );

            for( int i = 0; i < 面数 / 3; i++ )
            {
                list.Add(
                    new 面(
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ),
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ),
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ) ) );
            }

            return list;
        }
    }
}
