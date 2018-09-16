using System;
using System.Collections.Generic;

namespace MikuMikuFlex
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

        void 描画する( オブジェクト用エフェクト管理 EffectManager );
	}
}
