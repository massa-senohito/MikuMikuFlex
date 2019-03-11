using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMX
{
    /// <summary>
    ///     ボーン／モーフを共通化して格納可能。
    ///     PMD/PMXエディタでは Root用とPMD互換の表情枠を特殊枠として初期配置される。
    /// </summary>
    /// <remarks>
    /// ※PMXの初期状態では、
    ///     表示枠:0(先頭) -> "Root"(特殊枠指定) | 枠内に ボーン:0(先頭ボーン) を追加。対応されれば枠のルート位置への設定用
    ///     表示枠:1       -> "表情"(特殊枠指定) | PMD変換時は枠内に 表情枠 と同様の配置( 一部複製処理などで自動的に追加される場合あり)
    ///   という特殊枠が配置されます。特殊枠判定は、特殊枠フラグ1及び枠名で判断( 編集時に誤って削除しないように注意)
    /// </remarks>
    public class 表示枠
    {
        /// <summary>
        ///     "Roo" または "表情" なら特殊枠。
        /// </summary>
        public string 枠名 { get; private set; }

        public string 枠名_英 { get; private set; }

        /// <summary>
        ///     0:通常枠
        ///     1:特殊枠 ... "Root" または "表情"（PMD互換）
        /// </summary>
        public bool 特殊枠フラグ { get; private set; }

        public List<枠内要素> 枠内要素リスト { get; private set; }


        public 表示枠()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 表示枠( Stream fs, ヘッダ header )
        {
            this.枠名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.枠名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.特殊枠フラグ = ParserHelper.get_Byte( fs ) == 1;
            int 枠内要素数 = ParserHelper.get_Int( fs );
            this.枠内要素リスト = new List<枠内要素>( 枠内要素数 );
            for( int i = 0; i < 枠内要素数; i++ )
                this.枠内要素リスト.Add( new 枠内要素( fs, header ) );
        }
    }
}
