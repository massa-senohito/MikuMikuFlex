using System;
using System.Collections.Generic;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     Bourne2つの参照インデックスと、Bourne1のウェイト値(PMD方式)。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF2 : BoneWeight
    {
        /// <summary>
        ///     Bourne１のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone1ReferenceIndex;

        /// <summary>
        ///     Bourne２のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone2ReferenceIndex;

        /// <summary>
        ///     Bourne１のウェイト値。
        /// </summary>
        public float Bone1Weight;

        /// <summary>
        ///     Bourne２のウェイト値。
        /// </summary>
        public float Bone2Weight => ( 1.0f - Bone1Weight );
    }
}
