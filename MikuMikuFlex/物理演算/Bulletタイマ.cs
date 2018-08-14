using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMF.物理演算
{
	/// <summary>
	///     経過時間[ms]を計るクラス
	/// </summary>
	internal class Bulletタイマ
	{
		public Bulletタイマ()
		{
			_stopWatch.Start();
		}

		public long 経過時間msを返す()
		{
			var currentTime = _stopWatch.ElapsedMilliseconds;
			var elapsedTime = currentTime - _前回時計を見た時の時刻ms;

            _前回時計を見た時の時刻ms = currentTime;

            return elapsedTime;
		}


        private System.Diagnostics.Stopwatch _stopWatch = new System.Diagnostics.Stopwatch();

        private long _前回時計を見た時の時刻ms = 0;
    }
}
