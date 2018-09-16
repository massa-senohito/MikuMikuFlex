using System;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.モデル.PMX;

namespace _09_Render3DCGToUserControl
{

	//②-A フォームデザイナを利用して画面をレイアウトする
	public partial class Form1 : Form
	{
		private PMXModel model;

		public Form1()
		{
			InitializeComponent();
		}

		/*
         * ②-B 利用しているRenderControlのInitializeメソッドを呼び出す
         */
		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );
			renderControl1.Initialize();
		}

		/*
         * ②-C レンダリング時にMessagePumpで呼び出すためのメソッドを定義しておく
         */
		public void Render()
		{
			//使用しているコントロールのRenderメソッドを呼び出すことでコントロールは描画を実行する。
			renderControl1.Render();
		}

        protected override void OnClosed( EventArgs e )
        {
            model?.Dispose();
            model = null;

            base.OnClosed( e );
        }

        #region ②-D ボタンに応じて処理をする。今までのサンプルとそこまで内容は変わらない

        private void loadMotion_Click( object sender, EventArgs e )
		{
			if( model == null ) return;

			var ofd = new OpenFileDialog();
			ofd.Filter = "vmdモデルファイル(*.vmd)|*.vmd";
			if( ofd.ShowDialog( this ) == DialogResult.OK )
			{
				モーション motion = model.モーション管理.ファイルからモーションを生成し追加する( ofd.FileName, false );
				model.モーション管理.モーションを適用する( motion );
			}
		}

		private void loadModel_Click( object sender, EventArgs e )
		{
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog( this ) == DialogResult.OK )
			{
				model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
				renderControl1.ScreenContext.ワールド空間.Drawableを追加する( model );
			}
		}

		#endregion
	}
}
