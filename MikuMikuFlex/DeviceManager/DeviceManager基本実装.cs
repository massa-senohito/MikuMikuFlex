using System;
using System.Diagnostics;
using MikuMikuFlex.エフェクト;

namespace MikuMikuFlex.DeviceManager
{
	/// <summary>
	///     IDeviceManager の基礎クラス。
	/// </summary>
	public class DeviceManager基本実装 : IDeviceManager
	{
		// IDeviceManagerの実装

		/// <summary>
		///		D3D11 デバイス。
		/// </summary>
		public SharpDX.Direct3D11.Device D3DDevice { get; private set; }

		/// <summary>
		///		D3D10 デバイス。D2D,DW用。
		/// </summary>
		public SharpDX.Direct3D10.Device D3DDevice10 { get; private set; }

		/// <summary>
		///		D3D デバイスの機能レベル。
		/// </summary>
		public SharpDX.Direct3D.FeatureLevel DeviceFeatureLevel { get; private set; }

		/// <summary>
		///		D3D11デイバスコンテキスト。
		/// </summary>
		public SharpDX.Direct3D11.DeviceContext D3DDeviceContext { get; private set; }

		/// <summary>
		///		D3D11 デバイスと D3D10 デバイスを保有しているアダプタ。
		/// </summary>
		/// <remarks>
		///		テクスチャ共有を行うためには、D3D11とD3D10は同じアダプタから生成されている必要がある。
		/// </remarks>
		public SharpDX.DXGI.Adapter Adapter { get; private set; }

		/// <summary>
		///		アダプタを保有する DXGI ファクトリ。
		/// </summary>
		public SharpDX.DXGI.Factory2 DXGIFactory { get; set; }

        public SharpDX.Direct2D1.Factory2 D2DFactory { get; private set; }

        public SharpDX.DirectWrite.Factory DWFactory { get; private set; }


        /// <summary>
        ///		読み込む。
        /// </summary>
        public void Load()
		{
			this.Load(
				機能レベル11が必須: false,
				dx11flag: SharpDX.Direct3D11.DeviceCreationFlags.None,
				dx10flag_for2DDraw: SharpDX.Direct3D10.DeviceCreationFlags.BgraSupport );
		}

		// 以下、追加分

		/// <summary>
		///     初期化する。
		/// </summary>
		/// <param name="control">
		///		デバイスコンテキストを適用するコントロール。
		///	</param>
		public void Load( bool 機能レベル11が必須 = false, SharpDX.Direct3D11.DeviceCreationFlags dx11flag = SharpDX.Direct3D11.DeviceCreationFlags.None, SharpDX.Direct3D10.DeviceCreationFlags dx10flag_for2DDraw = SharpDX.Direct3D10.DeviceCreationFlags.BgraSupport )
		{
			this._ApplyDebugFlags( ref dx11flag, ref dx10flag_for2DDraw );

			this.DXGIFactory = new SharpDX.DXGI.Factory2();

			this.Adapter = DXGIFactory.GetAdapter1( 0 );

			#region " D3D11 デバイスを作成する。（機能レベルは 11.0 または 10.1） "
			//----------------
			try
			{
				// 機能レベル 11.x の D3D11 デバイスを作成する。
				D3DDevice = new SharpDX.Direct3D11.Device( 
					Adapter, 
					dx11flag,
					new[] {
						SharpDX.Direct3D.FeatureLevel.Level_11_0
					} );
			}
			catch( SharpDX.SharpDXException )
			{
				if( 機能レベル11が必須 )
					throw new NotSupportedException( "DX11がサポートされていません。DX10.1で初期化するにはLoadの第一引数をfalseにして下さい。" );

				try
				{
					// 機能レベル 10.x の D3D11 デバイスを作成する。
					D3DDevice = new SharpDX.Direct3D11.Device(
						Adapter,
						dx11flag, 
						new[] {
							SharpDX.Direct3D.FeatureLevel.Level_10_0
						} );
				}
				catch( SharpDX.SharpDXException )
				{
					throw new NotSupportedException( "DX11,DX10.1での初期化を試みましたが、両方ともサポートされていません。" );
				}
			}
			//----------------
			#endregion

			this.DeviceFeatureLevel = D3DDevice.FeatureLevel;

			this.D3DDeviceContext = D3DDevice.ImmediateContext; // 注: COM参照カウンタが増える。

			#region " D3D10 デバイスを作成する。（機能レベルは 9.3）"
			//----------------
#if VSG_DEBUG
#else
			// 機能レベル 9.3 の DX10 デバイスを作成する。
			this.D3DDevice10 = new SharpDX.Direct3D10.Device1(
				Adapter,
				//DriverType.Hardware,	--> SharpDX では、Adapter か DriverType のいずれかしか指定できない。
				dx10flag_for2DDraw,
				SharpDX.Direct3D10.FeatureLevel.Level_9_3 );
#endif
            //----------------
            #endregion

            this.DWFactory = new SharpDX.DirectWrite.Factory( SharpDX.DirectWrite.FactoryType.Isolated );

            this.D2DFactory = new SharpDX.Direct2D1.Factory2( SharpDX.Direct2D1.FactoryType.MultiThreaded, SharpDX.Direct2D1.DebugLevel.Information );

            // エフェクトマネージャを初期化する。
            エフェクト.エフェクト.初期化する( this );
		}

		/// <summary>
		///		終了する。
		/// </summary>
		public virtual void Dispose()
		{
			if( !D3DDeviceContext.IsDisposed && D3DDeviceContext.Rasterizer.State != null && !D3DDeviceContext.Rasterizer.State.IsDisposed )
				D3DDeviceContext.Rasterizer.State.Dispose();

			if( D3DDevice != null && !D3DDevice.IsDisposed ) D3DDevice.Dispose();

			if( D3DDevice10 != null && !D3DDevice10.IsDisposed ) D3DDevice10.Dispose();

			if( Adapter != null && !Adapter.IsDisposed ) Adapter.Dispose();

			if( DXGIFactory != null && !DXGIFactory.IsDisposed ) DXGIFactory.Dispose();

            if( DWFactory != null && !DWFactory.IsDisposed ) DWFactory.Dispose();

            if( D2DFactory != null && !D2DFactory.IsDisposed ) D2DFactory.Dispose();
        }

        [Conditional( "DEBUG" )]	// DEBUG 時のみ実行する。
		private void _ApplyDebugFlags( ref SharpDX.Direct3D11.DeviceCreationFlags dx11flag, ref SharpDX.Direct3D10.DeviceCreationFlags dx10flag_for2DDraw )
		{
			Debug.Print( "デバイスはデバッグモードで作成されました。" );

			//dx11flag = dx11flag | DeviceCreationFlags.Debug;
			dx10flag_for2DDraw = dx10flag_for2DDraw | SharpDX.Direct3D10.DeviceCreationFlags.BgraSupport;
		}
	}
}
