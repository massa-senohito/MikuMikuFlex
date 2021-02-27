using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     頂点シェーダーの入力
    /// </summary>
	public struct VS_INPUT
	{
        public Vector4 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Vector4 AddUV1;
        public Vector4 AddUV2;
        public Vector4 AddUV3;
        public Vector4 AddUV4;
        public float EdgeWeight;
        public UInt32 Index;

        public static readonly InputElement[] VertexElements = {
			new InputElement { SemanticName = "POSITION", Format = Format.R32G32B32A32_Float },
			new InputElement { SemanticName = "NORMAL",   Format = Format.R32G32B32_Float, AlignedByteOffset = InputElement.AppendAligned },
			new InputElement { SemanticName = "TEXCOORD", Format = Format.R32G32_Float,	SemanticIndex = 0, AlignedByteOffset = InputElement.AppendAligned },  // UV
			new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 1, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV1
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 2, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV2
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 3, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV3
			new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 4, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV4
			new InputElement { SemanticName = "EDGEWEIGHT", Format = Format.R32_Float, AlignedByteOffset = InputElement.AppendAligned },  // EdgeWeight
			new InputElement { SemanticName = "PSIZE", SemanticIndex = 15, Format = Format.R32_UInt, AlignedByteOffset = InputElement.AppendAligned },  // Index
		};

		public static int SizeInBytes => Marshal.SizeOf( typeof( VS_INPUT ) );
	}
}
