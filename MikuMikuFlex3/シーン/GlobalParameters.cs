using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using SharpDX;

namespace MikuMikuFlex3
{
    [StructLayout(LayoutKind.Explicit)]
    public struct GlobalParameters
    {
        // 描画中の材質がスフィアマップを使用するなら true。材質単位。
        // 　true の場合、SphereTexture オブジェクトが有効であること。
        [FieldOffset( 0 )]
        [MarshalAs( UnmanagedType.Bool)]
        public bool UseSphereMap; // HLSLのboolは4byte

        // スフィアマップの種類。true なら加算スフィア、false なら乗算スフィア。材質単位。
        [FieldOffset( 4 )]
        [MarshalAs( UnmanagedType.Bool )]
        public bool IsAddSphere;

        // 描画中の材質がテクスチャを使用するなら true。材質単位。
        // 　true の場合、Texture オブジェクトが有効であること。
        [FieldOffset( 8 )]
        [MarshalAs( UnmanagedType.Bool )]
        public bool UseTexture;

        // 描画中の材質がトゥーンテクスチャを使用するなら true。材質単位。
        // 　true の場合、ToonTexture オブジェクトが有効であること。
        [FieldOffset( 12 )]
        [MarshalAs( UnmanagedType.Bool )]
        public bool UseToonTextureMap;

        // 描画中の材質がセルフ影を使用するなら true。材質単位。
        [FieldOffset( 16 )]
        [MarshalAs( UnmanagedType.Bool )]
        public bool UseSelfShadow;

        // ワールド変換行列。モデル単位。
        [FieldOffset( 32 )]
        public Matrix WorldMatrix;

        // ビュー変換行列。シーン単位。
        [FieldOffset( 96 )]
        public Matrix ViewMatrix;

        // 射影変換行列。シーン単位。
        [FieldOffset( 160 )]
        public Matrix ProjectionMatrix;

        // カメラの位置。シーン単位。
        [FieldOffset( 224 )]
        public Vector4 CameraPosition;

        // カメラの注視点。シーン単位。
        [FieldOffset( 240 )]
        public Vector4 CameraTargetPosition;

        // カメラの上方向を示すベクトル。シーン単位。
        [FieldOffset( 256 )]
        public Vector4 CameraUp;

        // 照明１の色。シーン単位。
        [FieldOffset( 272 )]
        public Vector4 Light1Color;

        // 照明１の方向。シーン単位。
        [FieldOffset( 288 )]
        public Vector4 Light1Direction;

        // 照明２の色。シーン単位。
        [FieldOffset( 304 )]
        public Vector4 Light2Color;

        // 照明２の方向。シーン単位。
        [FieldOffset( 320 )]
        public Vector4 Light2Direction;

        // 照明３の色。シーン単位。
        [FieldOffset( 336 )]
        public Vector4 Light3Color;

        // 照明３の方向。シーン単位。
        [FieldOffset( 352 )]
        public Vector4 Light3Direction;

        // 環境光。材質単位。
        [FieldOffset( 368 )]
        public Vector4 AmbientColor;

        // 拡散色。材質単位。
        [FieldOffset( 384 )]
        public Vector4 DiffuseColor;

        // 反射色。材質単位。
        [FieldOffset( 400 )]
        public Vector4 SpecularColor;

        // エッジの色。材質単位。
        [FieldOffset( 416 )]
        public Vector4 EdgeColor;

        // 反射係数。材質単位。
        [FieldOffset( 432 )]
        public float SpecularPower;

        // エッジの幅。材質単位。
        [FieldOffset( 436 )]
        public float EdgeWidth;

        // テッセレーション係数。モデル単位。
        [FieldOffset( 440 )]
        public float TessellationFactor;

        // ビューポートサイズ[px]。シーン単位。
        [FieldOffset( 448 )]
        public Vector2 ViewportSize;


        public static int SizeInBytes => 464;
    }
}
