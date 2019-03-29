using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Animation;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.モーフ"/> に追加情報を付与するクラス。
    /// </summary>
    class PMXモーフ制御 : IDisposable
    {
        public PMXFormat.モーフ PMXFモーフ { get; protected set; }

        public Variable アニメ変数 { get; protected set; }



        // 生成と終了


        public PMXモーフ制御( PMXFormat.モーフ morph, Manager manager )
        {
            this.PMXFモーフ = morph;
            this.アニメ変数 = new Variable( manager );
        }

        public virtual void Dispose()
        {
            this.アニメ変数?.Dispose();
            this.PMXFモーフ = null;
        }
    }
}
