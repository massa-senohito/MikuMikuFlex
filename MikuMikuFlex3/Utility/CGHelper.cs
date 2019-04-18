using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    static class CGHelper
    {
        public static readonly Vector3 オイラー角の最大値 = new Vector3( MathUtil.Pi - float.Epsilon, 0.5f * MathUtil.Pi - float.Epsilon, MathUtil.Pi - float.Epsilon );

        public static Vector3 オイラー角の最小値 => -( オイラー角の最大値 );

        public static bool 間にある( this float 値, float 最小値, float 最大値 ) => ( 最小値 <= 値 ) && ( 最大値 >= 値 );

        public static Vector3 オイラー角の値域を正規化する( this Vector3 source )
        {
            // Y: -π/2 ～ π/2 に収める（ Y は Arcsin の引数とするため、この範囲になる）
            if( !source.Y.間にある( (float) -Math.PI * 0.5f, (float) Math.PI * 0.5f ) )
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
            if( !source.X.間にある( (float) -Math.PI, (float) Math.PI ) )
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
            if( !source.Z.間にある( (float) -Math.PI, (float) Math.PI ) )
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
        ///		クォータニオンを、Yaw(X軸回転), Pitch(Y軸回転), Roll(Z軸回転)に分解する。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="Z軸回転">Z軸回転</param>
        /// <param name="X軸回転">X軸回転(-PI/2～PI/2)</param>
        /// <param name="Y軸回転">Y軸回転</param>
        /// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
        public static bool クォータニオンをZXY回転に分解する( in Quaternion input, out float Z軸回転, out float X軸回転, out float Y軸回転 )
        {
            // クォータニオンを正規化する。
            var 正規化済み入力 = new Quaternion( input.X, input.Y, input.Z, input.W );
            正規化済み入力.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix 回転行列;
            Matrix.RotationQuaternion( ref 正規化済み入力, out 回転行列 );

            // X軸回転を取得する。
            // M32 は -Sin(X軸回転角) であり、M32 が +1 または -1 に非常に近いと、X軸回転角は≒±90度、すなわちZ軸とのジンバルロックが発生していると判定する。
            if( ( 回転行列.M32 > ( 1 - 1.0e-4 ) ) || ( 回転行列.M32 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                X軸回転 = (float) ( 回転行列.M32 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                Y軸回転 = (float) Math.Atan2( -回転行列.M13, 回転行列.M11 );
                Z軸回転 = 0;
                return false;
            }
            // M32 = -Sin(X軸回転角) であることから
            X軸回転 = -(float) Math.Asin( 回転行列.M32 );

            // Z軸回転を取得する。
            Z軸回転 = (float) Math.Asin( 回転行列.M12 / Math.Cos( X軸回転 ) );
            if( float.IsNaN( Z軸回転 ) )   // 計算失敗？
            {
                // 漏れ対策
                X軸回転 = (float) ( 回転行列.M32 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                Y軸回転 = (float) Math.Atan2( -回転行列.M13, 回転行列.M11 );
                Z軸回転 = 0;
                return false;
            }
            if( 回転行列.M22 < 0 )
            {
                Z軸回転 = (float) ( Math.PI - Z軸回転 );
            }

            // Pitch（Y軸回転）を取得する。
            Y軸回転 = (float) Math.Atan2( 回転行列.M31, 回転行列.M33 );
            return true;
        }

        /// <summary>
        ///		クォータニオンを、X,Y,Z回転に分解する。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="X軸回転">X軸回転</param>
        /// <param name="Y軸回転">Y軸回転(-PI/2～PI/2)</param>
        /// <param name="Z軸回転">Z軸回転</param>
        /// <returns></returns>
        public static bool クォータニオンをXYZ回転に分解する( in Quaternion input, out float X軸回転, out float Y軸回転, out float Z軸回転 )
        {
            // クォータニオンを正規化する。
            var 正規化済み入力 = new Quaternion( input.X, input.Y, input.Z, input.W );
            正規化済み入力.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix 回転行列;
            Matrix.RotationQuaternion( ref 正規化済み入力, out 回転行列 );

            // Y軸回りの回転を取得する。
            if( ( 回転行列.M13 > ( 1 - 1.0e-4 ) ) || ( 回転行列.M13 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                X軸回転 = 0;
                Y軸回転 = (float) ( 回転行列.M13 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                Z軸回転 = -(float) Math.Atan2( -回転行列.M21, 回転行列.M22 );
                return false;
            }
            Y軸回転 = -(float) Math.Asin( 回転行列.M13 );

            // X軸回りの回転を取得する。
            X軸回転 = (float) Math.Asin( 回転行列.M23 / Math.Cos( Y軸回転 ) );
            if( float.IsNaN( X軸回転 ) )
            {
                //ジンバルロック判定(漏れ対策)
                X軸回転 = 0;
                Y軸回転 = (float) ( 回転行列.M13 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                Z軸回転 = -(float) Math.Atan2( -回転行列.M21, 回転行列.M22 );
                return false;
            }
            if( 回転行列.M33 < 0 )
            {
                X軸回転 = (float) ( Math.PI - X軸回転 );
            }

            // Z軸回りの回転を取得する。
            Z軸回転 = (float) Math.Atan2( 回転行列.M12, 回転行列.M11 );
            return true;
        }

        /// <summary>
        ///		クォータニオンをY,Z,X回転に分解する。
        /// </summary>
        /// <param name="input">分解するクォータニオン</param>
        /// <param name="Y軸回転">Y軸回転</param>
        /// <param name="Z軸回転">Z軸回転(-PI/2～PI/2)</param>
        /// <param name="X軸回転">X軸回転</param>
        /// <returns></returns>
        public static bool クォータニオンをYZX回転に分解する( in Quaternion input, out float Y軸回転, out float Z軸回転, out float X軸回転 )
        {
            // クォータニオンを正規化する。
            var 正規化済み入寮 = new Quaternion( input.X, input.Y, input.Z, input.W );
            正規化済み入寮.Normalize();

            // 正規化されたクォータニオンから回転行列を生成する。
            Matrix 回転行列;
            Matrix.RotationQuaternion( ref 正規化済み入寮, out 回転行列 );

            // Z軸回りの回転を取得する。
            if( ( 回転行列.M21 > ( 1 - 1.0e-4 ) ) || ( 回転行列.M21 < ( -1 + 1.0e-4 ) ) )
            {
                // ジンバルロックしてる
                Y軸回転 = 0;
                Z軸回転 = (float) ( 回転行列.M21 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                X軸回転 = -(float) Math.Atan2( -回転行列.M32, 回転行列.M33 );
                return false;
            }

            Z軸回転 = -(float) Math.Asin( 回転行列.M21 );

            // Y軸回りの回転を取得する。
            Y軸回転 = (float) Math.Asin( 回転行列.M31 / Math.Cos( Z軸回転 ) );
            if( float.IsNaN( Y軸回転 ) )
            {
                //ジンバルロック判定(漏れ対策)
                Y軸回転 = 0;
                Z軸回転 = (float) ( 回転行列.M21 < 0 ? Math.PI / 2 : -Math.PI / 2 );
                X軸回転 = -(float) Math.Atan2( -回転行列.M32, 回転行列.M33 );
                return false;
            }
            if( 回転行列.M11 < 0 )
            {
                Y軸回転 = (float) ( Math.PI - Y軸回転 );
            }

            // X軸回りの回転を取得する。
            X軸回転 = (float) Math.Atan2( 回転行列.M23, 回転行列.M22 );
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
