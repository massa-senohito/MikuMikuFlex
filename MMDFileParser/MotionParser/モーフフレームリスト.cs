using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class モーフフレームリスト : List<モーフフレーム>
    {
        public モーフフレームリスト()
            : base()
        {
        }
        public モーフフレームリスト( int capacity )
            : base( capacity )
        {

        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static モーフフレームリスト 読み込む( Stream fs )
        {
            var モーフフレーム数 = (int) ParserHelper.get_DWORD( fs );

            var list = new モーフフレームリスト( モーフフレーム数 );

            for( int i = 0; i < モーフフレーム数; i++ )
                list.Add( モーフフレーム.読み込む( fs ) );

            return list;
        }
    }
}
