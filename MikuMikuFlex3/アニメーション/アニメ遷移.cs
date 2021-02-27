using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public abstract class AnimeTransition<T>
    {
        public double StartTimesec { get; protected set; } = -1;

        public double Durationsec { get; protected set; } = 0; // 確定時までに、派生クラスで設定すること。

        public T StartingValue { get; protected set; }

        public T EndValue { get; protected set; }       // 確定時までに、派生クラスで設定すること。

        internal bool NotConfirmed => ( this.StartTimesec < 0 );


        internal void Determine( double StartTimesec, T StartingValue )
        {
            this.StartTimesec = StartTimesec;
            this.StartingValue = StartingValue;
        }

        // 遷移が完了しているなら false を返す。
        internal abstract bool Update( double CurrentTimesec, out T CurrentValue );
    }
}
