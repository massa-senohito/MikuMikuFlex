using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MikuMikuFlex3
{
	/// <summary>
	///		TGA ファイルから PNG ストリームを生成するヘルパメソッドを提供する。
	/// </summary>
	internal static class TargaSolver
	{
        public static Stream LoadTargaImage( Stream imageStream, ImageFormat rootFormat = null )
        {
            try
            {
                using( var br = new BinaryReader( imageStream, System.Text.Encoding.Default, leaveOpen: true ) )   // leaveOpen=true; br を破棄しても imageStream は破棄しない
                {
                    // TgaLib.dll の場合

                    var tga = new TgaLib.TgaImage( br, false );
                    var bmp = tga.GetBitmap();

                    var ms = new MemoryStream();
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add( BitmapFrame.Create( bmp ) );

                    pngEncoder.Save( ms );
                    ms.Seek( 0, SeekOrigin.Begin );

                    imageStream.Close();
                    return ms;

                    // TargaImage.dll の場合
                    /*
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
                    */
                }
            }
            catch
            {
                // tga以外の形式のとき
                return imageStream;
            }
        }

        public static Stream LoadTargaImage( string filePath, ImageFormat rootFormat= null )
		{
            try
            {
                using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                using( var br = new BinaryReader( fs ) )
                {
                    // TgaLib.dll の場合

                    var tga = new TgaLib.TgaImage( br, false );
                    var bmp = tga.GetBitmap();

                    MemoryStream ms = new MemoryStream();
                    var pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add( BitmapFrame.Create( bmp ) );

                    pngEncoder.Save( ms );
                    ms.Seek( 0, SeekOrigin.Begin );

                    using( var fout = new FileStream( $"{ filePath }.png", FileMode.Create ) )
                    using( var bw = new BinaryWriter( fout ) )
                    {
                        bw.Write( ms.GetBuffer(), 0, (int) ms.Length );
                    }



                    return ms;

                    // TargaImage.dll の場合
                    /*
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
                    */
                }
            }
            catch
            {
                // tga以外の形式のとき
                return File.OpenRead( filePath );
            }
		}
	}
}
