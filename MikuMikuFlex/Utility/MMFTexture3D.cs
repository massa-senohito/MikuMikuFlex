using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.Utility
{
	/// <summary>
	///		Texture3D を生成するヘルパメソッドを提供する。
	/// </summary>
	/// <remarks>
	///		以前は D3DX ライブラリで同等の機能が提供されていたが、現在は D3DX は廃止されている。
	/// </remarks>
	static class MMFTexture3D
	{
		static public Texture3D FromStream( Device device, Stream stream )
		{
			var texture = (Texture3D) null;

			using( var image = new System.Drawing.Bitmap( stream ) )
			{
				var imageRect = new System.Drawing.Rectangle( 0, 0, image.Width, image.Height );
				using( var bitmap = image.Clone( imageRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) )
				{
					var locks = bitmap.LockBits( imageRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat );
					try
					{
						var dataBox = new[] { new DataBox( locks.Scan0, bitmap.Width * 4, bitmap.Height ) };
						var textureDesc = new Texture3DDescription() {
							BindFlags = BindFlags.ShaderResource,
							CpuAccessFlags = CpuAccessFlags.None,
							Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
							Height = bitmap.Height,
							Width = bitmap.Width,
							MipLevels = 1,
							OptionFlags = ResourceOptionFlags.None,
							Depth = 1,
							Usage = ResourceUsage.Default
						};
						texture = new Texture3D( device, textureDesc, dataBox );
					}
					finally
					{
						bitmap.UnlockBits( locks );
					}
				}
			}
			return texture;
		}
	}
}
