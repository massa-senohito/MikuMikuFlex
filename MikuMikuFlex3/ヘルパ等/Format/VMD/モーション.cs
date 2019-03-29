using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class モーション
    {
        public ヘッダ ヘッダ;

        public ボーンフレームリスト ボーンフレームリスト;

        public モーフフレームリスト モーフフレームリスト;

        public カメラフレームリスト カメラフレームリスト;

        public 照明フレームリスト 照明フレームリスト;

        // todo: VMDのセルフ影への対応
        //public セルフ影リスト セルフ影リスト;

        // todo: VMDのモデル表示・IK on/off への対応
        //public モデル表示_IKリスト モデル表示_IKリスト;


        public モーション()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        public モーション( Stream fs )
        {
            this.ヘッダ = new ヘッダ( fs );
            this.ボーンフレームリスト = new ボーンフレームリスト( fs );
            this.モーフフレームリスト = new モーフフレームリスト( fs );
            this.カメラフレームリスト = new カメラフレームリスト( fs );
            this.照明フレームリスト = new 照明フレームリスト( fs );
            //this.セルフ影リスト = new セルフ影リスト( fs );
            //this.モデル表示_IKリスト = new モデル表示_IKリスト( fs );
        }
    }
}