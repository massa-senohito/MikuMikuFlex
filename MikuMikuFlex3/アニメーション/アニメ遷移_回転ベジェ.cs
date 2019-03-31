using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class アニメ遷移_回転ベジェ : アニメ遷移<Quaternion>
    {
        public アニメ遷移_回転ベジェ( Quaternion 目標値, double 持続時間sec, VMDFormat.ベジェ曲線 ベジェ曲線 )
        {
            this._開始値 = Quaternion.Identity;
            this._終了値 = 目標値;
            this._持続時間sec = 持続時間sec;
            this._ベジェ曲線 = ベジェ曲線;
        }

        // 遷移が完了しているなら false を返す。
        public override bool 更新する( double 現在時間sec, out Quaternion 現在の値 )
        {
            Debug.Assert( !this.確定されていない, "遷移が開始されていません。" );

            if( 現在時間sec >= this._開始時刻sec + this._持続時間sec )
            {
                現在の値 = this._終了値;
                return false;   // 遷移終了済み
            }

            float Px = (float) ( ( 現在時間sec - this._開始時刻sec ) / this._持続時間sec );    // 0→1
            float Py = this._ベジェ曲線.横位置Pxに対応する縦位置Pyを返す( Px );                // 0→1

            現在の値 = Quaternion.Slerp( this._開始値, this._終了値, Py );

            return true;
        }


        private VMDFormat.ベジェ曲線 _ベジェ曲線;
    }
}
