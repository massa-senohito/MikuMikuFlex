using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class 頂点リスト : List<頂点>
    {
        public 頂点リスト()
            : base()
        {
        }
        public 頂点リスト( int capacity )
            : base( capacity )
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 頂点リスト 読み込む( FileStream fs, PMXヘッダ header )
        {
            int 頂点数 = ParserHelper.get_Int( fs );
            Debug.WriteLine( $"頂点数: {頂点数}" );

            var list = new 頂点リスト( 頂点数 );

            for( int i = 0; i < 頂点数; i++ )
                list.Add( 頂点.読み込む( fs, header ) );
#if DEBUG
            int[] 出現数 = new int[ 5 ];
            for( int i = 0; i < 頂点数; i++ )
            {
                var ウェイト変換方式 = (byte) list[ i ].ウェイト変形方式;
                出現数[ (int) ウェイト変換方式 ]++;
            }
            Debug.WriteLine( $"   " +
                $"BDEF1:{出現数[(int)BoneWeight.変形方式.BDEF1]}, "+
                $"BDEF2:{出現数[ (int) BoneWeight.変形方式.BDEF2 ]}, "+
                $"BDEF4:{出現数[ (int) BoneWeight.変形方式.BDEF4 ]}, "+
                $"SDEF:{出現数[ (int) BoneWeight.変形方式.SDEF ]}, "+
                $"QDEF:{出現数[ (int) BoneWeight.変形方式.QDEF ]}" );
#endif
            return list;
        }
    }
}
