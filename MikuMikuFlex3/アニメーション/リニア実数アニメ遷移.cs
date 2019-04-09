using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public class リニア実数アニメ遷移 : アニメ遷移<float>
    {
        public リニア実数アニメ遷移( float 目標値, double 持続時間sec )
        {
            this.終了値 = 目標値;
            this.持続時間sec = 持続時間sec;
        }

        // 遷移が完了しているなら false を返す。
        internal override bool 更新する( double 現在時刻sec, out float 現在の値 )
        {
            Debug.Assert( !this.確定されていない, "遷移が開始されていません。" );

            if( 現在時刻sec >= this.開始時刻sec + this.持続時間sec )
            {
                現在の値 = this.終了値;
                return false;   // 遷移終了済み
            }

            var t = ( 現在時刻sec - this.開始時刻sec ) / this.持続時間sec;

            現在の値 = (float) ( this.開始値 + ( this.終了値 - this.開始値 ) * t );

            return true;
        }
    }
}
