using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    class アニメ遷移_モーフリニア : アニメ遷移<float>
    {
        public アニメ遷移_モーフリニア( float 目標値, double 持続時間sec )
        {
            this._終了値 = 目標値;
            this._持続時間sec = 持続時間sec;
        }

        // 遷移が完了しているなら false を返す。
        public override bool 更新する( double 現在時間sec, out float 現在の値 )
        {
            Debug.Assert( !this.確定されていない, "遷移が開始されていません。" );

            if( 現在時間sec >= this._開始時刻sec + this._持続時間sec )
            {
                現在の値 = this._終了値;
                return false;   // 遷移終了済み
            }

            var t = ( 現在時間sec - this._開始時刻sec ) / this._持続時間sec;

            現在の値 = (float) ( this._開始値 + ( this._終了値 - this._開始値 ) * t );

            return true;
        }
    }
}
