using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace MikuMikuFlex3.PMXFormat
{
    public class ヘッダ
    {
        /// <summary>
        ///     PMXのバージョン。
        ///     2.0 か 2.1 。
        /// </summary>
        public float PMXバージョン { get; private set; }

        /// <summary>
        ///     エンコード方式。UTF8 または Unicode。
        /// </summary>
        public Encoding エンコード方式 { get; private set; }

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


        public ヘッダ()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ヘッダ( Stream st )
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

            this.PMXバージョン = ParserHelper.get_Float( st );

            // 後のデータ列のバイト列

            if( ParserHelper.get_Byte( st ) != 8 )
                throw new NotImplementedException();  // PMX2.0 は 8 で固定

            byte[] descriptionbuf = new byte[ 8 ];

            // 詳細のデータ（８バイト固定）

            st.Read( descriptionbuf, 0, 8 );
            this.エンコード方式 = ( 1 == descriptionbuf[ 0 ] ) ? Encoding.UTF8 : Encoding.Unicode;   // Unicode==UTF16LE
            this.追加UV数 = descriptionbuf[ 1 ];
            this.頂点インデックスサイズ = descriptionbuf[ 2 ];
            this.テクスチャインデックスサイズ = descriptionbuf[ 3 ];
            this.材質インデックスサイズ = descriptionbuf[ 4 ];
            this.ボーンインデックスサイズ = descriptionbuf[ 5 ];
            this.モーフインデックスサイズ = descriptionbuf[ 6 ];
            this.剛体インデックスサイズ = descriptionbuf[ 7 ];
        }
    }
}
