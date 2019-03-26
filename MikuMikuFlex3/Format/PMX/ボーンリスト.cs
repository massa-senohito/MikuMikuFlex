using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class ボーンリスト : List<ボーン>
    {
        public ボーンリスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ボーンリスト( FileStream fs, ヘッダ header )
        {
            int ボーン数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"ボーン数: {ボーン数}" );

            this.Capacity = ボーン数;

            for( int i = 0; i < ボーン数; i++ )
                this.Add( new ボーン( fs, header ) );
        }
    }
}
