using System;
using System.Collections.Generic;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     ボーン2つの参照インデックスと、ボーン1のウェイト値(PMD方式)。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF2 : ボーンウェイト
    {
        /// <summary>
        ///     ボーン１のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone1ReferenceIndex;

        /// <summary>
        ///     ボーン２のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone2ReferenceIndex;

        /// <summary>
        ///     ボーン１のウェイト値。
        /// </summary>
        public float Bone1Weight;

        /// <summary>
        ///     ボーン２のウェイト値。
        /// </summary>
        public float Bone2Weight => ( 1.0f - Bone1Weight );
    }
}