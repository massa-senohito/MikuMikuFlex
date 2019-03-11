using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMX
{
    public enum スフィアモード : byte
    {
        無効 = 0,
        乗算 = 1,
        加算 = 2,
        /// <summary>
        ///     追加UV1のx,yをUV参照して通常テクスチャ描画を行う。
        /// </summary>
        サブテクスチャ = 3,
    }
}
