using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public enum ローカル付与対象 : byte
    {
        ユーザ変形値_IKリンク_多重付与 = 0,
        親のローカル変形量 = 1,
    }
}
