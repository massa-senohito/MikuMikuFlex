using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.モデル.Other
{
	public struct DebugDotInputLayout
	{
		public Vector3 Position;

		public static InputElement[] InputElements = new[] {
			new InputElement() {
				SemanticName = "POSITION",
				Format = Format.R32G32B32_Float
			}
		};

		public static int SizeInBytes => Marshal.SizeOf( typeof( DebugDotInputLayout ) );
	}
}
