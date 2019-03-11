using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public class 面リスト : List<面>
    {
        public 面リスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 面リスト( FileStream fs, ヘッダ header )
        {
            int 面数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"面数: {面数 / 3}" );

            this.Capacity = 面数 / 3;

            for( int i = 0; i < 面数 / 3; i++ )
            {
                this.Add(
                    new 面(
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ),
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ),
                        ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ ) ) );
            }
        }
    }
}
