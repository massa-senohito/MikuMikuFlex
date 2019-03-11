using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public enum ジョイント種別
    {
        ばね付き6DOF = 0,  // PMX2.0では 0 のみ(拡張用)
        基本6DOF = 1,
        P2P = 2,
        円錐回転 = 3,
        スライダー = 5,
        ヒンジ = 6,
    }
}
