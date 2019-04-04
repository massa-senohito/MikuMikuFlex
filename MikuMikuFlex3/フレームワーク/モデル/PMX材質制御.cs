using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.材質"/> に追加情報を付与するクラス。
    /// </summary>
    public class PMX材質制御 : IDisposable
    {
        public float テッセレーション係数 { get; set; }

        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 拡散色 => MulEachMember( this._PMXF材質.拡散色, this.乗算差分.拡散色 ) + 加算差分.拡散色;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 反射色 => MulEachMember( this._PMXF材質.反射色, this.乗算差分.反射色 ) + 加算差分.反射色;

        public float 反射強度 => this._PMXF材質.反射強度 * this.乗算差分.反射強度 + this.加算差分.反射強度;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 環境色 => MulEachMember( this._PMXF材質.環境色, this.乗算差分.環境色 ) + 加算差分.環境色;

        /// <summary>
        ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 エッジ色 => ( this.描画フラグ.HasFlag( PMXFormat.描画フラグ.エッジ ) ) ? MulEachMember( this._PMXF材質.エッジ色, this.乗算差分.エッジ色 ) + 加算差分.エッジ色 : new Vector4( 0f );

        /// <summary>
        ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
        ///     Point 描画時は Point サイズ(※2.1拡張)。
        /// </summary>
        public float エッジサイズ => ( this.描画フラグ.HasFlag( PMXFormat.描画フラグ.エッジ ) ) ? this._PMXF材質.エッジサイズ * this.乗算差分.エッジサイズ + 加算差分.エッジサイズ : 0f;


        public struct 差分
        {
            // 材質色

            /// <summary>
            ///     (R, G, B, A)
            /// </summary>
            public Vector4 拡散色 { get; set; }

            /// <summary>
            ///     (R, G, B)
            /// </summary>
            public Vector3 反射色 { get; set; }

            public float 反射強度 { get; set; }

            /// <summary>
            ///     (R, G, B)
            /// </summary>
            public Vector3 環境色 { get; set; }

            // 描画

            /// <summary>
            ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
            ///     (R, G, B, A)
            /// </summary>
            public Vector4 エッジ色 { get; set; }

            /// <summary>
            ///     <see cref="描画フラグ.エッジ"/> が指定されているときのみ有効。
            ///     Point 描画時は Point サイズ(※2.1拡張)。
            /// </summary>
            public float エッジサイズ { get; set; }
        }

        public 差分 加算差分;// { get; protected set; }

        public 差分 乗算差分;// { get; protected set; }


        // 描画

        public PMXFormat.描画フラグ 描画フラグ => this._PMXF材質.描画フラグ;



        // テクスチャ／メモ

        public int 通常テクスチャの参照インデックス => this._PMXF材質.通常テクスチャの参照インデックス;

        public int スフィアテクスチャの参照インデックス => this._PMXF材質.スフィアテクスチャの参照インデックス;

        public PMXFormat.スフィアモード スフィアモード => this._PMXF材質.スフィアモード;

        /// <summary>
        ///     0 or 1  。
        ///     <see cref="共有Toonのテクスチャ参照インデックス"/> のサマリを参照のこと。
        /// </summary>
        public byte 共有Toonフラグ => this._PMXF材質.共有Toonフラグ;

        /// <summary>
        ///     <see cref="共有Toonフラグ"/> が 0 の時は、Toonテクスチャテクスチャテーブルの参照インデックス。
        ///     <see cref="共有Toonフラグ"/> が 1 の時は、共有Toonテクスチャ[0~9]がそれぞれ toon01.bmp~toon10.bmp に対応。
        /// </summary>
        public int 共有Toonのテクスチャ参照インデックス => this._PMXF材質.共有Toonのテクスチャ参照インデックス;

        /// <summary>
        ///     自由欄／スクリプト記述／エフェクトへのパラメータ配置など
        /// </summary>
        public String メモ => this._PMXF材質.メモ;


        /// <summary>
        ///     材質に対応する面数（頂点数で示す）。
        ///     １面は３頂点なので、必ず３の倍数になる。
        /// </summary>
        public int 頂点数 => this._PMXF材質.頂点数;

        public int 開始インデックス => this._PMXF材質.開始インデックス;



        // 生成と終了


        public PMX材質制御( PMXFormat.材質 material )
        {
            this._PMXF材質 = material;
            this.状態をリセットする();
        }

        public virtual void Dispose()
        {
            this._PMXF材質 = null;
        }



        // 更新


        public void 状態をリセットする()
        {
            this.テッセレーション係数 = 1f;

            this.加算差分.拡散色 = Vector4.Zero;
            this.加算差分.反射色 = Vector3.Zero;
            this.加算差分.反射強度 = 0;
            this.加算差分.環境色 = Vector3.Zero;
            this.加算差分.エッジ色 = Vector4.Zero;
            this.加算差分.エッジサイズ = 0;

            this.乗算差分.拡散色 = new Vector4( 1f );
            this.乗算差分.反射色 = new Vector3( 1f );
            this.乗算差分.反射強度 = 1;
            this.乗算差分.環境色 = new Vector3( 1f );
            this.乗算差分.エッジ色 = new Vector4( 1f );
            this.乗算差分.エッジサイズ = 1;
        }



        // private


        private PMXFormat.材質 _PMXF材質;


        public static Vector4 MulEachMember( Vector4 vec1, Vector4 vec2 )
            => new Vector4( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z, vec1.W * vec2.W );

        public static Vector3 MulEachMember( Vector3 vec1, Vector3 vec2 )
            => new Vector3( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z );
    }
}
