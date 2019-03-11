using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMD
{
    public class カメラフレームリスト : List<カメラフレーム>
    {
        public カメラフレームリスト()
            :base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal カメラフレームリスト( Stream fs )
        {
            var カメラフレーム数 = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = カメラフレーム数;

            for( int i = 0; i < カメラフレーム数; i++ )
                this.Add( new カメラフレーム( fs ) );
        }
    }
}
