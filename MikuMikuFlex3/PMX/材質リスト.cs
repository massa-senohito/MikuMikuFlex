using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public class 材質リスト : List<材質>
    {
        public 材質リスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 材質リスト( FileStream fs, ヘッダ header )
        {
            int 材質数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"材質数: {材質数}" );

            this.Capacity = 材質数;

            for( int i = 0; i < 材質数; i++ )
                this.Add( new 材質( fs, header ) );
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
    }
}
