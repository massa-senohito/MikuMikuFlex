using System.Collections.Generic;
using System.Linq;
using MMDFileParser.MotionParser;
using MMF.Utility;

namespace MMF.モーション
{
	public class モーフモーション
	{
		public string モーフ名 { get; private set; }


        public モーフモーション( string モーフ名 )
		{
			this.モーフ名 = モーフ名;
		}

		public void モーフフレームデータを付け加える( モーフフレーム morphFrameData )
		{
			_frameManager.フレームデータを追加する( morphFrameData );
		}

		public void モーフフレームデータリストをソートする()
		{
			_frameManager.フレームデータを昇順にソートする();
		}

		public float 指定したフレームにおけるモーフ値を取得する( float フレーム )
		{
			// 現在のフレームの前後のキーフレームを探す
			MMDFileParser.IFrameData 前フレーム, 後フレーム;

            _frameManager.現在のフレームの前後のキーフレームを探して返す( フレーム, out 前フレーム, out 後フレーム );

			var pastMorphFrame = (モーフフレーム) 前フレーム;
			var futureMorphFrame = (モーフフレーム) 後フレーム;

			// 現在のフレームの前後キーフレーム間での進行度を求めてペジェ関数で変換する
			float s = ( futureMorphFrame.フレーム番号 == pastMorphFrame.フレーム番号 ) ? 0 :
				(float) ( フレーム - pastMorphFrame.フレーム番号 ) / (float) ( futureMorphFrame.フレーム番号 - pastMorphFrame.フレーム番号 ); // 進行度

			return CGHelper.Lerp( pastMorphFrame.モーフ値, futureMorphFrame.モーフ値, s );
		}


        private MMDFileParser.FrameManager _frameManager = new MMDFileParser.FrameManager();
    }
}
