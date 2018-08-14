using System;
using MMF.行列;
using SharpDX.Direct3D11;

namespace MMF.DeviceManager
{
    /// <summary>
    ///     描画先のリソース一式。
    ///     レンダーターゲット、深度ステンシル、行列、カメラ、ビューポート、スワップチェーンなど。
    /// </summary>
    public interface ITargetContext : IDisposable
	{
		RenderTargetView D3Dレンダーターゲットビュー { get; }

		DepthStencilView 深度ステンシルビュー { get; }

		行列管理 行列管理 { get; set; }

		カメラモーション カメラモーション { get; set; }

		ワールド空間 ワールド空間 { get; set; }

		/// <summary>
		///     MME特殊パラメータ
		///     object_ssの時に取得するセルフシャドウモード
		///     bool parthfにあたる
		/// </summary>
		bool IsSelfShadowMode1 { get; }

		/// <summary>
		///     MME特殊パラメータ
		///     bool transpにあたる値
		/// </summary>
		bool IsEnabledTransparent { get; }

		void ビューポートを設定する();
	}
}
