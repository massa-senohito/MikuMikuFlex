using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     BDEF2に加え、SDEF用のfloat3(Vector3)が3つ。
    ///     実際の計算ではさらに補正値の算出が必要(一応そのままBDEF2としても使用可能)
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class SDEF : BoneWeight
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

        /// <summary>
        ///     PointC。
        /// </summary>
        public Vector3 SDEF_C;

        /// <summary>
        ///     PointR0。
        /// </summary>
        public Vector3 SDEF_R0;

        /// <summary>
        ///     PointR1。
        ///     ※修正値を要計算
        /// </summary>
        public Vector3 SDEF_R1;
    }
}
