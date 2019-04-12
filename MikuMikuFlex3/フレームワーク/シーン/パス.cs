using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public abstract class パス
    {
        public string 名前 = null;

        public abstract void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters );
    }
}
