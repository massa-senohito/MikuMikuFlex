using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser
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

        public enum 剛体形状
        {
            球 = 0,
            箱 = 1,
            カプセル = 2,
        }
        public 剛体形状 形状 { get; private set; }

        public Vector3 サイズ { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }

        public float 質量 { get; private set; }

        public float 移動減衰 { get; private set; }

        public float 回転減衰 { get; private set; }

        public float 反発力 { get;private set; }

        public float 摩擦力 { get; private set; }

        public enum 剛体の物理演算
        {
            ボーン追従 = 0,
            物理演算 = 1,
            物理演算とボーン位置合わせ = 2,
        }
        public 剛体の物理演算 物理演算 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 剛体 読み込む( Stream fs, PMXヘッダ header )
        {
            var data = new 剛体();

            data.剛体名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            data.剛体名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            data.関連ボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            data.グループ = ParserHelper.get_Byte( fs );
            data.非衝突グループフラグ = ParserHelper.get_UShort( fs );
            data.形状 = (剛体形状) ParserHelper.get_Byte( fs );
            data.サイズ = ParserHelper.get_Float3( fs );
            data.位置 = ParserHelper.get_Float3( fs );
            data.回転rad = ParserHelper.get_Float3( fs );
            data.質量 = ParserHelper.get_Float( fs );
            data.移動減衰 = ParserHelper.get_Float( fs );
            data.回転減衰 = ParserHelper.get_Float( fs );
            data.反発力 = ParserHelper.get_Float( fs );
            data.摩擦力 = ParserHelper.get_Float( fs );
            data.物理演算 = (剛体の物理演算) ParserHelper.get_Byte( fs );

            return data;
        }
    }
}
