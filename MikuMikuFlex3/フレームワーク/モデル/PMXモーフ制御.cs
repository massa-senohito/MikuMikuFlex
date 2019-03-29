using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.モーフ"/> に追加情報を付与するクラス。
    /// </summary>
    class PMXモーフ制御 : IDisposable
    {
        public PMXFormat.モーフ PMXFモーフ { get; protected set; }



        // 生成と終了


        public PMXモーフ制御( PMXFormat.モーフ morph )
        {
            this.PMXFモーフ = morph;
        }

        public virtual void Dispose()
        {
            this.PMXFモーフ = null;
        }
    }
}
