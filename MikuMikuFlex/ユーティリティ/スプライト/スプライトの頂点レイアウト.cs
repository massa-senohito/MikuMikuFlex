using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MikuMikuFlex.スプライト
{
    public struct スプライトの頂点レイアウト
    {
        public Vector3 Position;

        public Vector2 UV;

        public static InputElement[] InputElements = new[] {
            new InputElement() {
                SemanticName = "POSITION",
                Format = Format.R32G32B32_Float
            },
            new InputElement() {
                SemanticName = "UV",
                Format = Format.R32G32_Float,
                AlignedByteOffset = InputElement.AppendAligned
            },
        };

        public static int SizeInBytes
            => Marshal.SizeOf( typeof( スプライトの頂点レイアウト ) );
    }
}
