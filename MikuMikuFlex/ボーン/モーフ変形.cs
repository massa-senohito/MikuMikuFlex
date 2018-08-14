using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMF.ボーン
{
    public class モーフ変形
    {
        public string モーフ名 { get; private set; }

        public float モーフ値 { get; set; }


        public モーフ変形( string モーフ名 )
        {
            this.モーフ名 = モーフ名;
        }
    }
}
