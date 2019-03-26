using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
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
        public string モーフ名 { get; private set; }

        public string モーフ名_英 { get; private set; }

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

        public モーフ種別 モーフ種類 { get; private set; }

        public int モーフオフセット数 { get; private set; }

        public List<モーフオフセット> モーフオフセットリスト { get; private set; }


        public モーフ()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal モーフ( FileStream fs, ヘッダ header )
        {
            this.モーフオフセットリスト = new List<モーフオフセット>();
            this.モーフ名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.モーフ名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.操作パネル = ParserHelper.get_Byte( fs );
            byte Morphtype = ParserHelper.get_Byte( fs );
            this.モーフオフセット数 = ParserHelper.get_Int( fs );

            for( int i = 0; i < this.モーフオフセット数; i++ )
            {
                switch( Morphtype )
                {
                    case 0:
                        //Group Morph
                        this.モーフ種類 = モーフ種別.グループ;
                        this.モーフオフセットリスト.Add( new グループモーフオフセット( fs, header ) );
                        break;
                    case 1:
                        //Vertex Morph
                        this.モーフ種類 = モーフ種別.頂点;
                        this.モーフオフセットリスト.Add( new 頂点モーフオフセット( fs, header ) );
                        break;
                    case 2:
                        this.モーフ種類 = モーフ種別.ボーン;
                        this.モーフオフセットリスト.Add( new ボーンモーフオフセット( fs, header ) );
                        break;
                    //3~7はすべてUVMorph
                    case 3:
                        this.モーフ種類 = モーフ種別.UV;
                        this.モーフオフセットリスト.Add( new UVモーフオフセット( fs, header, モーフ種別.UV ) );
                        break;
                    case 4:
                        this.モーフ種類 = モーフ種別.追加UV1;
                        this.モーフオフセットリスト.Add( new UVモーフオフセット( fs, header, モーフ種別.追加UV1 ) );
                        break;
                    case 5:
                        this.モーフ種類 = モーフ種別.追加UV2;
                        this.モーフオフセットリスト.Add( new UVモーフオフセット( fs, header, モーフ種別.追加UV2 ) );
                        break;
                    case 6:
                        this.モーフ種類 = モーフ種別.追加UV3;
                        this.モーフオフセットリスト.Add( new UVモーフオフセット( fs, header, モーフ種別.追加UV3 ) );
                        break;
                    case 7:
                        this.モーフ種類 = モーフ種別.追加UV4;
                        this.モーフオフセットリスト.Add( new UVモーフオフセット( fs, header, モーフ種別.追加UV4 ) );
                        break;
                    case 8:
                        //Material Morph
                        this.モーフ種類 = モーフ種別.材質;
                        this.モーフオフセットリスト.Add( new 材質モーフオフセット( fs, header ) );
                        break;
                    case 9:
                        if( header.PMXバージョン < 2.1 ) throw new InvalidDataException( "FlipモーフはPMX2.1以降でサポートされています。" );
                        this.モーフ種類 = モーフ種別.フリップ;
                        this.モーフオフセットリスト.Add( new フリップモーフオフセット( fs, header ) );
                        break;
                    case 10:
                        if( header.PMXバージョン < 2.1 ) throw new InvalidDataException( "ImpulseモーフはPMX2.1以降でサポートされています。" );
                        this.モーフ種類 = モーフ種別.インパルス;
                        this.モーフオフセットリスト.Add( new インパルスモーフオフセット( fs, header ) );
                        break;
                }
            }
        }
    }
}
