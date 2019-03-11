using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.PMX
{
    public enum ボーンウェイト種別 : byte
    {
        /// <summary>
        ///     ウェイト 1.0 の単一ボーンの参照インデックス。
        /// </summary>
        /// <seealso cref="BoneWeight.BDEF1"/>
        BDEF1 = 0,

        /// <summary>
        ///     ボーン2つの参照インデックスと、ボーン1のウェイト値(PMD方式)。
        ///     ボーン2のウェイト値は [1.0 - ボーン1のウェイト値] となる。
        /// </summary>
        /// <seealso cref="BoneWeight.BDEF2"/>
        BDEF2 = 1,

        /// <summary>
        ///     ボーン4つの参照インデックスと、それぞれのウェイト値。
        ///     ウェイトの合計が1.0である保障はしない。
        /// </summary>
        /// <seealso cref="BoneWeight.BDEF4"/>
        BDEF4 = 2,

        /// <summary>
        ///     BDEF2に加え、SDEF用のfloat3(Vector3)が3つ。
        ///     実際の計算ではさらに補正値の算出が必要(一応そのままBDEF2としても使用可能)
        /// </summary>
        /// <seealso cref="BoneWeight.SDEF"/>
        SDEF = 3,

        /// <summary>
        ///     ボーン4つと、それぞれのウェイト値。(PMX2.1拡張)
        ///     ウェイト合計が1.0である保障はしない。
        /// </summary>
        /// <seealso cref="BoneWeight.QDEF"/>
        QDEF = 4,
    }
}
