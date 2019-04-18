using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class ボーンフレームリスト : List<ボーンフレーム>
    {
        public ボーンフレームリスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ボーンフレームリスト( Stream fs )
        {
            var ボーンフレーム数 = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = ボーンフレーム数;

            for( int i = 0; i < ボーンフレーム数; i++ )
                this.Add( new ボーンフレーム( fs ) );
        }
    }
}
