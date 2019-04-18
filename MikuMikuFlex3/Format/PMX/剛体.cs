using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class 剛体
    {
        public string 剛体名 { get; private set; }

        public string 剛体名_英 { get; private set; }

        /// <summary>
        ///     関連なしの場合は -1
        /// </summary>
        public int 関連ボーンインデックス { get; private set; }

        public byte グループ { get; private set; }

        public ushort 非衝突グループフラグ { get; private set; }

        public 剛体形状 形状 { get; private set; }

        public Vector3 サイズ { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }

        public float 質量 { get; private set; }

        public float 移動減衰 { get; private set; }

        public float 回転減衰 { get; private set; }

        public float 反発力 { get;private set; }

        public float 摩擦力 { get; private set; }

        public 剛体の物理演算 物理演算 { get; private set; }


        public 剛体()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 剛体( Stream fs, ヘッダ header )
        {
            this.剛体名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.剛体名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            this.関連ボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            this.グループ = ParserHelper.get_Byte( fs );
            this.非衝突グループフラグ = ParserHelper.get_UShort( fs );
            this.形状 = (剛体形状) ParserHelper.get_Byte( fs );
            this.サイズ = ParserHelper.get_Float3( fs );
            this.位置 = ParserHelper.get_Float3( fs );
            this.回転rad = ParserHelper.get_Float3( fs );
            this.質量 = ParserHelper.get_Float( fs );
            this.移動減衰 = ParserHelper.get_Float( fs );
            this.回転減衰 = ParserHelper.get_Float( fs );
            this.反発力 = ParserHelper.get_Float( fs );
            this.摩擦力 = ParserHelper.get_Float( fs );
            this.物理演算 = (剛体の物理演算) ParserHelper.get_Byte( fs );
        }
    }
}
