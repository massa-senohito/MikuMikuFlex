using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.モデル.Controller
{
    [Flags]
    public enum TransformType
    {
        Translation = 0x01,
        Rotation = 0x02,
        Scaling = 0x04,
        All = 0x07,
    }
}
