using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.材質"/> に追加情報を付与するクラス。
    /// </summary>
    class PMX材質
    {

        // 基本情報


        public PMXFormat.材質 PMXF材質 { get; protected set; }



        // 動的情報（入力）


        public float テッセレーション係数 { get; protected set; }



        // 生成と終了


        public PMX材質( PMXFormat.材質 material )
        {
            this.PMXF材質 = material;
            this.テッセレーション係数 = 1.0f;
        }
    }
}
