using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex
{
	/// <summary>
	///		シェーダーリソースビューを作成するためのヘルパメソッドを提供する。
	/// </summary>
	public static class MMFShaderResourceView
	{
		static public ShaderResourceView FromStream( Device device, Stream stream, out Texture2D texture )
		{
			var srv = (SharpDX.Direct3D11.ShaderResourceView) null;

			using( var image = new System.Drawing.Bitmap( stream ) )
			{
				var imageRect = new System.Drawing.Rectangle( 0, 0, image.Width, image.Height );
                using( var bitmap = image.Clone( imageRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) )
                {
                    var locks = bitmap.LockBits( imageRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat );
                    var dataBox = new[] { new SharpDX.DataBox( locks.Scan0, bitmap.Width * 4, bitmap.Height ) };
                    var textureDesc = new Texture2DDescription() {
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Height = bitmap.Height,
                        Width = bitmap.Width,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                        Usage = ResourceUsage.Default
                    };

                    texture = new Texture2D( device, textureDesc, dataBox );
                    bitmap.UnlockBits( locks );
                    srv = new SharpDX.Direct3D11.ShaderResourceView( device, texture );
                }
			}
			return srv;
		}

		static public ShaderResourceView FromFile( Device device, string path, out Texture2D texture )
		{
			var srv = (SharpDX.Direct3D11.ShaderResourceView) null;

			using( var image = new System.Drawing.Bitmap( path ) )
			{
				var imageRect = new System.Drawing.Rectangle( 0, 0, image.Width, image.Height );
                using( var bitmap = image.Clone( imageRect, System.Drawing.Imaging.PixelFormat.Format32bppArgb ) )
                {
                    var locks = bitmap.LockBits( imageRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat );
                    var dataBox = new[] { new SharpDX.DataBox( locks.Scan0, bitmap.Width * 4, bitmap.Height ) };
                    var textureDesc = new Texture2DDescription() {
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        Height = bitmap.Height,
                        Width = bitmap.Width,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                        Usage = ResourceUsage.Default
                    };

                    texture = new Texture2D( device, textureDesc, dataBox );
                    bitmap.UnlockBits( locks );
                    srv = new SharpDX.Direct3D11.ShaderResourceView( device, texture );
                }
			}
			return srv;
		}
	}
}
