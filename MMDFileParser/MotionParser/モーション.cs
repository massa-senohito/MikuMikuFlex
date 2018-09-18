using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.MotionParser
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


		/// <summary>
		///     指定されたストリームから読み込む。
		/// </summary>
		public static モーション 読み込む( Stream fs )
        {
            var motion = new モーション();

            motion.ヘッダ = ヘッダ.読み込む( fs );
            motion.ボーンフレームリスト = ボーンフレームリスト.読み込む( fs );
            motion.モーフフレームリスト = モーフフレームリスト.読み込む( fs );
            motion.カメラフレームリスト = カメラフレームリスト.読み込む( fs );
            motion.照明フレームリスト = 照明フレームリスト.読み込む( fs );
            // 拡張
            //motion.セルフ影リスト = セルフ影リスト.読み込む( fs );

            return motion;
        }
    }
}
