using MMDFileParser.PMXModelParser.MorphOffset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser
{
    /// <remarks>
    ///     格納可能なモーフは大別して、
    ///         頂点モーフ
    ///         UVモーフ
    ///         ボーンモーフ
    ///         材質モーフ
    ///         グループモーフ
    ///     の5種類。さらにUVモーフは、UV／追加UV1～4の計5種類に分類される。
    ///     ※追加UV数によっては不要なUVモーフが格納されることがあるが、モーフ側は特に削除などは行わないので注意。
    /// </remarks>
    public class モーフ
    {
        public String モーフ名 { get; private set; }

        public String モーフ名_英 { get; private set; }

        /// <summary>
        ///     0: システム予約
        ///     1: まゆ（左下）
        ///     2: 目（左上）
        ///     3: 口（右上）
        ///     4: その他（右下）
        /// </summary>
        /// <remarks>
        ///     特に使用しないので、enum 定義もしない。
        /// </remarks>
        public byte 操作パネル { get; private set; }

        public モーフ種類 モーフ種類 { get; private set; }

        public int モーフオフセット数 { get; private set; }

        public List<モーフオフセット> モーフオフセットリスト { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static モーフ 読み込む( FileStream fs, PMXヘッダ header )
        {
            var morph = new モーフ();

            morph.モーフオフセットリスト = new List<モーフオフセット>();
            morph.モーフ名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            morph.モーフ名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            morph.操作パネル = ParserHelper.get_Byte( fs );
            byte Morphtype = ParserHelper.get_Byte( fs );
            morph.モーフオフセット数 = ParserHelper.get_Int( fs );

            for( int i = 0; i < morph.モーフオフセット数; i++ )
            {
                switch( Morphtype )
                {
                    case 0:
                        //Group Morph
                        morph.モーフ種類 = モーフ種類.グループ;
                        morph.モーフオフセットリスト.Add( グループモーフオフセット.読み込む( fs, header ) );
                        break;
                    case 1:
                        //Vertex Morph
                        morph.モーフ種類 = モーフ種類.頂点;
                        morph.モーフオフセットリスト.Add( 頂点モーフオフセット.読み込む( fs, header ) );
                        break;
                    case 2:
                        morph.モーフ種類 = モーフ種類.ボーン;
                        morph.モーフオフセットリスト.Add( ボーンモーフオフセット.読み込む( fs, header ) );
                        break;
                    //3~7はすべてUVMorph
                    case 3:
                        morph.モーフ種類 = モーフ種類.UV;
                        morph.モーフオフセットリスト.Add( UVモーフオフセット.読み込む( fs, header, モーフ種類.UV ) );
                        break;
                    case 4:
                        morph.モーフ種類 = モーフ種類.追加UV1;
                        morph.モーフオフセットリスト.Add( UVモーフオフセット.読み込む( fs, header, モーフ種類.追加UV1 ) );
                        break;
                    case 5:
                        morph.モーフ種類 = モーフ種類.追加UV2;
                        morph.モーフオフセットリスト.Add( UVモーフオフセット.読み込む( fs, header, モーフ種類.追加UV2 ) );
                        break;
                    case 6:
                        morph.モーフ種類 = モーフ種類.追加UV3;
                        morph.モーフオフセットリスト.Add( UVモーフオフセット.読み込む( fs, header, モーフ種類.追加UV3 ) );
                        break;
                    case 7:
                        morph.モーフ種類 = モーフ種類.追加UV4;
                        morph.モーフオフセットリスト.Add( UVモーフオフセット.読み込む( fs, header, モーフ種類.追加UV4 ) );
                        break;
                    case 8:
                        //Material Morph
                        morph.モーフ種類 = モーフ種類.材質;
                        morph.モーフオフセットリスト.Add( 材質モーフオフセット.読み込む( fs, header ) );
                        break;
                    case 9:
                        if( header.PMXバージョン < 2.1 ) throw new InvalidDataException( "FlipモーフはPMX2.1以降でサポートされています。" );
                        morph.モーフ種類 = モーフ種類.フリップ;
                        morph.モーフオフセットリスト.Add( フリップモーフオフセット.読み込む( fs, header ) );
                        break;
                    case 10:
                        if( header.PMXバージョン < 2.1 ) throw new InvalidDataException( "ImpulseモーフはPMX2.1以降でサポートされています。" );
                        morph.モーフ種類 = モーフ種類.インパルス;
                        morph.モーフオフセットリスト.Add( インパルスモーフオフセット.読み込む( fs, header ) );
                        break;
                }
            }
            return morph;
        }
    }
}
