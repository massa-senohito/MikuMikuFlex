using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     3点(3頂点の参照インデックス)で1面。
    ///     材質毎の面数は材質内の面（頂点）数で管理（同PMD方式）
    /// </summary>
    public class 面
    {
        public uint 頂点1;

        public uint 頂点2;

        public uint 頂点3;

        public 面( uint 頂点1, uint 頂点2, uint 頂点3 )
        {
            this.頂点1 = 頂点1;
            this.頂点2 = 頂点2;
            this.頂点3 = 頂点3;
        }
    }
}
