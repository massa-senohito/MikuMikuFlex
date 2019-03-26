using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class 頂点モーフオフセット : モーフオフセット
    {
        public uint 頂点インデックス { get; private set; }

        public Vector3 座標オフセット量 { get; private set; }


        public 頂点モーフオフセット()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 頂点モーフオフセット( FileStream fs, ヘッダ header )
        {
            this.モーフ種類 = モーフ種別.頂点;
            this.頂点インデックス = ParserHelper.get_VertexIndex( fs, header.頂点インデックスサイズ );
            this.座標オフセット量 = ParserHelper.get_Float3( fs );
        }
    }
}
