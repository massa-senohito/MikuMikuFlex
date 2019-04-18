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
    public class 照明
    {
        public Vector3 照射方向 { get; set; }

        public Color3 色 { get; set; }


        public 照明()
        {
            this.照射方向 = new Vector3( -0.5f, -1.0f, 0.5f );  // MMD での初期値
            this.色 = new Color3( 0.6f, 0.6f, 0.6f );          // 
        }
    }
}
