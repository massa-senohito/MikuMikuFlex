using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;

namespace MikuMikuFlex
{
	/// <summary>
	///     レンダリングビューの基本クラス
	/// </summary>
	public class RenderControl : UserControl
	{
		public ScreenContext ScreenContext { get; private set; }

		/// <summary>
		///     バックグラウンドのカラー
		/// </summary>
		public Color3 BackgroundColor { get; set; }

		/// <summary>
		///     FPSカウンタ
		/// </summary>
		public FPSCounter FpsCounter { get; private set; }

		/// <summary>
		///     初期化したかどうかの判定
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		///     初期化処理
		/// </summary>
		/// <param name="context"></param>
		public virtual void Initialize()
		{
			if( RenderContext.Instance == null )
			{
                RenderContext.インスタンスを生成する();
				ScreenContext = RenderContext.Instance.Initialize( this );
			}
			else
			{
				ScreenContext = RenderContext.Instance.ScreenContextを作成する( this );
			}
			FpsCounter = new FPSCounter();
			FpsCounter.カウントを開始する();
			IsInitialized = true;
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			if( DesignMode )
			{
				var label = new Label();
				label.Dock = DockStyle.Fill;
				label.TextAlign = ContentAlignment.MiddleCenter;
				label.Text =
					string.Format(
						"{0}\n*デザインモード時は描画できません。\n*必ずFormのOnLoadでInitializeメソッドを呼び出してください。\n*別のコントロールで利用したRenderContextを利用する場合はInitializeの第一引数に与えてください",
						GetType() );
				Controls.Add( label );
			}
		}

		protected override void OnClientSizeChanged( EventArgs e )
		{
			base.OnClientSizeChanged( e );
			if( !DesignMode && ScreenContext != null )
			{
				ScreenContext.スワップチェーンをリサイズする();
				ScreenContext.行列管理.射影行列管理.アスペクト比 = (float) Width / Height;
			}
		}

		protected virtual void ClearViews()
		{
			RenderContext.Instance.画面をクリアする( new Color4( BackgroundColor ) );
		}

		/// <summary>
		///     レンダリング
		/// </summary>
		public virtual void Render()
		{
			if( !IsInitialized || !Visible ) return;
			RenderContext.Instance.描画対象にする( ScreenContext );
			ScreenContext.カメラを移動する();
            RenderContext.Instance.Timer.一定時間が経過していればActionを行う( () => {
                RenderContext.Instance.ワールド座標をすべて更新する( ScreenContext );
            } );
			FpsCounter.フレームを進める();
			ClearViews();
			ScreenContext.ワールド空間.登録されているすべての描画の必要があるものを描画する();
			ScreenContext.SwapChain.Present( 0, PresentFlags.None );
		}

		/// <summary>
		///     <see cref="T:System.Windows.Forms.Control" /> とその子コントロールが使用しているアンマネージ リソースを解放します。オプションで、マネージ リソースも解放します。
		/// </summary>
		/// <param name="disposing">マネージ リソースとアンマネージ リソースの両方を解放する場合は true。アンマネージ リソースだけを解放する場合は false。</param>
		protected override void Dispose( bool disposing )
		{
			base.Dispose( disposing );

            if( RenderContext.Instance != null )
            {
                RenderContext.Instance.ControltoScreenContextマップ.Remove( this );
            }

            ScreenContext?.Dispose();

            if( RenderContext.Instance != null &&
                RenderContext.Instance.ControltoScreenContextマップ.Count == 0 )
            {
                RenderContext.Instance.Dispose();
            }
		}
	}
}
