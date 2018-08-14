using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MMF.Utility
{
	/// <summary>
	///		TGA ファイルから PNG ストリームを生成するヘルパメソッドを提供する。
	/// </summary>
	public static class TargaSolver
	{
		public static Stream LoadTargaImage( string filePath )
		{
			try
			{
                using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                using( var br = new BinaryReader( fs ) )
                {
                    var tga = new TgaLib.TgaImage( br );
                    var bmp = tga.GetBitmap();

                    var pngStream = new MemoryStream();

                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add( BitmapFrame.Create( bmp ) );
                    pngEncoder.Save( pngStream );

                    pngStream.Seek( 0, SeekOrigin.Begin );

                    return pngStream;
                }
            }
			catch( Exception )
			{
				// tga以外の形式のとき
				return File.OpenRead( filePath );
			}
		}
	}
}
