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

        public void 状態をリセットする( int 追加UV数, PMXFormat.頂点リスト 初期リスト )
        {
            // 移動された頂点について、状態を初期化する。
            foreach( int i in this._変更を受けた頂点のインデックス集合 )
            {
                var iv = 初期リスト[ i ];

                this.入力頂点配列[ i ].Position.X = iv.位置.X;
                this.入力頂点配列[ i ].Position.Y = iv.位置.Y;
                this.入力頂点配列[ i ].Position.Z = iv.位置.Z;
                this.入力頂点配列[ i ].Position.W = 1f;
                this.入力頂点配列[ i ].UV = iv.UV;
                switch( 追加UV数 )
                {
                    case 0:
                        break;

                    case 1:
                        this.入力頂点配列[ i ].AddUV1 = iv.追加UV[ 0 ];
                        break;

                    case 2:
                        this.入力頂点配列[ i ].AddUV1 = iv.追加UV[ 0 ];
                        this.入力頂点配列[ i ].AddUV2 = iv.追加UV[ 1 ];
                        break;

                    case 3:
                        this.入力頂点配列[ i ].AddUV1 = iv.追加UV[ 0 ];
                        this.入力頂点配列[ i ].AddUV2 = iv.追加UV[ 1 ];
                        this.入力頂点配列[ i ].AddUV3 = iv.追加UV[ 2 ];
                        break;

                    case 4:
                        this.入力頂点配列[ i ].AddUV1 = iv.追加UV[ 0 ];
                        this.入力頂点配列[ i ].AddUV2 = iv.追加UV[ 1 ];
                        this.入力頂点配列[ i ].AddUV3 = iv.追加UV[ 2 ];
                        this.入力頂点配列[ i ].AddUV4 = iv.追加UV[ 3 ];
                        break;
                }
            }

            // フラグをクリアする。
            this._変更を受けた頂点のインデックス集合.Clear();
        }


        /// <summary>
        ///     すべての頂点を初期化するには数が多いので、移動された頂点を記録しておいて、
        ///     記録された頂点についてのみ初期化するようにする。
        /// </summary>
        private List<int> _変更を受けた頂点のインデックス集合 = new List<int>();
    }
}
