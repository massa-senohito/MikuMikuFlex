using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class BezierRotationAnimationTransition : AnimeTransition<Quaternion>
    {
        public BezierRotationAnimationTransition( Quaternion TargetValue, double Durationsec, VMDFormat.BezierCurve BezierCurve )
        {
            this.StartingValue = Quaternion.Identity;
            this.EndValue = TargetValue;
            this.Durationsec = Durationsec;
            this._BezierCurve = BezierCurve;
        }

        // 遷移が完了しているなら false を返す。
        internal override bool Update( double CurrentTimesec, out Quaternion CurrentValue )
        {
            Debug.Assert( !this.NotConfirmed, "TheTransitionHasNotStarted。" );

            if( CurrentTimesec >= this.StartTimesec + this.Durationsec )
            {
                CurrentValue = this.EndValue;
                return false;   // 遷移終了済み
            }

            float Px = (float) ( ( CurrentTimesec - this.StartTimesec ) / this.Durationsec );    // 0→1
            float Py = this._BezierCurve.HorizontalPositionPxに対応する縦位置Pyを返す( Px );                // 0→1

            CurrentValue = Quaternion.Slerp( this.StartingValue, this.EndValue, Py );

            return true;
        }


        private VMDFormat.BezierCurve _BezierCurve;
    }
}
