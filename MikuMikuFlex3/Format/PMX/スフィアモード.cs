using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public enum SphereMode : byte
    {
        Invalid = 0,
        Multiply = 1,
        Addition = 2,
        /// <summary>
        ///     AddToUV1のx,yをUV参照して通常テクスチャ描画を行う。
        /// </summary>
        Subtexture = 3,
    }
}
