using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;

namespace MikuMikuFlex.コントロール.Forms
{
	/// <summary>
	///     レンダリング対象のフォームのベースになるクラス
	/// </summary>
	public partial class RenderForm : Form
    {
        public ScreenContext ScreenContext { get; private set; }

        public FPSCounter FpsCounter { get; private set; }

        public Vector3 レンダーターゲットのクリア色 { get; set; }

        /// <summary>
        ///     MessagePump.RunによってRenderメソッドを呼ぶことができない場合、trueにするとフォームの中でループを実行する
        /// </summary>
        public bool ペイントループによる描画を実行する { get; set; }


        /// <summary>
        ///     Initializes a new instance of the <see cref="RenderForm" /> class.
        /// </summary>
        public RenderForm()
		{
			InitializeComponent();

			レンダーターゲットのクリア色 = new Vector3( 0.2f, 0.4f, 0.8f );
		}

		/// <summary>
		///     デバイスの作成をカスタマイズしたい場合
		/// </summary>
		/// <param name="deviceManager"></param>
		public RenderForm( DeviceManager deviceManager )
            : this()
		{
            RenderContext.インスタンスを生成する( deviceManager );
		}


		/// <summary>
		///     <see cref="E:System.Windows.Forms.Form.Load" /> イベントを発生させます。
		/// </summary>
		/// <param name="e">イベント データを格納している <see cref="T:System.EventArgs" />。</param>
		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			#region デザインモード時処理

			if( DesignMode )
			{
				var label = new Label();
				label.Text = "RenderForm\n*デザインモードでは描画できません。\n*ウィンドウの大きさ、タイトルなどはデザインビューからも変更可能です。";
				label.Dock = DockStyle.Fill;
				label.TextAlign = ContentAlignment.MiddleCenter;
				label.Font = new Font( "Meiriyo", 30 );
				Controls.Add( label );
				return;
			}

			#endregion

			if( RenderContext.Instance == null )
			{
                // コンストラクタで RenderContext が指定されなかった場合は作成する
                RenderContext.インスタンスを生成する();
				ScreenContext = RenderContext.Instance.Initialize( this );
			}
			else
			{
                // 指定された場合
				ScreenContext = RenderContext.Instance.ScreenContextを作成する( this );
			}

			FpsCounter = new FPSCounter();
			FpsCounter.カウントを開始する();

			ClientSizeChanged += RenderForm_ClientSizeChanged;

			_初期化済み = true;
		}

		/// <summary>
		///     リサイズされた。
		/// </summary>
		private void RenderForm_ClientSizeChanged( object sender, EventArgs e )
		{
			if( ScreenContext != null && RenderContext.Instance.DeviceManager != null )
			{
				ScreenContext.スワップチェーンをリサイズする();
				ScreenContext.行列管理.射影行列管理.アスペクト比 = (float) Width / Height;
			}
		}

		public virtual void OnUpdated()
		{
		}

		/// <summary>
		///     コントロールの背景を描画します。
		/// </summary>
		/// <param name="e">イベント データを格納している <see cref="T:System.Windows.Forms.PaintEventArgs" />。</param>
		protected override void OnPaintBackground( PaintEventArgs e )
		{
			if( DesignMode ) base.OnPaintBackground( e );
		}

		/// <summary>
		///     DoOnPaintLoop=trueのとき用
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
		protected override void OnPaint( PaintEventArgs e )
		{
			if( RenderContext.Instance != null && ペイントループによる描画を実行する && !DesignMode )
			{
				Render();
				Invalidate();
			}
		}

		protected virtual void 画面をクリアする()
		{
			RenderContext.Instance.画面をクリアする( new Color4( レンダーターゲットのクリア色 ) );
		}

		/// <summary>
		///     レンダリング
		/// </summary>
		public virtual void Render()
		{
			if( !_初期化済み || !Visible )
                return;

            RenderContext.Instance.描画対象にする( ScreenContext );

            ScreenContext.RenderContextにマウス監視を登録する();

            // 進行

            ScreenContext.カメラを移動する();

            // 一定時間が経過していれば、すべてのワールド座標の進行を行う。（経過していないなら何もしない。）
            RenderContext.Instance.Timer.一定時間が経過していればActionを行う( () => {
                RenderContext.Instance.ワールド座標をすべて更新する( ScreenContext );
            } );

            FpsCounter.フレームを進める();

            // 描画

            画面をクリアする();

            ScreenContext.ワールド空間.登録されているすべての描画の必要があるものを描画する();

            ScreenContext.SwapChain.Present( 0, PresentFlags.None );    // Present

            OnPresented();
		}

		protected virtual void OnPresented()
		{
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );
		}

		protected override void OnHandleDestroyed( EventArgs e )
		{
			base.OnHandleDestroyed( e );

            RenderContext.Instance.ControltoScreenContextマップ.Remove( this );

            ScreenContext.Dispose();

            if( RenderContext.Instance.ControltoScreenContextマップ.Count == 0 )
                RenderContext.Instance.Dispose();
		}

        private bool _初期化済み;
    }
}
