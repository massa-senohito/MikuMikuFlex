using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MikuMikuFlex.グリッド
{
	/// <summary>
	///     軸に利用する入力レイアウト
	/// </summary>
	public struct AxisGridLayout
	{
		/// <summary>
		///     入力レイアウト
		/// </summary>
		public static readonly InputElement[] VertexElements =
		{
			new InputElement
			{
				SemanticName = "POSITION",
				Format = Format.R32G32B32_Float
			},
			new InputElement
			{
				SemanticName = "COLOR",
				Format = Format.R32G32B32A32_Float,
				AlignedByteOffset = InputElement.AppendAligned
			}
		};

		/// <summary>
		///     色
		/// </summary>
		public Vector4 Color;

		/// <summary>
		///     位置
		/// </summary>
		public Vector3 Position;

		/// <summary>
		///     1要素あたりのサイズ
		/// </summary>
		public static int SizeInBytes => Marshal.SizeOf( typeof( AxisGridLayout ) );
	}
}
