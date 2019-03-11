using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public enum 剛体の物理演算
    {
        ボーン追従 = 0,
        物理演算 = 1,
        物理演算とボーン位置合わせ = 2,
    }
}
