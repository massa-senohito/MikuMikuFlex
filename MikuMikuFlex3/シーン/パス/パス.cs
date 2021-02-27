using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public abstract class Pass : IDisposable
    {
        public string GivenNames = null;


        public abstract void Dispose();

        public abstract void Draw( double CurrentTimesec, DeviceContext d3ddc, GlobalParameters globalParameters );
    }
}
