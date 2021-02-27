using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
	/// <summary>
	///     ElapsedTime[ms]を計るクラス
	/// </summary>
	internal class BulletTimer
	{
		public BulletTimer()
		{
			this._StopWatch.Start();
		}

		public long ElapsedTimemsを返す()
		{
			var CurrentTimems = this._StopWatch.ElapsedMilliseconds;
			var ElapsedTimems = CurrentTimems - this._TheTimeWhenYouLastSawTheClockms;

            this._TheTimeWhenYouLastSawTheClockms = CurrentTimems;

            return ElapsedTimems;
		}


        private Stopwatch _StopWatch = new Stopwatch();

        private long _TheTimeWhenYouLastSawTheClockms = 0;
    }
}
