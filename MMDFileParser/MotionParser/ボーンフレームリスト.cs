using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class ボーンフレームリスト : List<ボーンフレーム>
    {
        public ボーンフレームリスト()
            : base()
        {
        }
        public ボーンフレームリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ボーンフレームリスト 読み込む( Stream fs )
        {
            var ボーンフレーム数 = (int) ParserHelper.get_DWORD( fs );

            var list = new ボーンフレームリスト( ボーンフレーム数 );

            for( int i = 0; i < ボーンフレーム数; i++ )
            {
                list.Add( ボーンフレーム.読み込む( fs ) );
            }

            return list;
        }
    }
}
