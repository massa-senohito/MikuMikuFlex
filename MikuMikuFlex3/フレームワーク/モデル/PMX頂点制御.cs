using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    class PMX頂点制御
    {

        // 基本情報


        public PMXFormat.頂点 PMXF頂点 { get; protected set; }



        // 生成と終了


        public PMX頂点制御( PMXFormat.頂点 vertex )
        {
            this.PMXF頂点 = vertex;
        }



        // 進行


        public void 更新する( CS_INPUT[] 入力頂点配列 )
        {

        }
    }
}
