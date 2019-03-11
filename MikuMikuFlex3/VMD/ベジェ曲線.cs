using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.VMD
{
    /// <summary>
    ///     ３次ベジェ曲線。
    /// </summary>
    /// <remarks>
    ///     V0=(0,0) V1=V0側方向点, V2=V3側方向点, V3=(1,1) とし、V0 から V3へ至る３次ベジェ曲線を表す。
    ///     V1, V2 は、いずれも (0,0)-(1,1) の範囲内にあるものとする。
    ///     このとき、曲線上割合 t (0≦t≦1) における 点P の位置 P(t) は、次のように表される。
    ///         P(t) = V0*(1-t)^3 + 3*V1*(1-t)^2*t + 3*V2*(1-t)*t^2 + V3*t^3
    /// </remarks>
    /// <seealso cref="http://pr0jectze10.blogspot.com/2011/04/xnammd-part3.html"/>
    public class ベジェ曲線
    {
        /// <summary>
        ///     始点(0,0)側方向点。
        ///     (0,0)-(1,1) の範囲内にあるものとする。
        /// </summary>
        public Vector2 v1;

        /// <summary>
        ///     終点(1,1)側方向点。
        ///     (0,0)-(1,1) の範囲内にあるものとする。
        /// </summary>
        public Vector2 v2;


        /// <summary>
        ///     ベジェ曲線上の 点P (Px,Py) において、指定された Px から Py を求めて返す。
        /// </summary>
        /// <param name="Px">ベジェ補間曲線上の 点P の X値 Px を指定する。0.0～1.0。</param>
        /// <returns>指定された進行度合いに対応する、ベジェ補間曲線上の 点P の Y値 Py を表す。0.0～1.0。</returns>
        /// <remarks>
        ///     Px から Py を算出するには、まず Px に対応する 曲線上の割合 t を求め、
        ///     次に３次ベジェ曲線の式を使って t から Py を算出する。
        ///     t の算出には、ニュートン法による近似を使う。
        /// </remarks>
        public float 横位置Pxに対応する縦位置Pyを返す( float Px )
        {
            // (1) ニュートン法による近似を使って、Px に対応するベジェ補間曲線上の割合 t を算出する。

            float t = MathUtil.Clamp( Px, 0f, 1f );
            float dt;
            do
            {
                dt = -( _fx( t ) - Px ) / _dfx( t );

                if( float.IsNaN( dt ) )
                    break;

                t += MathUtil.Clamp( dt, -1f, 1f ); // 大幅に移動して別の解に到達するのを防止する用

            } while( Math.Abs( dt ) > _最小解像度 ); // dt が十分小さくなるまで繰り返す。


            // (2) ３次ベジェ曲線の式を使って、t に対応する Py を算出して返す。

            float Py = _fy( t );

            return MathUtil.Clamp( Py, 0f, 1f );   // 念のため、0-1の間に収まるようにした
        }


        private const float _最小解像度 = 1.0e-3f;


        // fy(t)を計算する関数
        private float _fy( float t )
        {
            // fy(t)
            //  = V0.Y*(1-t)^3 + 3*V1.Y*(1-t)^2*t + 3*V2.Y*(1-t)*t^2  + V3.Y*t^3
            //  = 0.0*(1-t)^3  + 3*V1.Y*(1-t)^2*t + 3*V2.y*(1-t)*t^2  + 1.0*t^3
            //  = 0            + 3*V1.Y*(1-t)^2*t + 3*V2.y*(1-t)*t^2  + t^3
            return
                0 +
                v1.Y * 3 * ( 1 - t ) * ( 1 - t ) * t +
                v2.Y * 3 * ( 1 - t ) * t * t +
                t * t * t;
        }

        // fx(t)を計算する関数
        private float _fx( float t )
        {
            // fx(t)
            //  = V0.Y*(1-t)^3 + 3*V1.Y*(1-t)^2*t + 3*V2.Y*(1-t)*t^2  + V3.Y*t^3
            //  = 0.0*(1-t)^3  + 3*V1.Y*(1-t)^2*t + 3*V2.y*(1-t)*t^2  + 1.0*t^3
            //  = 0            + 3*V1.Y*(1-t)^2*t + 3*V2.y*(1-t)*t^2  + t^3
            return
                0 +
                v1.X * 3 * ( 1 - t ) * ( 1 - t ) * t +
                v2.X * 3 * ( 1 - t ) * t * t +
                t * t * t;
        }

        // fx'(t) = dfx(t)/dt を計算する関数
        private float _dfx( float t )
        {
            // fx'(t)
            //  = d/dt{fx(t)}
            //  = d/dt{3*V1.X*(1-t)^2*t + 3*V2.X*(1-t)*t^2 + t^3}
            //  = d/dt{V1.X*(3*t^3-6*t^2+3*t) + V2.X*(3*t^2-3*t^3) + t^3}
            //  = V1.X*(9*t^2-12*t+3) + V2.X*(6*t-9*t^2) + 3*t^2
            return
                v1.X * ( 9 * t * t - 12 * t + 3 ) +
                v2.X * ( 6 * t - 9 * t * t ) +
                3 * t * t;
        }
    }
}
