using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class BezierMovingAnimationTransition : AnimeTransition<Vector3>
    {
        public BezierMovingAnimationTransition( Vector3 TargetValue, double Durationsec, VMDFormat.BezierCurve BezierCurveX, VMDFormat.BezierCurve BezierCurveY, VMDFormat.BezierCurve BezierCurveZ )
        {
            this.EndValue = TargetValue;
            this.Durationsec = Durationsec;
            this._BezierCurveX = BezierCurveX;
            this._BezierCurveY = BezierCurveY;
            this._BezierCurveZ = BezierCurveZ;
        }

        // 遷移が完了しているなら false を返す。
        internal override bool Update( double CurrentTimesec, out Vector3 CurrentValue )
        {
            Debug.Assert( !this.NotConfirmed, "TheTransitionHasNotStarted。" );

            if( CurrentTimesec >= this.StartTimesec + this.Durationsec )
            {
                CurrentValue = this.EndValue;
                return false;   // 遷移終了済み
            }

            float Px = (float) ( ( CurrentTimesec - this.StartTimesec ) / this.Durationsec );    // 0→1
            float Pyx = this._BezierCurveX.HorizontalPositionPxに対応する縦位置Pyを返す( Px );              // 0→1
            float Pyy = this._BezierCurveY.HorizontalPositionPxに対応する縦位置Pyを返す( Px );              // 0→1
            float Pyz = this._BezierCurveZ.HorizontalPositionPxに対応する縦位置Pyを返す( Px );              // 0→1

            CurrentValue = new Vector3(
                this.StartingValue.X + ( this.EndValue.X - this.StartingValue.X ) * Pyx,
                this.StartingValue.Y + ( this.EndValue.Y - this.StartingValue.Y ) * Pyy,
                this.StartingValue.Z + ( this.EndValue.Z - this.StartingValue.Z ) * Pyz );

            return true;
        }


        private VMDFormat.BezierCurve _BezierCurveX;

        private VMDFormat.BezierCurve _BezierCurveY;

        private VMDFormat.BezierCurve _BezierCurveZ;
    }
}
