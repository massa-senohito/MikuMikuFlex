using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.モーフ"/> に追加情報を付与するクラス。
    /// </summary>
    class PMXモーフ制御 : IDisposable
    {
        public PMXFormat.モーフ PMXFモーフ { get; protected set; }

        public float モーフ値 { get; set; }

        public アニメ変数<float> アニメ遷移;

        

        // 生成と終了


        public PMXモーフ制御( PMXFormat.モーフ morph )
        {
            this.PMXFモーフ = morph;
            this.モーフ値 = 0;
            this.アニメ遷移 = new アニメ変数<float>( 0f );
        }

        public virtual void Dispose()
        {
            this.PMXFモーフ = null;
        }



        // 更新


        public void モーフを適用する( double 現在時刻sec, PMXモデル PMXモデル )
        {
            var 現在値 = this.アニメ遷移.更新する( 現在時刻sec );

            switch( this.PMXFモーフ.モーフ種類 )
            {
                case PMXFormat.モーフ種別.頂点:
                    #region " 頂点モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.頂点モーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].Position += new Vector4( offset.座標オフセット量 * 現在値, 0f );
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.UV:
                    #region " UVモーフ "
                    //----------------
                    {
                        foreach( PMXFormat.UVモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].UV += new Vector2( offset.UVオフセット量.X, offset.UVオフセット量.Y ) * 現在値;
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.追加UV1:
                    #region " 追加UV1モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.UVモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].AddUV1 += offset.UVオフセット量 * 現在値;
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.追加UV2:
                    #region " 追加UV2モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.UVモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].AddUV2 += offset.UVオフセット量 * 現在値;
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.追加UV3:
                    #region " 追加UV3モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.UVモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].AddUV3 += offset.UVオフセット量 * 現在値;
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.追加UV4:
                    #region " 追加UV4モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.UVモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].AddUV4 += offset.UVオフセット量 * 現在値;
                            PMXモデル.PMX頂点制御.頂点の変更を通知する( (int) offset.頂点インデックス );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.ボーン:
                    #region " ボーンモーフ "
                    //----------------
                    {
                        foreach( PMXFormat.ボーンモーフオフセット offset in this.PMXFモーフ.モーフオフセットリスト )
                        {
                            var bone = PMXモデル.PMXボーン制御リスト[ offset.ボーンインデックス ];

                            bone.移動 += offset.移動量 * 現在値;
                            bone.回転 *= new Quaternion(
                                offset.回転量.X * 現在値,
                                offset.回転量.Y * 現在値,
                                offset.回転量.Z * 現在値,
                                offset.回転量.W * 現在値 );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.材質:
                    break;

                case PMXFormat.モーフ種別.グループ:
                    break;

                case PMXFormat.モーフ種別.フリップ:
                    break;

                case PMXFormat.モーフ種別.インパルス:
                    break;
            }
        }
    }
}
