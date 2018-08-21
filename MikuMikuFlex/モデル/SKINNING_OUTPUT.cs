using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.モデル
{
    /// <summary>
    ///     スキニング計算後のデータのフォーマット。
    /// </summary>
	public struct SKINNING_OUTPUT   // PS_INPUT と同義
	{
        #region 順番入れ替え危険

        public Vector4 Position;

        public Vector3 Normal;

        public Vector2 UV;

        public Vector4 AddUV1;

        public Vector4 AddUV2;

        public Vector4 AddUV3;

        public Vector4 AddUV4;

        public float EdgeWeight;

        public UInt32 Index;

        #endregion

        public static readonly InputElement[] VertexElements = {

			new InputElement
			{
				SemanticName = "POSITION",
				Format = Format.R32G32B32A32_Float
			},
			new InputElement
			{
				SemanticName = "NORMAL",
				Format = Format.R32G32B32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
			new InputElement    // UV
            {
				SemanticName = "TEXCOORD",
				Format = Format.R32G32_Float,
				SemanticIndex = 0,
				AlignedByteOffset = InputElement.AppendAligned
			},
			new InputElement    //AddUV1
            {
				SemanticName = "TEXCOORD",
				SemanticIndex = 1,
				Format = Format.R32G32B32A32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
            new InputElement    //AddUV2
            {
				SemanticName = "TEXCOORD",
				SemanticIndex = 2,
				Format = Format.R32G32B32A32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
            new InputElement    //AddUV3
            {
				SemanticName = "TEXCOORD",
				SemanticIndex = 3,
				Format = Format.R32G32B32A32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
			new InputElement    //AddUV4
            {
				SemanticName = "TEXCOORD",
				SemanticIndex = 4,
				Format = Format.R32G32B32A32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
			new InputElement    // EdgeWeight
			{
				SemanticName = "EDGEWEIGHT",
				Format = Format.R32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			},
			new InputElement    // Index
			{
				SemanticName = "PSIZE",
				SemanticIndex = 15,
				Format = Format.R32_UInt,
				AlignedByteOffset = InputElement.AppendAligned
			},
		};

		public static int SizeInBytes => Marshal.SizeOf( typeof( SKINNING_OUTPUT ) );
	}
}