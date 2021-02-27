using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     3Point(3頂点の参照インデックス)で1Surface。
    ///     材質毎の面数は材質内の面（Vertex）数で管理（同PMD方式）
    /// </summary>
    public class Surface
    {
        public uint Vertex1;

        public uint Vertex2;

        public uint Vertex3;

        public Surface( uint Vertex1, uint Vertex2, uint Vertex3 )
        {
            this.Vertex1 = Vertex1;
            this.Vertex2 = Vertex2;
            this.Vertex3 = Vertex3;
        }
    }
}
