using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.モデル
{
    /// <summary>
    ///     コンピュートシェーダーの入力
    /// </summary>
	public struct CS_INPUT
    {
        public Vector4 Position;
        public float BoneWeight1;
        public float BoneWeight2;
        public float BoneWeight3;
        public float BoneWeight4;
        public UInt32 BoneIndex1;
        public UInt32 BoneIndex2;
        public UInt32 BoneIndex3;
        public UInt32 BoneIndex4;
        public Vector3 Normal;
        public Vector2 UV;
        public Vector4 AddUV1;
        public Vector4 AddUV2;
        public Vector4 AddUV3;
        public Vector4 AddUV4;
        public Vector4 Sdef_C;
        public Vector3 SdefR0;
        public Vector3 SdefR1;
        public float EdgeWeight;
        public UInt32 Index;
        /// <summary>
        ///     <see cref="MMDFileParser.PMXModelParser.BoneWeight.変形方式"/> の値
        /// </summary>
        public UInt32 変形方式;

        public static readonly InputElement[] VertexElements = {
			new InputElement { SemanticName = "POSITION", Format = Format.R32G32B32A32_Float },
            new InputElement { SemanticName = "BLENDWEIGHT",  Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "BLENDINDICES", Format = Format.R32G32B32A32_UInt, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "NORMAL", Format = Format.R32G32B32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 0, Format = Format.R32G32_Float, AlignedByteOffset = InputElement.AppendAligned },        // UV
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 1, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV1
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 2, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV2
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 3, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV3
            new InputElement { SemanticName = "TEXCOORD", SemanticIndex = 4, Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },  // AddUV4
            new InputElement { SemanticName = "SDEF_C",  Format = Format.R32G32B32A32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "SDEF_R0", Format = Format.R32G32B32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "SDEF_R1", Format = Format.R32G32B32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "EDGEWEIGHT", Format = Format.R32_Float, AlignedByteOffset = InputElement.AppendAligned },
            new InputElement { SemanticName = "PSIZE", SemanticIndex = 15, Format = Format.R32_UInt, AlignedByteOffset = InputElement.AppendAligned },  // Index
            new InputElement { SemanticName = "DEFORM", Format = Format.R32_UInt, AlignedByteOffset = InputElement.AppendAligned },  // 変形方式
        };

        public static int SizeInBytes => Marshal.SizeOf( typeof( CS_INPUT ) );
    }
}
