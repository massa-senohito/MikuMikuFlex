using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class ベジェ実数アニメ遷移 : アニメ遷移<float>
    {
        public ベジェ実数アニメ遷移( float 目標値, double 持続時間sec, VMDFormat.ベジェ曲線 ベジェ曲線 )
        {
            this.終了値 = 目標値;
            this.持続時間sec = 持続時間sec;
            this._ベジェ曲線 = ベジェ曲線;
        }

        // 遷移が完了しているなら false を返す。
        public override bool 更新する( double 現在時刻sec, out float 現在の値 )
        {
            Debug.Assert( !this.確定されていない, "遷移が開始されていません。" );

            if( 現在時刻sec >= this.開始時刻sec + this.持続時間sec )
            {
                現在の値 = this.終了値;
                return false;   // 遷移終了済み
            }

            float Px = (float) ( ( 現在時刻sec - this.開始時刻sec ) / this.持続時間sec );    // 0→1
            float Py = this._ベジェ曲線.横位置Pxに対応する縦位置Pyを返す( Px );              // 0→1

            現在の値 = this.開始値 + ( this.終了値 - this.開始値 ) * Py;

            return true;
        }


        private VMDFormat.ベジェ曲線 _ベジェ曲線;
    }
}
