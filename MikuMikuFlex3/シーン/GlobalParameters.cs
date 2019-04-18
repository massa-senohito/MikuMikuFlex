using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     cbuffer GlobalParameters に対応する構造体。
    /// </summary>
    /// <remarks>
    ///     cbuffer については、GlobalParameters.hlsli を参照。
    ///     cbuffer では、各メンバが 16byte 境界に合うように配置されるので、
    ///     こちらの構造体でも、<see cref="FieldOffsetAttribute"/> を使って 16byte 境界になるようオフセットを指定する。
    /// </remarks>
    /// <seealso cref="https://docs.microsoft.com/en-us/windows/desktop/direct3dhlsl/dx-graphics-hlsl-packing-rules"/>
    [StructLayout(LayoutKind.Explicit)]
    public struct GlobalParameters
    {
        /// <summary>
        ///     描画中の材質がスフィアマップを使用するなら true。
        ///     材質単位。
        ///     true の場合、SphereTexture オブジェクトが有効であること。
        /// </summary>
        [FieldOffset( 0 )]
        [MarshalAs( UnmanagedType.Bool)]    // HLSLのboolは4byte, UnmanagedのBOOLも4byte
        public bool UseSphereMap;

        /// <summary>
        ///     スフィアマップの種類。
        ///     true なら加算スフィア、false なら乗算スフィア。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 4 )]
        [MarshalAs( UnmanagedType.Bool )]   // HLSLのboolは4byte, UnmanagedのBOOLも4byte
        public bool IsAddSphere;

        /// <summary>
        ///     描画中の材質がテクスチャを使用するなら true。
        ///     材質単位。
        ///     true の場合、Texture オブジェクトが有効であること。
        /// </summary>
        [FieldOffset( 8 )]
        [MarshalAs( UnmanagedType.Bool )]   // HLSLのboolは4byte, UnmanagedのBOOLも4byte
        public bool UseTexture;

        /// <summary>
        ///     描画中の材質がトゥーンテクスチャを使用するなら true。
        ///     材質単位。
        ///     true の場合、ToonTexture オブジェクトが有効であること。
        /// </summary>
        [FieldOffset( 12 )]
        [MarshalAs( UnmanagedType.Bool )]   // HLSLのboolは4byte, UnmanagedのBOOLも4byte
        public bool UseToonTextureMap;

        /// <summary>
        ///     描画中の材質がセルフ影を使用するなら true。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 16 )]
        [MarshalAs( UnmanagedType.Bool )]   // HLSLのboolは4byte, UnmanagedのBOOLも4byte
        public bool UseSelfShadow;

        /// <summary>
        ///     ワールド変換行列。
        ///     モデル単位。
        /// </summary>
        [FieldOffset( 32 )]
        public Matrix WorldMatrix;              // 64 bytes

        /// <summary>
        ///     ビュー変換行列。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 96 )]
        public Matrix ViewMatrix;               // 64 bytes

        /// <summary>
        ///     射影変換行列。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 160 )]
        public Matrix ProjectionMatrix;         // 64 bytes

        /// <summary>
        ///     カメラの位置。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 224 )]
        public Vector4 CameraPosition;          // 16 bytes

        /// <summary>
        ///     カメラの注視点。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 240 )]
        public Vector4 CameraTargetPosition;    // 16 bytes

        /// <summary>
        ///     カメラの上方向を示すベクトル。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 256 )]
        public Vector4 CameraUp;                // 16 bytes

        /// <summary>
        ///     照明１の色。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 272 )]
        public Vector4 Light1Color;             // 16bytes

        /// <summary>
        ///     照明１の方向。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 288 )]
        public Vector4 Light1Direction;         // 16 bytes

        /// <summary>
        ///     照明２の色。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 304 )]
        public Vector4 Light2Color;             // 16 bytes

        /// <summary>
        ///     照明２の方向。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 320 )]
        public Vector4 Light2Direction;         // 16 bytes

        /// <summary>
        ///     照明３の色。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 336 )]
        public Vector4 Light3Color;             // 16 bytes

        /// <summary>
        ///     照明３の方向。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 352 )]
        public Vector4 Light3Direction;         // 16 bytes

        /// <summary>
        ///     環境光。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 368 )]
        public Vector4 AmbientColor;            // 16 bytes

        /// <summary>
        ///     拡散色。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 384 )]
        public Vector4 DiffuseColor;            // 16 bytes

        /// <summary>
        ///     反射色。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 400 )]
        public Vector4 SpecularColor;           // 16 bytes

        /// <summary>
        ///     エッジの色。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 416 )]
        public Vector4 EdgeColor;               // 16 bytes

        /// <summary>
        ///     反射係数。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 432 )]
        public float SpecularPower;             // 4 bytes

        /// <summary>
        ///     エッジの幅。
        ///     材質単位。
        /// </summary>
        [FieldOffset( 436 )]
        public float EdgeWidth;                 // 4 bytes

        /// <summary>
        ///     テッセレーション係数。
        ///     モデル単位。
        /// </summary>
        [FieldOffset( 440 )]
        public float TessellationFactor;        // 4 bytes

        //[FieldOffset( 444 )]
        //public float dummy;                   // 4 bytes

        /// <summary>
        ///     ビューポートサイズ[px]。
        ///     シーン単位。
        /// </summary>
        [FieldOffset( 448 )]
        public Vector2 ViewportSize;            // 8 bytes

        //[FieldOffset( 456 )]
        //public Vector2 dummy;                 // 8 bytes


        public static int SizeInBytes => 464;
    }
}
