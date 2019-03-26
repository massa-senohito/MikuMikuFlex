using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class ボーンモーフオフセット : モーフオフセット
    {
        public int ボーンインデックス { get; private set; }

        public Vector3 移動量 { get; private set; }

        /// <summary>
        ///     クォータニオン(x,y,z,w)
        /// </summary>
        public Vector4 回転量 { get; private set; }


        public ボーンモーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ボーンモーフオフセット( FileStream fs, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.ボーン;
            this.ボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            this.移動量 = ParserHelper.get_Float3( fs );
            this.回転量 = ParserHelper.get_Float4( fs );
        }
    }
}
