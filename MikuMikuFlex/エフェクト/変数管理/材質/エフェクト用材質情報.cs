using System;
using System.IO;
using MMDFileParser.PMXModelParser;
using MikuMikuFlex.モデル;
using MikuMikuFlex.Utility;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.材質
{
    public class エフェクト用材質情報 : IDisposable
    {
        public String 材質名;
        public String 材質名_英;

        // TODO: 対応しているマテリアルが完全ではない

        public Vector4 環境色; // モーフ対応
        public Vector4 拡散色; // モーフ対応
        public Vector4 反射色; // モーフ対応
        public float 反射係数;  // モーフ対応
        public Vector4 エッジ色;    // モーフ対応
        public Vector4 放射色;
        public Vector4 地面影色;
        public bool 加算合成モードである;
        public bool サブテクスチャを使用する;
        public bool トゥーンを使用する;
        public bool セルフ影を使用する;
        public bool Line描画を使用する;
        public ShaderResourceView スフィアマップ;
        public ShaderResourceView テクスチャ;
        public ShaderResourceView トゥーンテクスチャ;
        public スフィアモード スフィアモード;
        public Vector4 スフィア加算値;
        public Vector4 スフィア乗算値;
        public Vector4 テクスチャ加算値;
        public Vector4 テクスチャ乗算値;
        public Vector4 トゥーン色;
        public float エッジ幅;  // モーフ対応
        public bool エッジが有効である;
        public bool 地面影が有効である;

        /// <summary>
        ///     材質モーフ用
        /// </summary>
        public エフェクト用材質情報 乗算差分;

        /// <summary>
        ///     材質モーフ用
        /// </summary>
        public エフェクト用材質情報 加算差分;

        /// <summary>
        ///     材質モーフ用
        /// </summary>
        public エフェクト用材質情報 初期値;

 
        public static エフェクト用材質情報 作成する( IDrawable drawable, MMDFileParser.PMXModelParser.材質 material )
        {
            var info = new エフェクト用材質情報() {
                材質名 = material.材質名,
                材質名_英 = material.材質名_英,
                環境色 = new Vector4( material.環境色, 1f ),
                拡散色 = material.拡散色,
                エッジ色 = material.エッジ色,
                放射色 = new Vector4( 1f ),
                地面影色 = drawable.地面影色,
                反射色 = new Vector4( material.反射色, 1f ),
                反射係数 = material.反射強度,
                トゥーン色 = new Vector4( 0f ),
                エッジ幅 = material.エッジサイズ,
                エッジが有効である = material.描画フラグ.HasFlag( 描画フラグ.エッジ ),
                地面影が有効である = material.描画フラグ.HasFlag( 描画フラグ.地面影 ),
                Line描画を使用する = material.描画フラグ.HasFlag( 描画フラグ.Line描画 ),
                乗算差分 = new エフェクト用材質情報(),
                加算差分 = new エフェクト用材質情報(),
            };

            info.初期値 = new エフェクト用材質情報() {
                環境色 = new Vector4( material.環境色, 1f ),
                拡散色 = material.拡散色,
                エッジ色 = material.エッジ色,
                放射色 = new Vector4( 1f ),
                地面影色 = drawable.地面影色,
                反射色 = new Vector4( material.反射色, 1f ),
                反射係数 = material.反射強度,
                トゥーン色 = new Vector4( 0f ),
                エッジ幅 = material.エッジサイズ,
            };

            info._リセットする();

            return info;
        }


        // プライベートコンストラクタ
        private エフェクト用材質情報()
        {
        }

        public void Dispose()
        {
            乗算差分?.Dispose();
            乗算差分 = null;

            加算差分?.Dispose();
            加算差分 = null;

            初期値?.Dispose();
            初期値 = null;

            テクスチャ?.Dispose();
            テクスチャ = null;

            スフィアマップ?.Dispose();
            スフィアマップ = null;

            トゥーンテクスチャ?.Dispose();
            トゥーンテクスチャ = null;
        }

        /// <summary>
        ///     現在の材質情報を更新する。
        /// </summary>
        /// <remarks>
        ///     外部で <see cref="乗算差分"/> と <see cref="加算差分"/> が更新されているという前提で、それを反映する。
        ///     上記２つは反映後にリセットされて、また外部からの更新を待つ。
        /// </remarks>
        public void 更新する()
        {
            // 値 ＝ 初期値 × 乗算値 ＋ 加算値

            環境色 = CGHelper.MulEachMember( 初期値.環境色, 乗算差分.環境色 ) + 加算差分.環境色;
            拡散色 = CGHelper.MulEachMember( 初期値.拡散色, 乗算差分.拡散色 ) + 加算差分.拡散色;
            反射色 = CGHelper.MulEachMember( 初期値.反射色, 乗算差分.反射色 ) + 加算差分.反射色;
            反射係数 = 初期値.反射係数 * 乗算差分.反射係数 + 加算差分.反射係数;

            if( エッジが有効である )
            {
                エッジ色 = CGHelper.MulEachMember( 初期値.エッジ色, 乗算差分.エッジ色 ) + 加算差分.エッジ色;
                エッジ幅 = 初期値.エッジ幅 * 乗算差分.エッジ幅 + 加算差分.エッジ幅;
            }

            _リセットする();
        }


        private void _リセットする()
        {
            乗算差分.環境色 = new Vector4( 1f );
            乗算差分.拡散色 = new Vector4( 1f );
            乗算差分.反射色 = new Vector4( 1f );
            乗算差分.反射係数 = 1f;
            乗算差分.エッジ色 = new Vector4( 1f, 1f, 1f, 1f );
            乗算差分.エッジ幅 = 1f;

            加算差分.環境色 = new Vector4( 0f );
            加算差分.拡散色 = new Vector4( 0f );
            加算差分.反射色 = new Vector4( 0f );
            加算差分.反射係数 = 0f;
            加算差分.エッジ色 = new Vector4( 0f );
            加算差分.エッジ幅 = 0f;
        }
    }
}
