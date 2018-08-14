using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class インパルスモーフオフセット : モーフオフセット
    {
        public int 剛体インデックス { get; private set; }

        /// <summary>
        ///     0:OFF, 1:ON
        /// </summary>
        public byte ローカルフラグ { get; private set; }

        /// <summary>
        ///     すべて 0 の場合は"停止制御"として特殊化
        /// </summary>
        public Vector3 移動速度 { get; private set; }

        /// <summary>
        ///     すべて 0 の場合は"停止制御"として特殊化
        /// </summary>
        public Vector3 回転トルク { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static インパルスモーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new インパルスモーフオフセット();

            offset.モーフ種類 = モーフ種類.インパルス;
            offset.剛体インデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            offset.ローカルフラグ = ParserHelper.get_Byte( fs );
            offset.移動速度 = ParserHelper.get_Float3( fs );
            offset.回転トルク = ParserHelper.get_Float3( fs );

            return offset;
        }
    }
}
