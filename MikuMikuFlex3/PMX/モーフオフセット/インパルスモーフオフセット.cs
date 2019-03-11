using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMX
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


        public インパルスモーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal インパルスモーフオフセット( FileStream fs, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.インパルス;
            this.剛体インデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.ローカルフラグ = ParserHelper.get_Byte( fs );
            this.移動速度 = ParserHelper.get_Float3( fs );
            this.回転トルク = ParserHelper.get_Float3( fs );
        }
    }
}
