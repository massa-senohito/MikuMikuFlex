using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public enum ジョイント種別
    {
        /// <summary>
        ///     ６つの拘束制限、ばね付き
        /// </summary>
        ばね付き6DOF = 0,

        /// <summary>
        ///     ６つの拘束制限、ばねなし (PMX2.1)
        /// </summary>
        基本6DOF = 1,

        /// <summary>
        ///     点接続。 (PMX2.1) 
        ///     単純に２つの剛体を点でつなげる、拘束制限のないジョイント。
        /// </summary>
        P2P = 2,

        /// <summary>
        ///     コーンツイスト。(PMX2.1)
        ///     円錐の範囲内を制限とするジョイント。
        /// </summary>
        円錐回転 = 3,

        /// <summary>
        ///     軸移動。(PMX2.1)
        ///     １つの軸を方向の制限とするジョイント。
        /// </summary>
        スライダー = 5,

        /// <summary>
        ///     軸回転。(PMX2.1)
        ///     １つの軸に対する回転のみ行えるジョイント。
        /// </summary>
        ヒンジ = 6,
    }
}
