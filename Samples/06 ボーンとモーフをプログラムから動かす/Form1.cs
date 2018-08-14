using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMF;
using MMF.コントロール.Forms;
using MMF.モデル.PMX;

namespace _06_MoveBoneAndMorphFromCode
{
	public partial class Form1 : RenderForm
	{
		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
				PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
				ScreenContext.ワールド空間.Drawableを追加する( model );
				
				//②ボーン、モーフの編集用に作成したサンプルのGUIを表示する
				TransformController controller = new TransformController( model );
				controller.Show( this );
                controller.Activate();
			}

            Activate();
		}
	}
}
