using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
	/// <summary>
	///     経過時間[ms]を計るクラス
	/// </summary>
	internal class Bulletタイマ
	{
		public Bulletタイマ()
		{
			this._StopWatch.Start();
		}

		public long 経過時間msを返す()
		{
			var 現在時刻ms = this._StopWatch.ElapsedMilliseconds;
			var 経過時間ms = 現在時刻ms - this._前回時計を見た時の時刻ms;

            this._前回時計を見た時の時刻ms = 現在時刻ms;

            return 経過時間ms;
		}


        private Stopwatch _StopWatch = new Stopwatch();

        private long _前回時計を見た時の時刻ms = 0;
    }
}
