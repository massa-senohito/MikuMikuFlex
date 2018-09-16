using System;
using System.Diagnostics;

namespace MikuMikuFlex
{
	/// <summary>
	///     モーションに関するタイマー
	/// </summary>
	public class モーションタイマ
	{
		public int 秒間フレーム数 = 30;

		/// <summary>
		///     タイマーの更新速度
		///     理想とするfps
		///     一般的には60
		/// </summary>
		public int TimerPerSecond = 300;

		public float 経過時間ms { get; private set; }

		public static Stopwatch stopWatch;


		static モーションタイマ()
		{
			stopWatch = new Stopwatch();
			stopWatch.Start();
		}

        public モーションタイマ()
		{
		}

        /// <summary>
        ///     まだ経過していないなら何もしない。
        /// </summary>
		public void 一定時間が経過していればActionを行う( Action 一定時間ごとに行う処理 )
        {
            if( _最後に計測した時刻ms == 0 )
            {
                _最後に計測した時刻ms = stopWatch.ElapsedMilliseconds;
            }
            else
            {
                long 現在時刻ms = stopWatch.ElapsedMilliseconds;

                if( ( 現在時刻ms - _最後に計測した時刻ms ) > 1000 / TimerPerSecond )
                {
                    this.経過時間ms = 現在時刻ms - _最後に計測した時刻ms;

                    一定時間ごとに行う処理();

                    _最後に計測した時刻ms = stopWatch.ElapsedMilliseconds;
                }
            }
        }


        private long _最後に計測した時刻ms = 0;
    }
}