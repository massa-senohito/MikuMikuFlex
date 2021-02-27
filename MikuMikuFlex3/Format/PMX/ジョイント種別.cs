using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public enum JointType
    {
        /// <summary>
        ///     ６つの拘束制限、WithSpring
        /// </summary>
        WithSpring6DOF = 0,

        /// <summary>
        ///     ６つの拘束制限、ばねなし (PMX2.1)
        /// </summary>
        Basic6DOF = 1,

        /// <summary>
        ///     点接続。 (PMX2.1) 
        ///     単純に２つの剛体を点でつなげる、拘束制限のないジョイント。
        /// </summary>
        P2P = 2,

        /// <summary>
        ///     コーンツイスト。(PMX2.1)
        ///     円錐の範囲内を制限とするジョイント。
        /// </summary>
        ConeRotation = 3,

        /// <summary>
        ///     軸移動。(PMX2.1)
        ///     １つの軸を方向の制限とするジョイント。
        /// </summary>
        Slider = 5,

        /// <summary>
        ///     AxleRotation。(PMX2.1)
        ///     １つの軸に対する回転のみ行えるジョイント。
        /// </summary>
        Hinge = 6,
    }
}
