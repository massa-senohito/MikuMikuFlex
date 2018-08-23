using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.Utility
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

        /// <summary>
        ///     Matrix を、拡大ベクトル・回転行列・平行移動ベクトルに分解する。
        /// </summary>
        /// <remarks>
        ///     参考: 「その39 知っていると便利？ワールド変換行列から情報を抜き出そう」
        ///     http://marupeke296.com/DXG_No39_WorldMatrixInformation.html
        /// </remarks>
        public static void ToScaleRotationTranslation( this SharpDX.Matrix matrix, out SharpDX.Vector3 scale, out SharpDX.Quaternion rotation, out SharpDX.Vector3 translation )
        {
            scale = new SharpDX.Vector3(
                new SharpDX.Vector3( matrix.M11, matrix.M12, matrix.M13 ).Length(),
                new SharpDX.Vector3( matrix.M21, matrix.M22, matrix.M23 ).Length(),
                new SharpDX.Vector3( matrix.M31, matrix.M32, matrix.M33 ).Length() );

            translation = new SharpDX.Vector3( matrix.M41, matrix.M42, matrix.M43 );

            // matrix を構成する scale と translation が定まれば、rotation も定まる。
            // ※参考ページ中段の「WorldMatrix = S・Rx・Ry・Rz・T = ...」の図を参照のこと。

            rotation = SharpDX.Quaternion.RotationMatrix(
                new SharpDX.Matrix {
                    Row1 = ( scale.X == 0f ) ? SharpDX.Vector4.Zero : new SharpDX.Vector4( matrix.M11 / scale.X, matrix.M12 / scale.X, matrix.M13 / scale.X, 0f ),
                    Row2 = ( scale.Y == 0f ) ? SharpDX.Vector4.Zero : new SharpDX.Vector4( matrix.M21 / scale.Y, matrix.M22 / scale.Y, matrix.M23 / scale.Y, 0f ),
                    Row3 = ( scale.Z == 0f ) ? SharpDX.Vector4.Zero : new SharpDX.Vector4( matrix.M31 / scale.Z, matrix.M32 / scale.Z, matrix.M33 / scale.Z, 0f ),
                    Row4 = new SharpDX.Vector4( 0f, 0f, 0f, 1f ),
                } );
        }
    }
}
