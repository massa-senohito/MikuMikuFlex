using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public abstract class アニメ遷移<T>
    {
        public double 開始時刻sec { get; protected set; } = -1;

        public double 持続時間sec { get; protected set; } = 0; // 確定時までに、派生クラスで設定すること。

        public T 開始値 { get; protected set; }

        public T 終了値 { get; protected set; }       // 確定時までに、派生クラスで設定すること。

        internal bool 確定されていない => ( this.開始時刻sec < 0 );


        internal void 確定する( double 開始時刻sec, T 開始値 )
        {
            this.開始時刻sec = 開始時刻sec;
            this.開始値 = 開始値;
        }

        // 遷移が完了しているなら false を返す。
        internal abstract bool 更新する( double 現在時刻sec, out T 現在の値 );
    }
}
