using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMF.Utility
{
	public static class 拡張メソッド
	{
		public static SharpDX.Matrix ToSharpDX( this BulletSharp.Math.Matrix value )
		{
			return new SharpDX.Matrix(
				value.M11, value.M12, value.M13, value.M14,
				value.M21, value.M22, value.M23, value.M24,
				value.M31, value.M32, value.M33, value.M34,
				value.M41, value.M42, value.M43, value.M44 );
		}

		public static BulletSharp.Math.Matrix ToBulletSharp( this SharpDX.Matrix value )
		{
			return new BulletSharp.Math.Matrix(
				value.M11, value.M12, value.M13, value.M14,
				value.M21, value.M22, value.M23, value.M24,
				value.M31, value.M32, value.M33, value.M34,
				value.M41, value.M42, value.M43, value.M44 );
		}

		public static BulletSharp.Math.Vector3 ToBulletSharp( this SharpDX.Vector3 value )
		{
			return new BulletSharp.Math.Vector3( value.X, value.Y, value.Z );
		}
	}
}
