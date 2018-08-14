using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.モデル.Shape
{
	public struct シェイプ用入力エレメント
	{
		public static readonly InputElement[] VertexElements =
		{
			new InputElement
			{
				SemanticName="POSITION",
				Format = Format.R32G32B32A32_Float
			}
		};

		public Vector4 Position;

		public static int SizeInBytes => Marshal.SizeOf( typeof( シェイプ用入力エレメント ) );
	}
}
