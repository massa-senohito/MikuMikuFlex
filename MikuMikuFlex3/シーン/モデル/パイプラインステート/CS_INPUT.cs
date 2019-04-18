using System;
using System.Runtime.InteropServices;
using SharpDX;

namespace MikuMikuFlex3
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

        public static int SizeInBytes => Marshal.SizeOf( typeof( CS_INPUT ) );
    }
}
