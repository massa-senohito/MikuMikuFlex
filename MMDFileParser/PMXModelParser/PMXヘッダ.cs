using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    public class PMXヘッダ
    {
        /// <summary>
        ///     PMXのバージョン。2.0 か 2.1 。
        /// </summary>
        public float PMXバージョン { get; private set; }

        public enum EncodeType
        {
            UTF8,
            UTF16LE,
        }
        public EncodeType エンコード方式 { get; private set; }

        /// <summary>
        ///     0～4 。
        /// </summary>
        public int 追加UV数 { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int 頂点インデックスサイズ { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int テクスチャインデックスサイズ { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int 材質インデックスサイズ { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int ボーンインデックスサイズ { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int モーフインデックスサイズ { get; private set; }

        /// <summary>
        ///     1, 2, 4 のいずれか。
        /// </summary>
        public int 剛体インデックスサイズ { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static PMXヘッダ 読み込む( FileStream fs )
        {
            var header = new PMXヘッダ();

            // マジックナンバー("PMX "の４バイト）の読み取り

            var MagicNumberbuf = new byte[ 4 ];
            fs.Read( MagicNumberbuf, 0, 4 );
            if( Encoding.Unicode.GetString( MagicNumberbuf, 0, 4 ) != "PMX " &&
                Encoding.UTF8.GetString( MagicNumberbuf, 0, 4 ) != "PMX " )
            {
                throw new InvalidDataException( "PMXファイルのマジックナンバーが間違っています。ファイルが破損しているか、対応バージョンではありません。" );
            }

            // バージョン情報の読み取り

            header.PMXバージョン = ParserHelper.get_Float( fs );

            // 後のデータ列のバイト列

            if( ParserHelper.get_Byte( fs ) != 8 )
                throw new NotImplementedException();  // PMX2.0 は 8 で固定

            byte[] descriptionbuf = new byte[ 8 ];

            // 詳細のデータ（８バイト固定）

            fs.Read( descriptionbuf, 0, 8 );
            header.エンコード方式 = descriptionbuf[ 0 ] == 1 ? EncodeType.UTF8 : EncodeType.UTF16LE;
            header.追加UV数 = descriptionbuf[ 1 ];
            header.頂点インデックスサイズ = descriptionbuf[ 2 ];
            header.テクスチャインデックスサイズ = descriptionbuf[ 3 ];
            header.材質インデックスサイズ = descriptionbuf[ 4 ];
            header.ボーンインデックスサイズ = descriptionbuf[ 5 ];
            header.モーフインデックスサイズ = descriptionbuf[ 6 ];
            header.剛体インデックスサイズ = descriptionbuf[ 7 ];

            return header;
        }
    }
}
