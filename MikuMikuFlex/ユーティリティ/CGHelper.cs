using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MMDFileParser.MotionParser;
using MikuMikuFlex.エフェクト;
using MikuMikuFlex.モデル;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace MikuMikuFlex
{
	public static class CGHelper
	{
		public const float PI = (float) Math.PI;

		public const string Toonリソースフォルダ = @"Resource\Toon\";


		public static readonly Vector3 オイラー角の最大値 = new Vector3( 
			PI - float.Epsilon, 
			0.5f * PI - float.Epsilon, 
			PI - float.Epsilon );

        public static Vector3 オイラー角の最小値
            => -( オイラー角の最大値 );

        public static bool 間にある( this float 値, float 最小値, float 最大値 )
            => ( 最小値 <= 値 ) && ( 最大値 >= 値 );

		public static Vector3 オイラー角の値域を正規化する( this Vector3 source )
		{
            // Y: -π/2 ～ π/2 に収める（ Y は Arcsin の引数とするため、この範囲になる）
            if( !source.Y.間にある( (float) -Math.PI * 0.5f, (float) Math.PI * 0.5f ) ) // X, Z もいじるので、X, Z よりも先に判定。
            {
                source.X -= (float) Math.PI;
                source.Y = (float) Math.PI - source.Y;
                source.Z -= (float) Math.PI;
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

            // Y: -π/2 ～ π/2 に収める（ Y は Arcsin の引数とするため、この範囲になる）
            //if( !source.Y.間にある( (float) -Math.PI * 0.5f, (float) Math.PI * 0.5f ) )
            //{
			//	if( source.Y > 0 )
			//	{
			//		source.Y -= (float) Math.PI * 2;
			//	}
			//	else
			//	{
			//		source.Y += (float) Math.PI * 2;
			//	}
			//}

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


		public static Buffer D3Dバッファを作成する( IEnumerable<byte> dataList, Device device, BindFlags flag )
		{
			using( var dataStream = DataStream.Create( dataList.ToArray(), true, true ) )
			{
				return new Buffer(
					device, 
					dataStream, 
					new BufferDescription {
						BindFlags = flag,
						SizeInBytes = (int) dataStream.Length
					} );
			}
		}

        public static Buffer D3Dバッファを作成する<T>( IEnumerable<T> dataList, Device device, BindFlags flag ) where T : struct
		{
			using( var dataStream = DataStream.Create( dataList.ToArray(), true, true ) )
			{
				return new Buffer(
					device,
					dataStream, 
					new BufferDescription {
						BindFlags = flag,
						SizeInBytes = (int) dataStream.Length
					} );
			}
		}

        public static Buffer D3Dバッファを作成する( Device device, int size, BindFlags flag )
		{
			return new Buffer(
				device,
				new BufferDescription {
					BindFlags = flag,
					SizeInBytes = size
				} );
		}


        public static Effect EffectFx5を作成する( string filePath, Device device )
		{
			using( var fs = File.OpenRead( filePath ) )
			using( ShaderBytecode byteCode = MMELoader.Instance.指定したファイル名からシェーダーバイトコードを取得して返す( filePath, fs ) )
			{
				return new Effect( device, byteCode );
			}
		}

        internal static Effect EffectFx5を作成するFromResource( string resourcePath, Device device )
		{
            using( Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream( resourcePath ) )
			using( ShaderBytecode byteCode = MMELoader.Instance.指定したファイル名からシェーダーバイトコードを取得して返す( resourcePath, fs ) )
			{
				return new Effect( device, byteCode );
			}
		}


		public static Vector3 ToVector3( this Matrix matrix )
            => new Vector3( matrix.M41, matrix.M42, matrix.M43 );

		public static Quaternion ToSuaternion( Vector3 rotation )
            => Quaternion.RotationYawPitchRoll( rotation.Y, rotation.X, rotation.Z );


		/// <summary>
		///		クォータニオンを、Yaw(X軸回転), Pitch(Y軸回転), Roll(Z軸回転)に分解する。
		/// </summary>
		/// <param name="input">分解するクォータニオン</param>
		/// <param name="Z軸回転">Z軸回転</param>
		/// <param name="X軸回転">X軸回転(-PI/2～PI/2)</param>
		/// <param name="Y軸回転">Y軸回転</param>
		/// <returns>ジンバルロックが発生した時はfalse。ジンバルロックはX軸回転で発生</returns>
		public static bool クォータニオンをZXY回転に分解する( Quaternion input, out float Z軸回転, out float X軸回転, out float Y軸回転 )
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
		public static bool クォータニオンをXYZ回転に分解する( Quaternion input, out float X軸回転, out float Y軸回転, out float Z軸回転 )
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
		public static bool クォータニオンをYZX回転に分解する( Quaternion input, out float Y軸回転, out float Z軸回転, out float X軸回転 )
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


        public static Vector3 MulEachMember( Vector3 vec1, Vector3 vec2 )
            => new Vector3( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z );

        public static Vector4 MulEachMember( Vector4 vec1, Vector4 vec2 )
            => new Vector4( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z, vec1.W * vec2.W );

		public static Vector3 ToRadians( Vector3 degree )
            => new Vector3( ToRadians( degree.X ), ToRadians( degree.Y ), ToRadians( degree.Z ) );

		public static float ToRadians( float degree )
            => ( degree * PI / 180f );

		public static float ToDegree( float radians )
            => ( 180f * radians / PI );

		public static Vector3 ToDegree( Vector3 radian ) 
            => new Vector3( ToDegree( radian.X ), ToDegree( radian.Y ), ToDegree( radian.Z ) );

		public static float 値を最大値と最小値の範囲に収める( float 値, float 最小値, float 最大値 )
		{
			if( 最小値 > 値 )
				return 最小値;

			if( 最大値 < 値 )
				return 最大値;

			return 値;
		}

		// バイト列変換し、代入する系。

		public static void LoadIndex( int surface, List<byte> InputIndexBuffer )
		{
			var vertex = (UInt16) surface;

			byte[] v = BitConverter.GetBytes( vertex );

			for( int i = 0; i < v.Length; i++ )
				InputIndexBuffer.Add( v[ i ] );
		}

		public static void AddListBuffer( float value, List<byte> InputBuffer )
		{
			byte[] v = BitConverter.GetBytes( value );

			for( int i = 0; i < v.Length; i++ )
				InputBuffer.Add( v[ i ] );
		}
		public static void AddListBuffer( int value, List<byte> InputBuffer ) => AddListBuffer( (UInt16) value, InputBuffer );
		public static void AddListBuffer( UInt16 value, List<byte> InputBuffer )
		{
			byte[] v = BitConverter.GetBytes( value );

			for( int i = 0; i < v.Length; i++ )
				InputBuffer.Add( v[ i ] );
		}
		public static void AddListBuffer( Vector2 value, List<byte> InputBuffer )
		{
			AddListBuffer( value.X, InputBuffer );
			AddListBuffer( value.Y, InputBuffer );
		}
		public static void AddListBuffer( Vector3 value, List<byte> InputBuffer )
		{
			AddListBuffer( value.X, InputBuffer );
			AddListBuffer( value.Y, InputBuffer );
			AddListBuffer( value.Z, InputBuffer );
		}
		public static void AddListBuffer( Vector4 value, List<byte> InputBuffer )
		{
			AddListBuffer( value.X, InputBuffer );
			AddListBuffer( value.Y, InputBuffer );
			AddListBuffer( value.Z, InputBuffer );
			AddListBuffer( value.W, InputBuffer );
		}

		// 補完系。

		/// <summary>
		///     与えた２つのフレームから、指定フレームでの回転行列を計算する。
		/// </summary>
		/// <param name="frame1">若いキーフレーム番号</param>
		/// <param name="frame2"></param>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static Quaternion ComplementRotateQuaternion( ボーンフレーム frame1, ボーンフレーム frame2, float progress )
            => Quaternion.Slerp( frame1.ボーンの回転, frame2.ボーンの回転, progress );

        public static Quaternion ComplementRotateQuaternion( カメラフレーム frame1, カメラフレーム frame2, float progress )
		{
			var q1 = Quaternion.RotationYawPitchRoll( frame1.回転.Y, frame1.回転.X, frame1.回転.Z );
			var q2 = Quaternion.RotationYawPitchRoll( frame2.回転.Y, frame2.回転.X, frame2.回転.Z );
			return Quaternion.Slerp( q1, q2, progress );
		}


		/// <summary>
		///     与えた2つのフレームから指定フレームでの補完位置を計算します。
		/// </summary>
		/// <param name="frame1"></param>
		/// <param name="frame2"></param>
		/// <param name="frame"></param>
		/// <returns></returns>
		public static Vector3 ComplementTranslate( ボーンフレーム frame1, ボーンフレーム frame2, Vector3 progress )
		{
			return new Vector3(
				Lerp( frame1.ボーンの位置.X, frame2.ボーンの位置.X, progress.X ),
				Lerp( frame1.ボーンの位置.Y, frame2.ボーンの位置.Y, progress.Y ),
				Lerp( frame1.ボーンの位置.Z, frame2.ボーンの位置.Z, progress.Z ) );
		}

        public static Vector3 ComplementTranslate( カメラフレーム frame1, カメラフレーム frame2, Vector3 progress )
		{
			return new Vector3(
				Lerp( frame1.位置.X, frame2.位置.X, progress.X ),
				Lerp( frame1.位置.Y, frame2.位置.Y, progress.Y ),
				Lerp( frame1.位置.Z, frame2.位置.Z, progress.Z ) );
		}


		public static float Lerp( float start, float end, float factor )
            => ( start + ( ( end - start ) * factor ) );
	}
}