using System;
using System.Collections.Generic;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     ウェイト 1.0 の単一ボーンの参照インデックス。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF1 : BoneWeight
    {
        /// <summary>
        ///     ボーンのインデックス。
        ///     -1 なら非参照。
        /// </summary>
        public int boneReferenceIndex;
    }
}
