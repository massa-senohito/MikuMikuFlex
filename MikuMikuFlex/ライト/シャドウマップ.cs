using System;
using MikuMikuFlex.行列;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.ライト
{
	/// <summary>
	///     影を落とすことができるものにつけるインターフェース
	/// </summary>
	public interface シャドウマップ : IDisposable
	{
		行列管理 照明から見た行列 { get; }

		RenderTargetView レンダーターゲット { get; }

		DepthStencilView 深度ステンシルビュー { get; }

		Texture2D 深度テクスチャ { get; }

		ShaderResourceView テクスチャから利用した深度のリソース { get; }

		モデル状態 Transformer { get; }
	}
}
