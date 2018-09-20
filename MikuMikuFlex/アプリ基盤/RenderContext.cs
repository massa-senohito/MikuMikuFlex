using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex
{
	/// <summary>
	///     レンダリングに関する様々な情報を保持します。
	///     3DCG描画をする際はこの参照が必要になることが多いです
	/// </summary>
	public class RenderContext : IDisposable
	{
        // シングルトン

        public static RenderContext Instance
        {
            get;
            protected set;
        } = null;

        public static void インスタンスを生成する()
        {
            Instance = new RenderContext();
            Instance.Initialize();
        }
        public static void インスタンスを生成する( DeviceManager deviceManager )
        {
            Instance = new RenderContext();
            Instance.Initialize( deviceManager );
        }
        public static void インスタンスを解放する()
        {
            Instance?.Dispose();
            Instance = null;
        }


        // イベント

        public event EventHandler 更新通知 = delegate { };


        // プロパティ

        public DeviceManager DeviceManager { get; private set; }

        /// <summary>
        ///     一定時間ごとに <see cref="ワールド座標をすべて更新する"/> を呼び出すタイマ。
        /// </summary>
        public モーションタイマ Timer;

        /// <summary>
        ///     レンダーターゲットのクリア色。
        ///     スクリプトの Clear 関数で使われる。
        /// </summary>
		public Color4 クリア色 { get; set; }

        /// <summary>
        ///     深度ステンシルのクリア色。
        ///     スクリプトの Clear 関数で使われる。
        /// </summary>
		public float クリア深度 { get; set; }

        /// <summary>
        ///     マウスの挙動をリアルタイム取得。
        ///		ScreenContextがターゲットになっていないとnullになる。
        /// </summary>
        public マウス監視 パネル監視;

        /// <summary>
        ///     コントロールとScreenContestのマップ。
        /// </summary>
        public Dictionary<Control, ScreenContext> ControltoScreenContextマップ { get; } = new Dictionary<Control, ScreenContext>();


        // 描画先プロパティ

        /// <summary>
        ///     描画先のリソース一式。
        ///     レンダーターゲット、深度ステンシル、行列、カメラ、ビューポート、スワップチェーンなど。
        /// </summary>
		public TargetContext 描画ターゲットコンテキスト  { get; private set; }

        public RenderTargetView[] レンダーターゲット配列 = new RenderTargetView[ 8 ];

        public DepthStencilView 深度ステンシルターゲット;


        // 描画ステートプロパティ

        public 照明行列管理 照明行列管理;

        public 行列管理 行列管理 => 描画ターゲットコンテキスト.行列管理;

        /// <summary>
        ///     任意の DeviceContext に BlendState を設定できる。
        /// </summary>
		public ブレンドステート管理 ブレンドステート管理 { get; private set; }

        public RasterizerState 片面描画の際のラスタライザステート { get; private set; }

        public RasterizerState 両面描画の際のラスタライザステート { get; private set; }

        public RasterizerState 片面描画の際のラスタライザステートLine { get; private set; }

        public RasterizerState 両面描画の際のラスタライザステートLine { get; private set; }

        public RasterizerState 裏側片面描画の際のラスタライザステート { get; private set; }


        // メソッド

        protected RenderContext()
		{
            if( null != Instance )
                throw new Exception( "インスタンスはすでに生成済みです。" );
        }

        public void Initialize()
		{
			this._デバイスを初期化する();

            this.Timer = new モーションタイマ();

            this.ブレンドステート管理 = new ブレンドステート管理( this );
            this.ブレンドステート管理.ブレンドステートを設定する( (SharpDX.Direct3D11.DeviceContext)this.DeviceManager.D3DDeviceContext, ブレンドステート管理.BlendStates.Alignment );

        }

        public void Initialize( DeviceManager deviceManager )
        {
            this.DeviceManager = deviceManager;
            this.Initialize();
        }

        public ScreenContext Initialize( Control targetControl )
		{
            this.Initialize();

			var matrixManager = this._行列を初期化する();
			var primaryContext = new ScreenContext( targetControl, matrixManager );
			this.ControltoScreenContextマップ.Add( targetControl, primaryContext );

            this.描画ターゲットコンテキスト = primaryContext;
            this._レンダーターゲットを更新する();

			return primaryContext;
		}

        public void Dispose()
        {
            foreach( var screenContext in this.ControltoScreenContextマップ )
                screenContext.Value.Dispose();

            this.裏側片面描画の際のラスタライザステート?.Dispose();
            this.裏側片面描画の際のラスタライザステート = null;

            this.片面描画の際のラスタライザステートLine?.Dispose();
            this.片面描画の際のラスタライザステートLine = null;

            this.両面描画の際のラスタライザステートLine?.Dispose();
            this.両面描画の際のラスタライザステートLine = null;

            this.片面描画の際のラスタライザステート?.Dispose();
            this.片面描画の際のラスタライザステート = null;

            this.両面描画の際のラスタライザステート?.Dispose();
            this.両面描画の際のラスタライザステート = null;

            foreach( var disposable in this.Disposables )
                disposable.Dispose();

            if( this._DeviceManagerの破棄をこのインスタンスで行う )
                DeviceManager.Dispose();
        }

        public void 画面をクリアする( Color4 color )
		{
			this._レンダーターゲットを更新する();

			this.DeviceManager.D3DDeviceContext.ClearRenderTargetView( this.描画ターゲットコンテキスト.D3Dレンダーターゲットビュー, color );
			this.DeviceManager.D3DDeviceContext.ClearDepthStencilView( this.描画ターゲットコンテキスト.深度ステンシルビュー, DepthStencilClearFlags.Depth, 1, 0 );
		}

		public void ワールド座標をすべて更新する( ScreenContext screen )
        {
            screen.ワールド空間.すべてのDynamicTextureを更新する();
            screen.ワールド空間.すべてのMovableを更新する();

			// 自身も更新。
			this.更新通知( this, new EventArgs() );
		}

		public void 描画対象にする( TargetContext context )
		{
			this.描画ターゲットコンテキスト = context;
			context.ビューポートを設定する();

            _レンダーターゲットを更新する();
		}

		public ScreenContext ScreenContextを作成する( Control control )
		{
			var カメラ = new カメラ(
				カメラの初期位置: new Vector3( 0, 20, -40 ),
				カメラの初期注視点: new Vector3( 0, 3, 0 ), 
				カメラの初期上方向ベクトル: new Vector3( 0, 1, 0 ) );

			var 射影行列 = new 射影();
			射影行列.射影行列を初期化する( (float) Math.PI / 4f, 1.618f, 1, 200 );

			var matrixManager = new 行列管理( new ワールド行列(), カメラ, 射影行列 );

			var context = new ScreenContext( control, matrixManager );
			this.ControltoScreenContextマップ.Add( control, context );

			return context;
		}


		internal List<IDisposable> Disposables = new List<IDisposable>();

        private bool _DeviceManagerの破棄をこのインスタンスで行う = false;


		private 行列管理 _行列を初期化する()
		{
            var 行列管理 = new 行列管理( 
                new ワールド行列(),
                new カメラ(
                    カメラの初期位置: new Vector3( 0f, 20f, -40f ),
                    カメラの初期注視点: new Vector3( 0f, 3f, 0f ),
                    カメラの初期上方向ベクトル: new Vector3( 0f, 1f, 0f ) ),
                new 射影()
                    .射影行列を初期化する( (float) Math.PI / 4f, 1.618f, 1, 2000 ) );

            this.照明行列管理 = new 照明行列管理( 行列管理 );
			return 行列管理;
		}

		private void _デバイスを初期化する()
		{
			this._行列を初期化する();

			// 未生成時の（コンストラクタで指定していない）ときのみ DeviceManager を生成する。
			if( this.DeviceManager == null )
			{
				this._DeviceManagerの破棄をこのインスタンスで行う = true;	// 自分が生成したので自分で破棄することを覚えておく。

				this.DeviceManager = new DeviceManager既定実装();
				this.DeviceManager.Load();
			}

            this.片面描画の際のラスタライザステート = new RasterizerState( DeviceManager.D3DDevice, new RasterizerStateDescription {
                CullMode = SharpDX.Direct3D11.CullMode.Back,
                FillMode = SharpDX.Direct3D11.FillMode.Solid,
            } );

            this.両面描画の際のラスタライザステート = new RasterizerState( DeviceManager.D3DDevice, new RasterizerStateDescription {
                CullMode = SharpDX.Direct3D11.CullMode.None,
                FillMode = SharpDX.Direct3D11.FillMode.Solid,
            } );

            this.片面描画の際のラスタライザステートLine = new RasterizerState( DeviceManager.D3DDevice, new RasterizerStateDescription {
                CullMode = SharpDX.Direct3D11.CullMode.Back,
                FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
            } );

            this.両面描画の際のラスタライザステートLine = new RasterizerState( DeviceManager.D3DDevice, new RasterizerStateDescription {
                CullMode = SharpDX.Direct3D11.CullMode.None,
                FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
            } );

            this.裏側片面描画の際のラスタライザステート = new RasterizerState( DeviceManager.D3DDevice, new RasterizerStateDescription {
                CullMode = SharpDX.Direct3D11.CullMode.Front,
                FillMode = SharpDX.Direct3D11.FillMode.Solid,
            } );
        }

        private void _レンダーターゲットを更新する()
		{
			this.レンダーターゲット配列[ 0 ] = this.描画ターゲットコンテキスト.D3Dレンダーターゲットビュー;
			this.深度ステンシルターゲット = this.描画ターゲットコンテキスト.深度ステンシルビュー;

			this.DeviceManager.D3DDeviceContext.OutputMerger.SetTargets(
				this.深度ステンシルターゲット,
				this.レンダーターゲット配列 );
		}
	}
}