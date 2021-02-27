using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    static class CGHelper
    {
        public static readonly Vector3 MaximumEulerAngles = new Vector3( MathUtil.Pi - float.Epsilon, 0.5f * MathUtil.Pi - float.Epsilon, MathUtil.Pi - float.Epsilon );

        public static Vector3 MinimumEulerAngles => -( MaximumEulerAngles );

        public static bool Between( this float Value, float MinimumValue, float MaximumValue ) => ( MinimumValue <= Value ) && ( MaximumValue >= Value );

        public static Vector3 NormalizeTheRangeOfEulerAngles( this Vector3 source )
        {
            // Y: -π/2 ～ π/2 に収める（ Y は Arcsin の引数とするため、この範囲になる）
            if( !source.Y.Between( (float) -Math.PI * 0.5f, (float) Math.PI * 0.5f ) )
            {
                if( source.Y > 0 )
                {
                    source.Y -= (float) Math.PI * 2;
                }
                else
                {
                    source.Y += (float) Math.PI * 2;
                }
            }

            // X: -π ～ π に収める
            if( !source.X.Between( (float) -Math.PI, (float) Math.PI ) )
            {
                if( source.X > 0 )
                {
                    source.X -= (float) Math.PI * 2;
                }
                else
                {
                    source.X += (float) Math.PI * 2;
                }
            }

            // Z: -π ～ π に収める
            if( !source.Z.Between( (float) -Math.PI, (float) Math.PI ) )
            {
                if( source.Z > 0 )
                {
                    source.Z -= (float) Math.PI * 2;
                }
                else
                {
                    source.Z += (float) Math.PI * 2;
                }
            }

            return source;
        }

        public static Matrix ToSharpDX( this BulletSharp.Math.Matrix value )
        {
            return new Matrix(
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44 );
        }

        public static BulletSharp.Math.Matrix ToBulletSharp( this Matrix value )
        {
            return new BulletSharp.Math.Matrix(
                value.M11, value.M12, value.M13, value.M14,
                value.M21, value.M22, value.M23, value.M24,
                value.M31, value.M32, value.M33, value.M34,
                value.M41, value.M42, value.M43, value.M44 );
        }

        public static BulletSharp.Math.Vector3 ToBulletSharp( this Vector3 value )
        {
            return new BulletSharp.Math.Vector3( value.X, value.Y, value.Z );
        }


        /// <summary>
        ///		Quaternion、Yaw(XAxleRotation), Pitch(YAxleRotation), Roll(ZAxleRotation)に分解する。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="ZAxleRotation">ZAxleRotation</param>
        /// <param name="XAxleRotation">XAxleRotation(-PI/2～PI/2)</param>
        /// <param name="YAxleRotation">YAxleRotation</param>
        /// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
        public static bool QuaternionZXYDisassembleIntoRotation( in Quaternion input, out float ZAxleRotation, out float XAxleRotation, out float YAxleRotation )
        {
            // クォータニオンを正規化する。
            var NormalizedInput = new Quaternion( input.X, input.Y, input.Z, input.W );
            NormalizedInput.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix RotationMatrix;
            Matrix.RotationQuaternion( ref NormalizedInput, out RotationMatrix );

            // X軸回転を取得する。
            // M32 は -Sin(XAxisRotationAngle) であり、M32 が +1 または -1 に非常に近いと、X軸回転角は≒±90度、すなわちZ軸とのジンバルロックが発生していると判定する。
            if( ( RotationMatrix.M32 > ( 1 - 1.0e-4 ) ) || ( RotationMatrix.M32 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                XAxleRotation = (float) ( RotationMatrix.M32 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                YAxleRotation = (float) Math.Atan2( -RotationMatrix.M13, RotationMatrix.M11 );
                ZAxleRotation = 0;
                return false;
            }
            // M32 = -Sin(XAxisRotationAngle) であることから
            XAxleRotation = -(float) Math.Asin( RotationMatrix.M32 );

            // Z軸回転を取得する。
            ZAxleRotation = (float) Math.Asin( RotationMatrix.M12 / Math.Cos( XAxleRotation ) );
            if( float.IsNaN( ZAxleRotation ) )   // 計算失敗？
            {
                // 漏れ対策
                XAxleRotation = (float) ( RotationMatrix.M32 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                YAxleRotation = (float) Math.Atan2( -RotationMatrix.M13, RotationMatrix.M11 );
                ZAxleRotation = 0;
                return false;
            }
            if( RotationMatrix.M22 < 0 )
            {
                ZAxleRotation = (float) ( Math.PI - ZAxleRotation );
            }

            // Pitch（YAxleRotation）を取得する。
            YAxleRotation = (float) Math.Atan2( RotationMatrix.M31, RotationMatrix.M33 );
            return true;
        }

        /// <summary>
        ///		Quaternion、X,Y,ZDisassembleIntoRotation。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="XAxleRotation">XAxleRotation</param>
        /// <param name="YAxleRotation">YAxleRotation(-PI/2～PI/2)</param>
        /// <param name="ZAxleRotation">ZAxleRotation</param>
        /// <returns></returns>
        public static bool QuaternionXYZDisassembleIntoRotation( in Quaternion input, out float XAxleRotation, out float YAxleRotation, out float ZAxleRotation )
        {
            // クォータニオンを正規化する。
            var NormalizedInput = new Quaternion( input.X, input.Y, input.Z, input.W );
            NormalizedInput.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix RotationMatrix;
            Matrix.RotationQuaternion( ref NormalizedInput, out RotationMatrix );

            // Y軸回りの回転を取得する。
            if( ( RotationMatrix.M13 > ( 1 - 1.0e-4 ) ) || ( RotationMatrix.M13 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                XAxleRotation = 0;
                YAxleRotation = (float) ( RotationMatrix.M13 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                ZAxleRotation = -(float) Math.Atan2( -RotationMatrix.M21, RotationMatrix.M22 );
                return false;
            }
            YAxleRotation = -(float) Math.Asin( RotationMatrix.M13 );

            // X軸回りの回転を取得する。
            XAxleRotation = (float) Math.Asin( RotationMatrix.M23 / Math.Cos( YAxleRotation ) );
            if( float.IsNaN( XAxleRotation ) )
            {
                //ジンバルロック判定(漏れ対策)
                XAxleRotation = 0;
                YAxleRotation = (float) ( RotationMatrix.M13 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                ZAxleRotation = -(float) Math.Atan2( -RotationMatrix.M21, RotationMatrix.M22 );
                return false;
            }
            if( RotationMatrix.M33 < 0 )
            {
                XAxleRotation = (float) ( Math.PI - XAxleRotation );
            }

            // Z軸回りの回転を取得する。
            ZAxleRotation = (float) Math.Atan2( RotationMatrix.M12, RotationMatrix.M11 );
            return true;
        }

        /// <summary>
        ///		QuaternionY,Z,XDisassembleIntoRotation。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="YAxleRotation">YAxleRotation</param>
        /// <param name="ZAxleRotation">ZAxleRotation(-PI/2～PI/2)</param>
        /// <param name="XAxleRotation">XAxleRotation</param>
        /// <returns></returns>
        public static bool QuaternionYZXDisassembleIntoRotation( in Quaternion input, out float YAxleRotation, out float ZAxleRotation, out float XAxleRotation )
        {
            // クォータニオンを正規化する。
            var NormalizedDormitory = new Quaternion( input.X, input.Y, input.Z, input.W );
            NormalizedDormitory.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix RotationMatrix;
            Matrix.RotationQuaternion( ref NormalizedDormitory, out RotationMatrix );

            // Z軸回りの回転を取得する。
            if( ( RotationMatrix.M21 > ( 1 - 1.0e-4 ) ) || ( RotationMatrix.M21 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                YAxleRotation = 0;
                ZAxleRotation = (float) ( RotationMatrix.M21 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                XAxleRotation = -(float) Math.Atan2( -RotationMatrix.M32, RotationMatrix.M33 );
                return false;
            }

            ZAxleRotation = -(float) Math.Asin( RotationMatrix.M21 );

            // Y軸回りの回転を取得する。
            YAxleRotation = (float) Math.Asin( RotationMatrix.M31 / Math.Cos( ZAxleRotation ) );
            if( float.IsNaN( YAxleRotation ) )
            {
                //ジンバルロック判定(漏れ対策)
                YAxleRotation = 0;
                ZAxleRotation = (float) ( RotationMatrix.M21 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                XAxleRotation = -(float) Math.Atan2( -RotationMatrix.M32, RotationMatrix.M33 );
                return false;
            }
            if( RotationMatrix.M11 < 0 )
            {
                YAxleRotation = (float) ( Math.PI - YAxleRotation );
            }

            // X軸回りの回転を取得する。
            XAxleRotation = (float) Math.Atan2( RotationMatrix.M23, RotationMatrix.M22 );
            return true;
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
