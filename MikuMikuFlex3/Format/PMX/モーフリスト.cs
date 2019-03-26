using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class モーフリスト : List<モーフ>
    {
        public モーフリスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal モーフリスト( FileStream fs, ヘッダ header )
        {
            int モーフ数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"モーフ数: {モーフ数}" );

            this.Capacity = モーフ数;

            for( int i = 0; i < モーフ数; i++ )
                this.Add( new モーフ( fs, header ) );
        }
    }
}
