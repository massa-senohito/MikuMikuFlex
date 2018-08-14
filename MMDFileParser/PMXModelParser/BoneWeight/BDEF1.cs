using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDFileParser.PMXModelParser.BoneWeight
{
    /// <summary>
    ///     ウェイト 1.0 の単一ボーンの参照インデックス。
    ///     参照インデックスは -1（非参照）の場合があるので注意。
    /// </summary>
    public class BDEF1 : ボーンウェイト
    {
        public int boneReferenceIndex;
    }
}
