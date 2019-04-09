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
    public class SDEF : ボーンウェイト
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

        /// <summary>
        ///     点C。
        /// </summary>
        public Vector3 SDEF_C;

        /// <summary>
        ///     点R0。
        /// </summary>
        public Vector3 SDEF_R0;

        /// <summary>
        ///     点R1。
        ///     ※修正値を要計算
        /// </summary>
        public Vector3 SDEF_R1;
    }
}
