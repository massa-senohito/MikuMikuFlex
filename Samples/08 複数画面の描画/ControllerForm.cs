using System;
using System.Windows.Forms;
using MikuMikuFlex;

namespace _08_MultiScreenRendering
{
	public partial class ControllerForm : Form
	{
		private readonly Form1 form1;
		private readonly ChildForm childForm;

		public ControllerForm( Form1 form1, ChildForm childForm )
		{
			this.form1 = form1;
			this.childForm = childForm;
			InitializeComponent();
		}

		#region ②-A ボタンに応じたワールド空間に対してモデルを追加する

		private void Add2ChildForm_Click( object sender, EventArgs e )
		{
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
                PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
				childForm.ScreenContext.ワールド空間.Drawableを追加する( model );
			}
		}

		private void Add2Form1_Click( object sender, EventArgs e )
		{
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
				PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
				form1.ScreenContext.ワールド空間.Drawableを追加する( model );
			}
		}

		#endregion

	}
}
