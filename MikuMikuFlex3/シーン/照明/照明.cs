using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     光源が不可視の並行光線。
    /// </summary>
    public class Illumination
    {
        public Vector3 IrradiationDirection { get; set; }

        public Color3 Color { get; set; }


        public Illumination()
        {
            this.IrradiationDirection = new Vector3( -0.5f, -1.0f, 0.5f );  // MMD での初期値
            this.Color = new Color3( 0.6f, 0.6f, 0.6f );          // 
        }
    }
}
