using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     Bourne／モーフを共通化して格納可能。
    ///     PMD/PMXエディタでは Root用とPMD互換の表情枠を特殊枠として初期配置される。
    /// </summary>
    /// <remarks>
    /// ※PMXの初期状態では、
    ///     DisplayFrame:0(先頭) -> "Root"(特殊枠指定) | 枠内に Bourne:0(先頭ボーン) を追加。対応されれば枠のルート位置への設定用
    ///     DisplayFrame:1       -> "表情"(特殊枠指定) | PMD変換時は枠内に 表情枠 と同様の配置( 一部複製処理などで自動的に追加される場合あり)
    ///   という特殊枠が配置されます。特殊枠判定は、SpecialFrameFlag1及び枠名で判断( 編集時に誤って削除しないように注意)
    /// </remarks>
    public class DisplayFrame
    {
        /// <summary>
        ///     "Roo" または "表情" なら特殊枠。
        /// </summary>
        public string FrameName { get; private set; }

        public string FrameName_English { get; private set; }

        /// <summary>
        ///     0:通常枠
        ///     1:特殊枠 ... "Root" または "表情"（PMD互換）
        /// </summary>
        public bool SpecialFrameFlag { get; private set; }

        public List<ElementsInTheFrame> InFrameElementList { get; private set; }


        public DisplayFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal DisplayFrame( Stream fs, Header header )
        {
            this.FrameName = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.FrameName_English = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.SpecialFrameFlag = ParserHelper.get_Byte( fs ) == 1;
            int NumberOfElementsInTheFrame = ParserHelper.get_Int( fs );
            this.InFrameElementList = new List<ElementsInTheFrame>( NumberOfElementsInTheFrame );
            for( int i = 0; i < NumberOfElementsInTheFrame; i++ )
                this.InFrameElementList.Add( new ElementsInTheFrame( fs, header ) );
        }
    }
}
