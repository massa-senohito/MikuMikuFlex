using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MikuMikuFlex.Utility
{
	/// <summary>
	///		TGA ファイルから PNG ストリームを生成するヘルパメソッドを提供する。
	/// </summary>
	public static class TargaSolver
	{
		public static Stream LoadTargaImage( string filePath, ImageFormat rootFormat= null )
		{
			try
			{
                using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                using( var br = new BinaryReader( fs ) )
                {
                    Bitmap tgaFile = null;

                    if( rootFormat == null )
                        rootFormat = ImageFormat.Png;

                    try
                    {
                        tgaFile = Paloma.TargaImage.LoadTargaImage( filePath );
                    }
                    catch
                    {
                        //tga以外の形式のとき
                        return File.OpenRead( filePath );
                    }

                    MemoryStream ms = new MemoryStream();
                    tgaFile.Save( ms, rootFormat );
                    ms.Seek( 0, SeekOrigin.Begin );
                    return ms;
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
