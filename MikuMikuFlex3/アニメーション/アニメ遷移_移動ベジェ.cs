using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class アニメ遷移_移動ベジェ : アニメ遷移<Vector3>
    {
        public アニメ遷移_移動ベジェ( Vector3 目標値, double 持続時間sec, VMDFormat.ベジェ曲線 ベジェ曲線X, VMDFormat.ベジェ曲線 ベジェ曲線Y, VMDFormat.ベジェ曲線 ベジェ曲線Z )
        {
            this.終了値 = 目標値;
            this.持続時間sec = 持続時間sec;
            this._ベジェ曲線X = ベジェ曲線X;
            this._ベジェ曲線Y = ベジェ曲線Y;
            this._ベジェ曲線Z = ベジェ曲線Z;
        }

        // 遷移が完了しているなら false を返す。
        public override bool 更新する( double 現在時刻sec, out Vector3 現在の値 )
        {
            Debug.Assert( !this.確定されていない, "遷移が開始されていません。" );

            if( 現在時刻sec >= this.開始時刻sec + this.持続時間sec )
            {
                現在の値 = this.終了値;
                return false;   // 遷移終了済み
            }

            float Px = (float) ( ( 現在時刻sec - this.開始時刻sec ) / this.持続時間sec );    // 0→1
            float Pyx = this._ベジェ曲線X.横位置Pxに対応する縦位置Pyを返す( Px );              // 0→1
            float Pyy = this._ベジェ曲線Y.横位置Pxに対応する縦位置Pyを返す( Px );              // 0→1
            float Pyz = this._ベジェ曲線Z.横位置Pxに対応する縦位置Pyを返す( Px );              // 0→1

            現在の値 = new Vector3(
                this.開始値.X + ( this.終了値.X - this.開始値.X ) * Pyx,
                this.開始値.Y + ( this.終了値.Y - this.開始値.Y ) * Pyy,
                this.開始値.Z + ( this.終了値.Z - this.開始値.Z ) * Pyz );

            return true;
        }


        private VMDFormat.ベジェ曲線 _ベジェ曲線X;

        private VMDFormat.ベジェ曲線 _ベジェ曲線Y;

        private VMDFormat.ベジェ曲線 _ベジェ曲線Z;
    }
}
