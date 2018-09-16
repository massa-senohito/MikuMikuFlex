using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuFlex;

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
				this._Model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
				this.ScreenContext.ワールド空間.Drawableを追加する( this._Model );
				
				//②ボーン、モーフの編集用に作成したサンプルのGUIを表示する
				this._Controller = new TransformController( this._Model );
				this._Controller.Show( this );
                this._Controller.Activate();
			}

            Activate();
		}

        protected override void OnClosed( EventArgs e )
        {
            this._Controller?.Close();
            this._Controller = null;

            this._Model?.Dispose();
            this._Model = null;

            base.OnClosed( e );
        }

        private PMXModel _Model;

        private TransformController _Controller;
    }
}
