using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class 照明フレームリスト : List<照明フレーム>
    {
        public 照明フレームリスト()
            :base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 照明フレームリスト( Stream fs )
        {
            var 照明フレーム数 = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = 照明フレーム数;

            for( int i = 0; i < 照明フレーム数; i++ )
                this.Add( new 照明フレーム( fs ) );
        }
    }
}
