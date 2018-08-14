using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
{
    public class カメラフレームリスト : List<カメラフレーム>
    {
        public カメラフレームリスト()
            :base()
        {
        }
        public カメラフレームリスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static カメラフレームリスト 読み込む( Stream fs )
        {
            var カメラフレーム数 = (int) ParserHelper.get_DWORD( fs );

            var list = new カメラフレームリスト( カメラフレーム数 );

            for( int i = 0; i < カメラフレーム数; i++ )
                list.Add( カメラフレーム.読み込む( fs ) );

            return list;
        }
    }
}
