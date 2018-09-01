using System;
using System.Collections.Generic;
using MikuMikuFlex.エフェクト;

namespace MikuMikuFlex.モデル
{
	/// <summary>
	///     レンダリングの方法を引き受けるインターフェース
	/// </summary>
	public interface サブセット管理 : IDisposable
	{
        int サブセットリストの要素数 { get; }

        List<サブセット> サブセットリスト { get; }

        
        /// <summary>
        ///     サブセットマネージャを初期化し、サブセット分割を実行します。
        /// </summary>
        void 初期化する( トゥーンテクスチャ管理 ToonManager, サブリソースローダー subresourceManager );

        void すべてを描画する( オブジェクト用エフェクト管理 EffectManager );

        void エッジを描画する( オブジェクト用エフェクト管理 EffectManager );

        void 地面影を描画する( オブジェクト用エフェクト管理 EffectManager );
	}
}
