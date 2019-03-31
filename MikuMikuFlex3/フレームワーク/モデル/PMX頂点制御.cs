using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    class PMX頂点制御
    {
        public CS_INPUT[] 入力頂点配列;


        public void 頂点の移動を通知する( int 頂点インデックス )
        {
            this._移動された頂点.Add( 頂点インデックス );
        }

        public void 頂点の移動情報をクリアする( PMXFormat.頂点リスト 初期リスト )
        {
            // 移動された頂点について、位置情報を初期化する。
            foreach( int i in this._移動された頂点 )
                this.入力頂点配列[ i ].Position = new Vector4( 初期リスト[ i ].位置, 1f );

            // フラグをクリアする。
            this._移動された頂点 = new HashSet<int>();
        }


        /// <summary>
        ///     すべての頂点を初期化するには数が多いので、移動された頂点を記録しておいて、
        ///     記録された頂点についてのみ初期化するようにする。
        /// </summary>
        private HashSet<int> _移動された頂点 = new HashSet<int>();
    }
}
