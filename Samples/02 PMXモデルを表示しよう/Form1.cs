using System;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.コントロール.Forms;
using MikuMikuFlex.モデル.PMX;

namespace _02_SimpleRenderPMX
{
	public partial class Form1 : RenderForm
	{
		public Form1()
		{
			InitializeComponent();
		}

		//①Form.OnLoadをオーバーライドする。
		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e ); //RenderFormはOnLoad内で3DCG空間を初期化しているため、base.OnLoadがOnLoad内で一番初めに呼ぶべきである。

			//ファイルを開くダイアログ
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
				// ダイアログの返値がOKの場合、モデルの読み込み処理をする

				// (1) モデルを読み込む
				PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );


				// (2) ワールド空間にモデルを追加する
				ScreenContext.ワールド空間.Drawableを追加する( model );
				//WorldSpaceは、このフォームの描画する3D空間を示している。ここにモデルなど(IDrawableを実装している)ものを渡すと、描画してくれる。
				//WorldSpaceは、ScreenContext.WorldSpaceと常に等しい。ウィンドウごとに必要な3DCG描画に必要な情報はScreenContextに保管されている。
			}

            Activate();
		}
	}
}
