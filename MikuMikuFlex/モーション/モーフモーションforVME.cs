using System;
using System.Collections.Generic;
using System.Linq;
using OpenMMDFormat;

namespace MikuMikuFlex.モーション
{
	/// <summary>
	/// ひとつのモーフのモーションの集合(VME版)
	/// フレームの昇順に並んでいる
	/// </summary>
	class モーフモーションforVME
	{
		public string モーフ名 { get; private set; }


		public モーフモーションforVME( string morphName, List<MorphFrame> morphFrames )
		{
			this.モーフ名 = モーフ名;
			foreach( var morphFrame in morphFrames ) _frameManager.フレームデータを追加する( morphFrame );
			if( !_frameManager.フレームは昇順に並んでいる ) throw new Exception( "VMEデータがソートされていません" );
		}

		public float 指定したフレームにおけるモーフ値を取得する( ulong フレーム )
		{
			// 現在のフレームの前後のキーフレームを探す
			MMDFileParser.IFrameData 前フレーム, 後フレーム;

			_frameManager.現在のフレームの前後のキーフレームを探して返す( フレーム, out 前フレーム, out 後フレーム );

			var pastMorphFrame = (MorphFrame) 前フレーム;
			var futureMorphFrame = (MorphFrame) 後フレーム;

			// 現在のフレームの前後キーフレーム間での進行度を求める
			float s = ( futureMorphFrame.frameNumber == pastMorphFrame.frameNumber ) ? 0 :
				(float) ( フレーム - pastMorphFrame.frameNumber ) / (float) ( futureMorphFrame.frameNumber - pastMorphFrame.frameNumber ); // 進行度

			// 線形補完で値を求める
			return CGHelper.Lerp( pastMorphFrame.value, futureMorphFrame.value, s );
		}


        private MMDFileParser.FrameManager _frameManager = new MMDFileParser.FrameManager();
    }
}
