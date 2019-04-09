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
    public class PMXモーフ制御 : IDisposable
    {
        public string モーフ名 => this.PMXFモーフ.モーフ名;

        public PMXFormat.モーフ種別 モーフ種類 => this.PMXFモーフ.モーフ種類;


        public float モーフ値 { get; set; }

        public アニメ変数<float> アニメ変数_モーフ;

        internal PMXFormat.モーフ PMXFモーフ { get; private protected set; }



        // 生成と終了


        public PMXモーフ制御( PMXFormat.モーフ morph )
        {
            this.PMXFモーフ = morph;
            this.モーフ値 = 0;
            this.アニメ変数_モーフ = new アニメ変数<float>( 0f );
        }

        public virtual void Dispose()
        {
            this.PMXFモーフ = null;
        }



        // 更新


        internal void モーフを適用する( double 現在時刻sec, PMXモデル PMXモデル )
        {
            var 現在値 = this.アニメ変数_モーフ.更新する( 現在時刻sec );

            if( 現在値 > 0.001 )
                this._モーフを適用する( 現在値, PMXモデル, this );
        }

        private void _モーフを適用する( float 現在値, PMXモデル PMXモデル, PMXモーフ制御 適用対象モーフ )
        {
            switch( 適用対象モーフ.PMXFモーフ.モーフ種類 )
            {
                case PMXFormat.モーフ種別.頂点:
                    #region " 頂点モーフ "
                    //----------------
                    {
                        foreach( PMXFormat.頂点モーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
                        {
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].Position.X += offset.座標オフセット量.X * 現在値;
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].Position.Y += offset.座標オフセット量.Y * 現在値;
                            PMXモデル.PMX頂点制御.入力頂点配列[ offset.頂点インデックス ].Position.Z += offset.座標オフセット量.Z * 現在値;

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
                        foreach( PMXFormat.UVモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
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
                        foreach( PMXFormat.UVモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
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
                        foreach( PMXFormat.UVモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
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
                        foreach( PMXFormat.UVモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
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
                        foreach( PMXFormat.UVモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
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
                        foreach( PMXFormat.ボーンモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
                        {
                            var bone = PMXモデル.ボーンリスト[ offset.ボーンインデックス ];

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
                    #region " 材質モーフ "
                    //----------------
                    {
                        // todo: 材質モーフ・テクスチャ係数への対応
                        // todo: 材質モーフ・スフィアテクスチャ係数への対応
                        // todo: 材質モーフ・Toonテクスチャ係数への対応

                        foreach( PMXFormat.材質モーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
                        {
                            if( offset.材質インデックス == -1 ) // -1:全材質が対象
                            {
                                foreach( var 材質 in PMXモデル.材質リスト )
                                    差分セット( offset, 材質 );
                            }
                            else
                            {
                                var 材質 = PMXモデル.材質リスト[ offset.材質インデックス ];

                                差分セット( offset, 材質 );
                            }
                        }


                        void 差分セット( PMXFormat.材質モーフオフセット offset, PMX材質制御 材質 )
                        {
                            switch( offset.オフセット演算形式 )
                            {
                                case 0: // 乗算
                                    材質.乗算差分.拡散色 += offset.拡散色 * 現在値;
                                    材質.乗算差分.反射色 += offset.反射色 * 現在値;
                                    材質.乗算差分.反射強度 += offset.反射強度 * 現在値;
                                    材質.乗算差分.環境色 += offset.環境色 * 現在値;
                                    材質.乗算差分.エッジ色 += offset.エッジ色 * 現在値;
                                    材質.乗算差分.エッジサイズ += offset.エッジサイズ * 現在値;
                                    break;

                                case 1: // 加算
                                    材質.加算差分.拡散色 += offset.拡散色 * 現在値;
                                    材質.加算差分.反射色 += offset.反射色 * 現在値;
                                    材質.加算差分.反射強度 += offset.反射強度 * 現在値;
                                    材質.加算差分.環境色 += offset.環境色 * 現在値;
                                    材質.加算差分.エッジ色 += offset.エッジ色 * 現在値;
                                    材質.加算差分.エッジサイズ += offset.エッジサイズ * 現在値;
                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.グループ:
                    #region " グループモーフ "
                    //----------------
                    {
                        foreach( PMXFormat.グループモーフオフセット offset in 適用対象モーフ.PMXFモーフ.モーフオフセットリスト )
                        {
                            var メンバモーフ = PMXモデル.モーフリスト[ offset.モーフインデックス ];

                            if( メンバモーフ.PMXFモーフ.モーフ種類 == PMXFormat.モーフ種別.グループ )
                                throw new InvalidOperationException( "グループモーフのグループとしてグループモーフが指定されています。" );

                            this._モーフを適用する( 現在値 * offset.影響度, PMXモデル, メンバモーフ );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.モーフ種別.フリップ:
                    // todo: フリップモーフの実装
                    break;

                case PMXFormat.モーフ種別.インパルス:
                    // todo: インパルスモーフの実装
                    break;
            }
        }
    }
}
