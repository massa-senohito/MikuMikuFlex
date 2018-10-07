using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using MMDFileParser.MotionParser;

namespace MikuMikuFlex
{
	/// <summary>
	///     VMDモーションデータを使ってボーンを更新するクラス。
	/// </summary>
	internal class ボーンモーション
	{
        public String ボーン名
            => _ボーン.ボーン名;

        public uint 最後のフレーム番号
            => _frameManager.最後のフレーム番号を返す();


        public ボーンモーション( PMXボーン bone )
		{
			this._ボーン = bone;
		}

		public void ボーンフレームを追加する( ボーンフレーム boneFrameData )
		{
			_frameManager.フレームデータを追加する( boneFrameData );
		}

		public void ボーンフレームリストをソートする()
		{
			_frameManager.フレームデータを昇順にソートする();
		}

		public void ボーンを指定したフレームの姿勢に更新する( float 目標フレーム )
		{
            // 目標フレームの前後のキーフレームを探す

            _frameManager.現在のフレームの前後のキーフレームを探して返す( 目標フレーム, out MMDFileParser.IFrameData 過去フレーム, out MMDFileParser.IFrameData 未来フレーム );

            var 過去のボーンフレーム = (ボーンフレーム) 過去フレーム;
			var 未来のボーンフレーム = (ボーンフレーム) 未来フレーム;


            // 目標フレームの前後キーフレーム間での進行度を求めてペジェ関数で変換する

            float 進行度合0to1 = ( 未来のボーンフレーム.フレーム番号 == 過去のボーンフレーム.フレーム番号 ) ? 0 :
				(float) ( 目標フレーム - 過去のボーンフレーム.フレーム番号 ) / (float) ( 未来のボーンフレーム.フレーム番号 - 過去のボーンフレーム.フレーム番号 ); // リニア

            var 移行度合 = new float[ 4 ];

            for( int i = 0; i < 4; i++ )
                移行度合[ i ] = 過去のボーンフレーム.ベジェ曲線[ i ].横位置Pxに対応する縦位置Pyを返す( 進行度合0to1 );   // リニア → ベジェ[4]


            // ボーンを更新する

            _ボーン.移動 = CGHelper.ComplementTranslate( 過去のボーンフレーム, 未来のボーンフレーム, new Vector3( 移行度合[ 0 ], 移行度合[ 1 ], 移行度合[ 2 ] ) );

            _ボーン.回転 = CGHelper.ComplementRotateQuaternion( 過去のボーンフレーム, 未来のボーンフレーム, 移行度合[ 3 ] );
		}


        private PMXボーン _ボーン;

        private MMDFileParser.FrameManager _frameManager = new MMDFileParser.FrameManager();
    }
}