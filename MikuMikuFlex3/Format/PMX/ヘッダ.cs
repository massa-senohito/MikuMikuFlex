using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MikuMikuFlex3.PMXFormat
{
    public class Header
    {
        /// <summary>
        ///     PMXのバージョン。
        ///     2.0 か 2.1 。
        /// </summary>
        public float PMXVersion { get; private set; }

        /// <summary>
        ///     EncodingMethod。UTF8 または Unicode。
        /// </summary>
        public Encoding EncodingMethod { get; private set; }

        /// <summary>
        ///     0～4 。
        /// </summary>
        public int AddToUVNumber { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int VertexIndexSize { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int TextureIndexSize { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int MaterialIndexSize { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int BoneIndexSize { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int MorphIndexSize { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int RigidBodyIndexSize { get; private set; }


        public Header()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Header( Stream st )
        {
            // マジックナンバー("PMX "の４バイト）の読み取り

            var MagicNumberbuf = new byte[ 4 ];
            st.Read( MagicNumberbuf, 0, 4 );
            if( Encoding.Unicode.GetString( MagicNumberbuf, 0, 4 ) != "PMX " &&
                Encoding.UTF8.GetString( MagicNumberbuf, 0, 4 ) != "PMX " )
            {
                throw new InvalidDataException( "PMXファイルのマジックナンバーが間違っています。ファイルが破損しているか、対応バージョンではありません。" );
            }

            // バージョン情報の読み取り

            this.PMXVersion = ParserHelper.get_Float( st );

            // 後のデータ列のバイト列

            if( ParserHelper.get_Byte( st ) != 8 )
                throw new NotImplementedException();  // PMX2.0 は 8 で固定

            byte[] descriptionbuf = new byte[ 8 ];

            // 詳細のデータ（８バイト固定）

            st.Read( descriptionbuf, 0, 8 );
            this.EncodingMethod = ( 1 == descriptionbuf[ 0 ] ) ? Encoding.UTF8 : Encoding.Unicode;   // Unicode==UTF16LE
            this.AddToUVNumber = descriptionbuf[ 1 ];
            this.VertexIndexSize = descriptionbuf[ 2 ];
            this.TextureIndexSize = descriptionbuf[ 3 ];
            this.MaterialIndexSize = descriptionbuf[ 4 ];
            this.BoneIndexSize = descriptionbuf[ 5 ];
            this.MorphIndexSize = descriptionbuf[ 6 ];
            this.RigidBodyIndexSize = descriptionbuf[ 7 ];
        }
    }
}
