using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser.BoneWeight
{
    /// <summary>
    ///     ボーン2つの参照インデックスと、ボーン1のウェイト値(PMD方式)。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF2 : ボーンウェイト
    {
        public int Bone1ReferenceIndex;

        public int Bone2ReferenceIndex;

        public float Bone1Weight;

        /// <summary>
        ///     ボーン2のウェイト値は [1.0 - ボーン1のウェイト値] となる。
        /// </summary>
        public float Bone2Weight => ( 1.0f - Bone1Weight );
    }
}