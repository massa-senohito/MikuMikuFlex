using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class 材質リスト : List<材質>
    {
        public 材質リスト()
            : base()
        {
        }
        public 材質リスト( int capacity )
            : base( capacity )
        {
        }

        public 材質 指定された位置の材質を返す( int index )
        {
            int 面数 = 0;

            for( int i = 0; i < this.Count; i++ )
            {
                面数 += this[ i ].頂点数 / 3;

                if( index < 面数 )
                    return this[ i ];
            }

            throw new InvalidDataException();
        }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 材質リスト 読み込む( FileStream fs, PMXヘッダ header )
        {
            int 材質数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"材質数: {材質数}" );

            var list = new 材質リスト( 材質数 );

            for( int i = 0; i < 材質数; i++ )
                list.Add( 材質.読み込む( fs, header ) );

            return list;
        }
    }
}
