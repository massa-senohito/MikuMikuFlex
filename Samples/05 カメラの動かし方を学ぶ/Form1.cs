using System;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.モデル.PMX;

namespace _05_HowToUpdateCamera
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

            // 必須
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
                this._Model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );

                // 任意
				var ofd2 = new OpenFileDialog();
				ofd2.Filter = "vmdモーションファイル(*.vmd)|*.vmd";
				if( ofd2.ShowDialog() == DialogResult.OK )
				{
					モーション motion = this._Model.モーション管理.ファイルからモーションを生成し追加する( ofd2.FileName, true );
					this._Model.モーション管理.モーションを適用する( motion, 0, モーション再生終了後の挙動.Replay );
				}

				this.ScreenContext.ワールド空間.Drawableを追加する( this._Model );

				//③ カメラモーションの選択ダイアログを表示し、選ばれたものをScreenContext.CameraMotionProviderに代入する。
				var selector = new CameraControlSelector( this._Model );
				selector.ShowDialog( this );
				this.ScreenContext.カメラモーション = selector.ResultCameraMotionProvider;

                /*
                 * ScreenContext.カメラモーション に代入されたインターフェースの モーションを更新する() が毎回呼ばれることによりカメラを更新している。
                 * この変数の型は カメラモーション インターフェースのため、これを実装すればカメラの動きは容易に定義可能である。
                 */
			}

            Activate();
		}

        protected override void OnClosed( EventArgs e )
        {
            this._Model?.Dispose();
            this._Model = null;

            base.OnClosed( e );
        }

        private PMXModel _Model;
    }
}
