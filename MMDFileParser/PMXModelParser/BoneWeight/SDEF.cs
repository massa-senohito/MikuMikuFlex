using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.BoneWeight
{
    /// <summary>
    ///     BDEF2に加え、SDEF用のfloat3(Vector3)が3つ。
    ///     実際の計算ではさらに補正値の算出が必要(一応そのままBDEF2としても使用可能)
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class SDEF : ボーンウェイト
    {
        public int Bone1ReferenceIndex;

        public int Bone2ReferenceIndex;

        public float Bone1Weight;

        public float Bone2Weight => ( 1.0f - Bone1Weight );

        public Vector3 SDEF_C;

        public Vector3 SDEF_R0;

        // ※修正値を要計算
        public Vector3 SDEF_R1;
    }
}
