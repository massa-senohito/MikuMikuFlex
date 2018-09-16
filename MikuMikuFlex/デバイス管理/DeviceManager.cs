using System;

namespace MikuMikuFlex
{
	public interface DeviceManager : IDisposable
	{
		/// <summary>
		///		D3D11 デバイス。
		/// </summary>
		SharpDX.Direct3D11.Device D3DDevice { get; }

		/// <summary>
		///		D3D デバイスの機能レベル。
		/// </summary>
		SharpDX.Direct3D.FeatureLevel DeviceFeatureLevel { get; }

		/// <summary>
		///		D3D11デイバスコンテキスト。
		/// </summary>
		SharpDX.Direct3D11.DeviceContext D3DDeviceContext { get; }

		/// <summary>
		///		D3D11 デバイスと D3D10 デバイスを保有しているアダプタ。
		/// </summary>
		/// <remarks>
		///		テクスチャ共有を行うためには、D3D11とD3D10は同じアダプタから生成されている必要がある。
		/// </remarks>
		SharpDX.DXGI.Adapter Adapter { get; }

		/// <summary>
		///		アダプタを保有する DXGI ファクトリ。
		/// </summary>
		SharpDX.DXGI.Factory2 DXGIFactory { get; set; }


        /// <summary>
        ///		読み込む。
        /// </summary>
        void Load();
	}
}
