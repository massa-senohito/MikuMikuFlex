using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.BoneWeight
{
    /// <summary>
    ///     ボーン4つの参照インデックスと、それぞれのウェイト値。
    ///     ウェイトの合計が1.0である保障はしない。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF4 : ボーンウェイト
    {
        public int Bone1ReferenceIndex;

        public int Bone2ReferenceIndex;

        public int Bone3ReferenceIndex;

        public int Bone4ReferenceIndex;

        /// <summary>
        ///     ボーン1～4のウェイト値。
        ///     (x,y,z,w) = (Bone1Weight, Bone2Weight, Bone3Weight, Bone4Weight)
        /// </summary>
        public Vector4 Weights;
    }
}