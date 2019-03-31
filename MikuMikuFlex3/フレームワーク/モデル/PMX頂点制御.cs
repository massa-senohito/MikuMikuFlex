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


        public void 頂点の変更を通知する( int 頂点インデックス )
        {
            this._変更を受けた頂点のインデックス集合.Add( 頂点インデックス );
        }

        public void 状態をリセットする( PMXFormat.頂点リスト 初期リスト )
        {
            // 移動された頂点について、状態を初期化する。
            foreach( int i in this._変更を受けた頂点のインデックス集合 )
            {
                this.入力頂点配列[ i ].Position = new Vector4( 初期リスト[ i ].位置, 1f );
                this.入力頂点配列[ i ].UV = 初期リスト[ i ].UV;
                this.入力頂点配列[ i ].AddUV1 = ( 0 < 初期リスト[ i ].追加UV.Length ) ? 初期リスト[ i ].追加UV[ 0 ] : Vector4.Zero;
                this.入力頂点配列[ i ].AddUV2 = ( 1 < 初期リスト[ i ].追加UV.Length ) ? 初期リスト[ i ].追加UV[ 1 ] : Vector4.Zero;
                this.入力頂点配列[ i ].AddUV3 = ( 2 < 初期リスト[ i ].追加UV.Length ) ? 初期リスト[ i ].追加UV[ 2 ] : Vector4.Zero;
                this.入力頂点配列[ i ].AddUV4 = ( 3 < 初期リスト[ i ].追加UV.Length ) ? 初期リスト[ i ].追加UV[ 3 ] : Vector4.Zero;
            }

            // フラグをクリアする。
            this._変更を受けた頂点のインデックス集合 = new HashSet<int>();
        }


        /// <summary>
        ///     すべての頂点を初期化するには数が多いので、移動された頂点を記録しておいて、
        ///     記録された頂点についてのみ初期化するようにする。
        /// </summary>
        private HashSet<int> _変更を受けた頂点のインデックス集合 = new HashSet<int>();
    }
}
