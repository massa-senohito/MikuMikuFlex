using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class 頂点リスト : List<頂点>
    {
        public 頂点リスト()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 頂点リスト( Stream st, ヘッダ header )
        {
            int 頂点数 = ParserHelper.get_Int( st );
            Debug.WriteLine( $"頂点数: {頂点数}" );

            this.Capacity = 頂点数;

            for( int i = 0; i < 頂点数; i++ )
                this.Add( new 頂点( st, header ) );
#if DEBUG
            int[] 出現数 = new int[ 5 ];
            for( int i = 0; i < 頂点数; i++ )
            {
                var ウェイト変換方式 = (byte) this[ i ].ウェイト変形方式;
                出現数[ ウェイト変換方式 ]++;
            }
            Debug.WriteLine( $"   " +
                $"BDEF1:{出現数[(int) ボーンウェイト種別.BDEF1]}, "+
                $"BDEF2:{出現数[ (int) ボーンウェイト種別.BDEF2 ]}, "+
                $"BDEF4:{出現数[ (int) ボーンウェイト種別.BDEF4 ]}, "+
                $"SDEF:{出現数[ (int) ボーンウェイト種別.SDEF ]}, "+
                $"QDEF:{出現数[ (int) ボーンウェイト種別.QDEF ]}" );
#endif
        }
    }
}
