using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.モデル.PMX;

namespace _04_TransformModelFormCode
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

			// モデルファイルを開く。（必須）
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
				this._Model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );

				// モーションファイルを開く。（任意）
				var ofd2 = new OpenFileDialog();
				ofd2.Filter = "vmdモーションファイル(*.vmd)|*.vmd";
				if( ofd2.ShowDialog() == DialogResult.OK )
				{
					モーション motion = this._Model.モーション管理.ファイルからモーションを生成し追加する( ofd2.FileName, true );
					this._Model.モーション管理.モーションを適用する( motion, 0, モーション再生終了後の挙動.Replay );
				}

				this.ScreenContext.ワールド空間.Drawableを追加する( this._Model );
				
				//②コントローラーフォームに対して読み込んだモデルを渡して表示します。
				this._Controller = new Controller( this._Model );
				this._Controller.Show();
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

        private Controller _Controller;
    }
}
