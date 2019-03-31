using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public abstract class アニメ遷移<T>
    {
        public bool 確定されていない => ( this._開始時刻sec < 0 );


        public void 確定する( double 開始時刻sec, T 開始値 )
        {
            this._開始時刻sec = 開始時刻sec;
            this._開始値 = 開始値;
        }

        // 遷移が完了しているなら false を返す。
        public abstract bool 更新する( double 現在時間sec, out T 現在の値 );



        // protected


        protected double _開始時刻sec = -1;

        protected double _持続時間sec = 0; // 確定時までに、派生クラスで設定すること。

        protected T _開始値;

        protected T _終了値;       // 確定時までに、派生クラスで設定すること。
    }
}
