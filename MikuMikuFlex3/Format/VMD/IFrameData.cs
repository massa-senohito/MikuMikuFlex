using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public interface IFrameData: IComparable
    {
        uint FrameNumber { get; }
    }
}
