using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public class LinearRealAnimationTransition : AnimeTransition<float>
    {
        public LinearRealAnimationTransition( float TargetValue, double Durationsec )
        {
            this.EndValue = TargetValue;
            this.Durationsec = Durationsec;
        }

        // 遷移が完了しているなら false を返す。
        internal override bool Update( double CurrentTimesec, out float CurrentValue )
        {
            Debug.Assert( !this.NotConfirmed, "TheTransitionHasNotStarted。" );

            if( CurrentTimesec >= this.StartTimesec + this.Durationsec )
            {
                CurrentValue = this.EndValue;
                return false;   // 遷移終了済み
            }

            var t = ( CurrentTimesec - this.StartTimesec ) / this.Durationsec;

            CurrentValue = (float) ( this.StartingValue + ( this.EndValue - this.StartingValue ) * t );

            return true;
        }
    }
}
