using System;
using System.Collections.Generic;
using MMF.エフェクト;

namespace MMF.モデル
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
        /// <param name="context">デバイス</param>
        /// <param name="effect"></param>
        /// <param name="subresourceManager"></param>
        /// <param name="ToonManager"></param>
        void 初期化する( トゥーンテクスチャ管理 ToonManager, サブリソースローダー subresourceManager );

		void すべてを描画する( エフェクト.エフェクト effect );

		void エッジを描画する( エフェクト.エフェクト effect );

		void 地面影を描画する( エフェクト.エフェクト effect );
	}
}
