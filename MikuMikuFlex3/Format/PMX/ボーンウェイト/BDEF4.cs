using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     Bourne4つの参照インデックスと、それぞれのウェイト値。
    ///     ウェイトの合計が1.0である保障はしない。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF4 : BoneWeight
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
        ///     Bourne３のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone3ReferenceIndex;

        /// <summary>
        ///     Bourne４のインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int Bone4ReferenceIndex;

        /// <summary>
        ///     Bourne１～４のウェイト値。
        ///     (x,y,z,w) = (Bone1Weight, Bone2Weight, Bone3Weight, Bone4Weight)
        /// </summary>
        public Vector4 Weights;
    }
}
