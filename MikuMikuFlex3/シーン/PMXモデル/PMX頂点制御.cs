using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class PMX頂点制御
    {
        public CS_INPUT[] 入力頂点配列;

        public bool[] 単位更新フラグ;

        public const int 単位更新の頂点数 = 1500;
        


        // 生成と終了


        public PMX頂点制御( CS_INPUT[] 初期配列 )
        {
            this.入力頂点配列 = 初期配列;

            int 単位数 = 初期配列.Length / 単位更新の頂点数 + 1;

            this.単位更新フラグ = new bool[ 単位数 ];
            this._単位更新フラグステータス = new 単位更新フラグステータス[ 単位数 ];

            for( int i = 0; i < this.単位更新フラグ.Length; i++ )
            {
                this.単位更新フラグ[ i ] = false;
                this._単位更新フラグステータス[ i ] = 単位更新フラグステータス.変更なし;
            }
        }



        // 更新


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

            this._変更を受けた頂点のインデックス集合.Clear();


            // フラグをローテーションする。
            //   変更なし/false   → 変更なし/false
            //   変更あり/true    → 初期化あり/true
            //   初期かあり/true  → 変更なし/false

            for( int i = 0; i < this._単位更新フラグステータス.Length; i++ )
            {
                if( this._単位更新フラグステータス[ i ] == 単位更新フラグステータス.変更あり )
                {
                    this._単位更新フラグステータス[ i ] = 単位更新フラグステータス.初期化あり;
                }
                else if( this._単位更新フラグステータス[ i ] == 単位更新フラグステータス.初期化あり )
                {
                    this._単位更新フラグステータス[ i ] = 単位更新フラグステータス.変更なし;
                    this.単位更新フラグ[ i ] = false;
                }
            }
        }

        public void 頂点の変更を通知する( int 頂点インデックス )
        {
            this._変更を受けた頂点のインデックス集合.Add( 頂点インデックス );

            int 単位インデックス = 頂点インデックス / 単位更新の頂点数;

            this.単位更新フラグ[ 単位インデックス ] = true;
            this._単位更新フラグステータス[ 単位インデックス ] = 単位更新フラグステータス.変更あり;
        }



        // private


        /// <summary>
        ///     すべての頂点を初期化するには数が多いので、移動された頂点を記録しておいて、
        ///     記録された頂点についてのみ初期化するようにする。
        /// </summary>
        private List<int> _変更を受けた頂点のインデックス集合 = new List<int>();

        private enum 単位更新フラグステータス
        {                 // 更新フラグが:
            変更なし,     // 前回 false      → 今回 false
            変更あり,     // 前回 false/true → 今回 true
            初期化あり,   // 前回 true       → 今回 false
        }
        private 単位更新フラグステータス[] _単位更新フラグステータス;
    }
}
