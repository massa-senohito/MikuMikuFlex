using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex.VMD
{
    public class モーション
    {
        public ヘッダ ヘッダ;

        public ボーンフレームリスト ボーンフレームリスト;

        public モーフフレームリスト モーフフレームリスト;

        public カメラフレームリスト カメラフレームリスト;

        public 照明フレームリスト 照明フレームリスト;

        // Todo: セルフ影には未対応
        //public セルフ影リスト セルフ影リスト;

        // TODO: モデル表示・IK on/off には未対応
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
            // 拡張
            //this.セルフ影リスト = new セルフ影リスト( fs );
        }
    }
}
