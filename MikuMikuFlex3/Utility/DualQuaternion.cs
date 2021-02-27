using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.Utility
{
    public struct DualQuaternion
    {
        public Quaternion Real;

        public Quaternion Dual;

        public Quaternion Rotation => this.Real;

        public Quaternion Move => this.Dual;

        public float Length
            => Quaternion.Dot( this.Real, this.Dual );


        public static DualQuaternion Default
            => new DualQuaternion( Quaternion.Identity, Quaternion.Zero );

        public static DualQuaternion Zero
            => new DualQuaternion();


        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="real">Real部、あるいは回転。</param>
        /// <param name="dual">Dual部、あるいは移動。</param>
        public DualQuaternion( Quaternion real, Quaternion dual )
        {
            this.Real = Quaternion.Normalize( real );
            this.Dual = dual;
        }

        public DualQuaternion( Matrix matrix )
        {
            matrix.ToScaleRotationTranslation( out Vector3 scale, out Quaternion rotation, out Vector3 translation );

            this.Real = rotation;

            this.Dual = new Quaternion {
                X = 0.5f * ( translation.X * rotation.W + translation.Y * rotation.Z - translation.Z * rotation.Y ),
                Y = 0.5f * ( -translation.X * rotation.Z + translation.Y * rotation.W + translation.Z * rotation.X ),
                Z = 0.5f * ( translation.X * rotation.Y - translation.Y * rotation.X + translation.Z * rotation.W ),
                W = -0.5f * ( translation.X * rotation.X + translation.Y * rotation.Y + translation.Z * rotation.Z ),
            };
        }

        public void Normalize()
        {
            var scale = 1f / this.Length;
            scale = ( Math.Abs( scale ) < MathUtil.ZeroTolerance ) ? 0 : scale;

            this.Real *= scale;
            this.Dual *= scale;
        }

        public Matrix ToMatrix()
        {
            this.Normalize();

            var r = new Vector4( this.Real.X, this.Real.Y, this.Real.Z, this.Real.W );
            var t = this.Dual * 2.0f * Quaternion.Conjugate( this.Real );

            return new Matrix {
                M11 = ( r.W * r.W ) + ( r.X * r.X ) - ( r.Y * r.Y ) - ( r.Z * r.Z ),
                M12 = ( 2f * r.X * r.Y ) + ( 2f * r.W * r.Z ),
                M13 = ( 2f * r.X * r.Z ) - ( 2f * r.W * r.Y ),
                M14 = 0f,
                M21 = ( 2f * r.X * r.Y ) - ( 2f * r.W * r.Z ),
                M22 = ( r.W * r.W ) + ( r.Y * r.Y ) - ( r.X * r.X ) - ( r.Z * r.Z ),
                M23 = ( 2f * r.Y * r.Z ) + ( 2f * r.W * r.X ),
                M24 = 0f,
                M31 = ( 2f * r.X * r.Z ) + ( 2f * r.W * r.Y ),
                M32 = ( 2f * r.Y * r.Z ) - ( 2f * r.W * r.X ),
                M33 = ( r.W * r.W ) + ( r.Z * r.Z ) - ( r.X * r.X ) - ( r.Y * r.Y ),
                M34 = 0f,
                M41 = t.X,
                M42 = t.Y,
                M43 = t.Z,
                M44 = 1f,
            };
        }


        public static DualQuaternion operator +( DualQuaternion left, DualQuaternion right )
            => new DualQuaternion( left.Real + right.Real, left.Dual + right.Dual );

        public static DualQuaternion operator *( DualQuaternion left, float right )
            => new DualQuaternion( left.Real * right, left.Dual * right );
    }
}
