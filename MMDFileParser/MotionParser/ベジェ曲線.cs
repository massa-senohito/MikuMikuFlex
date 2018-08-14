using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.MotionParser
{
    public class ベジェ曲線
    {
        public Vector2 v1;

        public Vector2 v2;


        public float 進行度合いに対応する移行度合いを返す( float 進行度合い )
        {
            // ニュートン法による近似
            float t = CGHelper.値を最小値と最大値の範囲に収める( 進行度合い, 0, 1 );
            float dt;
            do
            {
                dt = -( _fx( t ) - 進行度合い ) / _dfx( t );
                if( float.IsNaN( dt ) )
                    break;
                t += CGHelper.値を最小値と最大値の範囲に収める( dt, -1f, 1f ); // 大幅に移動して別の解に到達するのを防止する用

            } while( Math.Abs( dt ) > _Epsilon );

            return CGHelper.値を最小値と最大値の範囲に収める( _fy( t ), 0f, 1f );   // 念のため、0-1の間に収まるようにした
        }


        private const float _Epsilon = 1.0e-3f;

        // fy(t)を計算する関数
        private float _fy( float t )
        {
            //fy(t)=(1-t)^3*0+3*(1-t)^2*t*v1.y+3*(1-t)*t^2*v2.y+t^3*1
            return 3 * ( 1 - t ) * ( 1 - t ) * t * v1.Y + 3 * ( 1 - t ) * t * t * v2.Y + t * t * t;
        }

        // fx(t)を計算する関数
        private float _fx( float t )
        {
            //fx(t)=(1-t)^3*0+3*(1-t)^2*t*v1.x+3*(1-t)*t^2*v2.x+t^3*1
            return 3 * ( 1 - t ) * ( 1 - t ) * t * v1.X + 3 * ( 1 - t ) * t * t * v2.X + t * t * t;
        }

        // dfx/dtを計算する関数
        private float _dfx( float t )
        {
            //dfx(t)/dt=-6(1-t)*t*v1.x+3(1-t)^2*v1.x-3t^2*v2.x+6(1-t)*t*v2.x+3t^2
            return -6 * ( 1 - t ) * t * v1.X + 3 * ( 1 - t ) * ( 1 - t ) * v1.X - 3 * t * t * v2.X + 6 * ( 1 - t ) * t * v2.X + 3 * t * t;
        }
    }
}
