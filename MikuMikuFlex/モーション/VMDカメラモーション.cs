using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using MMDFileParser.MotionParser;

namespace MikuMikuFlex
{
	/// <summary>
	///     VMDファイルを用いて動かすカメラ用のモーションを管理するクラス
	/// </summary>
	public class VMDカメラモーション : カメラモーション
	{
        public float 現在のフレーム番号 { get; set; } = 0;

        public float 最終のフレーム番号 { get; }


		/// <summary>
		///     コンストラクタ
		///     モーションを再生する際は、インスタンス作成の後Startメソッドを呼ぶこと
		/// </summary>
		/// <param name="VMDモーション">VMDファイルのデータ</param>
		public VMDカメラモーション( MMDFileParser.MotionParser.モーション VMDモーション )
		{
            _カメラフレームリスト = VMDモーション.カメラフレームリスト;
			_カメラフレームリスト.Sort( new カメラフレーム() );

            _stopWatch = new Stopwatch();

            if( 0 == _カメラフレームリスト.Count )
                最終のフレーム番号 = 0;
			else
				最終のフレーム番号 = _カメラフレームリスト.Last().フレーム番号;
		}

        public void モーションを再生する( float 開始フレーム番号 = 0, bool 反復再生する = false )
		{
			現在のフレーム番号 = 開始フレーム番号;

            _再生中 = true;
			_反復再生する = 反復再生する;

            _stopWatch.Start();
        }

        public void モーションを停止する()
		{
			_stopWatch.Stop();
			_再生中 = false;
		}

		public void モーションを更新する( カメラ camera, 射影 projection )
		{
			if( 0 == _前回の時刻msec )
			{
				_前回の時刻msec = _stopWatch.ElapsedMilliseconds;
			}
			else
			{
                // フレーム番号を更新

				long 現在の時刻msec = _stopWatch.ElapsedMilliseconds;
                long 経過時間msec = 現在の時刻msec - _前回の時刻msec;

				if( _再生中 )
                    現在のフレーム番号 += 経過時間msec / 30f;

				if( _反復再生する && 最終のフレーム番号 < 現在のフレーム番号 )
                    現在のフレーム番号 = 0;

				_前回の時刻msec = 現在の時刻msec;
			}

            if( 0 == _カメラフレームリスト.Count )
                return;

            // フレームの更新は、現在のフレーム番号で進行度合いを測る。

            for( int i = 0; i < _カメラフレームリスト.Count - 1; i++ )
            {
                if( _カメラフレームリスト[ i ].フレーム番号 < 現在のフレーム番号 &&
                    _カメラフレームリスト[ i + 1 ].フレーム番号 >= 現在のフレーム番号 )
                {
                    // frame は [i] と [i+1] の間にある

                    uint フレーム間隔 =
                        _カメラフレームリスト[ i + 1 ].フレーム番号 -
                        _カメラフレームリスト[ i ].フレーム番号;

                    float 進行度合い0to1 = ( 現在のフレーム番号 - _カメラフレームリスト[ i ].フレーム番号 ) / (float) フレーム間隔; // 0.0～1.0

                    _フレームを更新する( _カメラフレームリスト[ i ], _カメラフレームリスト[ i + 1 ], camera, projection, 進行度合い0to1 );

                    return;
                }
            }

            // returnされなかったとき＝つまり最終フレーム以降のとき

            _フレームを更新する( _カメラフレームリスト.Last(), _カメラフレームリスト.Last(), camera, projection, 0 );
        }


        public static VMDカメラモーション ファイルから読み込む( string path )
        {
            return new VMDカメラモーション( MMDFileParser.MotionParser.モーション.読み込む( File.OpenRead( path ) ) );
        }

        
        private List<カメラフレーム> _カメラフレームリスト;

        private Stopwatch _stopWatch;

        private long _前回の時刻msec;

        private bool _再生中 = false;

        private bool _反復再生する;


        private void _フレームを更新する( カメラフレーム cameraFrame1, カメラフレーム cameraFrame2, カメラ camera, 射影 projection, float 進行度合い0to1 )
        {
            float ProgX = cameraFrame1.ベジェ曲線[ 0 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );
            float ProgY = cameraFrame1.ベジェ曲線[ 1 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );
            float ProgZ = cameraFrame1.ベジェ曲線[ 2 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );
            float ProgR = cameraFrame1.ベジェ曲線[ 3 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );
            float ProgL = cameraFrame1.ベジェ曲線[ 4 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );
            float ProgP = cameraFrame1.ベジェ曲線[ 5 ].横位置Pxに対応する縦位置Pyを返す( 進行度合い0to1 );

            // 注視点

            camera.カメラの注視点 = CGHelper.ComplementTranslate(
                cameraFrame1, 
                cameraFrame2,
                new Vector3( ProgX, ProgY, ProgZ ) );


            // 位置

            Quaternion rotation = CGHelper.ComplementRotateQuaternion( cameraFrame1, cameraFrame2, ProgR );
            float length = CGHelper.Lerp( cameraFrame1.距離, cameraFrame2.距離, ProgL );
            Vector3 Position2target = Vector3.TransformCoordinate( new Vector3( 0, 0, 1 ), Matrix.RotationQuaternion( rotation ) );
            camera.カメラの位置 = camera.カメラの注視点 + length * Position2target;


            // 射影

            float angle = CGHelper.Lerp( cameraFrame1.視野角, cameraFrame2.視野角, ProgP );
            projection.視野角rad = CGHelper.ToRadians( angle );
        }
    }
}
