using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class 照明フレームリスト : List<照明フレーム>
    {
        public 照明フレームリスト()
            :base()
        {
        }
        public 照明フレームリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 照明フレームリスト 読み込む( Stream fs )
        {
            var 照明フレーム数 = (int) ParserHelper.get_DWORD( fs );

            var list = new 照明フレームリスト( 照明フレーム数 );

            for( int i = 0; i < 照明フレーム数; i++ )
                list.Add( 照明フレーム.読み込む( fs ) );

            return list;
        }
    }
}
