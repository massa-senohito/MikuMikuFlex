using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;

namespace MikuMikuFlex3
{
    internal class ParserHelper
    {
        public static string get_TextBuf( Stream fs, Encoding encoding )
        {
            // 文字数を取得。
            var temp = new byte[ 4 ];
            fs.Read( temp, 0, 4 );
            int WordCount = BitConverter.ToInt32( temp, 0 );

            // 文字列を取得。
            var String = new byte[ WordCount ];
            fs.Read( String, 0, WordCount );

            // 指定されたエンコーダで変換する。
            return encoding.GetString( String );
        }

        public static float get_Float( Stream fs )
        {
            var buffer = new byte[ 4 ];
            fs.Read( buffer, 0, 4 );

            return BitConverter.ToSingle( buffer, 0 );
        }

        public static Vector4 get_Float4( Stream fs )
        {
            var buffer = new byte[ 16 ];
            fs.Read( buffer, 0, 16 );

            return new Vector4(
                BitConverter.ToSingle( buffer, 0 ),
                BitConverter.ToSingle( buffer, 4 ),
                BitConverter.ToSingle( buffer, 8 ),
                BitConverter.ToSingle( buffer, 12 ) );
        }

        public static Vector3 get_Float3( Stream fs )
        {
            var buffer = new byte[ 12 ];
            fs.Read( buffer, 0, 12 );

            return new Vector3(
                BitConverter.ToSingle( buffer, 0 ),
                BitConverter.ToSingle( buffer, 4 ),
                BitConverter.ToSingle( buffer, 8 ) );
        }

        public static Vector2 get_Float2( Stream fs )
        {
            var buffer = new byte[ 8 ];
            fs.Read( buffer, 0, 8 );

            return new Vector2(
                BitConverter.ToSingle( buffer, 0 ),
                BitConverter.ToSingle( buffer, 4 ) );
        }

        public static int get_Int( Stream fs )
        {
            var buffer = new byte[ 4 ];
            fs.Read( buffer, 0, 4 );

            return BitConverter.ToInt32( buffer, 0 );
        }

        public static ushort get_UShort( Stream fs )
        {
            var buffer = new byte[ 2 ];
            fs.Read( buffer, 0, 2 );

            return BitConverter.ToUInt16( buffer, 0 );
        }

        public static byte get_Byte( Stream fs )
        {
            var buffer = new byte[ 1 ];
            fs.Read( buffer, 0, 1 );

            return buffer[ 0 ];
        }

        public static int get_Index( Stream fs, int size )
        {
            var buffer = new byte[ size ];
            fs.Read( buffer, 0, size );

            switch( size )
            {
                case 1:
                    return (sbyte) buffer[ 0 ];
                case 2:
                    return BitConverter.ToInt16( buffer, 0 );
                case 4:
                    return BitConverter.ToInt32( buffer, 0 );
                default:
                    throw new InvalidDataException();
            }
        }

        public static uint get_VertexIndex( Stream fs, int size )
        {
            var buffer = new byte[ size ];
            fs.Read( buffer, 0, size );

            switch( size )
            {
                case 1:
                    return buffer[ 0 ];
                case 2:
                    return BitConverter.ToUInt16( buffer, 0 );
                case 4:
                    return BitConverter.ToUInt32( buffer, 0 );
                default:
                    throw new InvalidDataException();
            }
        }

        public static bool isFlagEnabled( short chk, short flag )
        {
            return ( ( chk & flag ) == flag );
        }

        public static string get_Shift_JISString( Stream fs, int length )
        {
            var encoding = Encoding.GetEncoding( "Shift_JIS" );

            List<byte> textBuf = new List<byte>();

            for( int i = 0; i < length; i++ )
            {
                var t = new byte[ 1 ] { get_Byte( fs ) };

                if( encoding.GetString( t )[ 0 ] == '\0' )
                {
                    fs.Read( new byte[ length - ( i + 1 ) ], 0, length - ( i + 1 ) );
                    break;
                }
                else
                {
                    textBuf.Add( t[ 0 ] );
                }
            }

            return encoding.GetString( textBuf.ToArray() );
        }

        public static uint get_DWORD( Stream fs )
        {
            var buffer = new byte[ 4 ];

            if( fs.Read( buffer, 0, 4 ) == 0 )
                throw new EndOfStreamException();

            return BitConverter.ToUInt32( buffer, 0 );
        }

        public static Quaternion get_Quaternion( Stream fs )
        {
            var buffer = new byte[ 16 ];
            fs.Read( buffer, 0, 16 );

            return new Quaternion(
                BitConverter.ToSingle( buffer, 0 ),
                BitConverter.ToSingle( buffer, 4 ),
                BitConverter.ToSingle( buffer, 8 ),
                BitConverter.ToSingle( buffer, 12 ) );
        }
    }
}
