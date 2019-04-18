using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class モーフフレームリスト : List<モーフフレーム>
    {
        public モーフフレームリスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal モーフフレームリスト( Stream fs )
        {
            var モーフフレーム数 = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = モーフフレーム数;

            for( int i = 0; i < モーフフレーム数; i++ )
                this.Add( new モーフフレーム( fs ) );
        }
    }
}
