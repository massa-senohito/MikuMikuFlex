using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.モデル
{
	public struct MMM_SKINNING_INPUT
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

        public MMDFileParser.PMXModelParser.BoneWeight.変形方式 変形方式;
	}
}