using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     ボーンの方向（"＞" の向き；表示先）を示す指標。
    /// </summary>
    public enum ボーンの接続先表示方法 : byte
    {
        /// <summary>
        ///     方向を、ボーンからの相対位置で指定する。
        /// </summary>
        相対座標で指定 = 0,

        /// <summary>
        ///     方向を、ボーンで指定する。
        ///     このボーンは、PMDでは「子ボーン」と表現されるが、PMXではただの「表示先」であり、親子関係を表すものではないので注意。
        /// </summary>
        ボーンで指定 = 1,
    }
}
