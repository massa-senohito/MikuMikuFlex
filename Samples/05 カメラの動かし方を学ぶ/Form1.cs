using System;
using System.Windows.Forms;
using MMF;
using MMF.コントロール.Forms;
using MMF.モデル.PMX;
using MMF.モーション;

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

			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
			if( ofd.ShowDialog() == DialogResult.OK )
			{
                PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );

				var ofd2 = new OpenFileDialog();
				ofd2.Filter = "vmdモーションファイル(*.vmd)|*.vmd";
				if( ofd2.ShowDialog() == DialogResult.OK )
				{
					モーション motion = model.モーション管理.ファイルからモーションを生成し追加する( ofd2.FileName, true );
					model.モーション管理.モーションを適用する( motion, 0, モーション再生終了後の挙動.Replay );
				}

				ScreenContext.ワールド空間.Drawableを追加する( model );

				//③ カメラモーションの選択ダイアログを表示し、選ばれたものをScreenContext.CameraMotionProviderに代入する。
				var selector = new CameraControlSelector( model );
				selector.ShowDialog( this );
				ScreenContext.カメラモーション = selector.ResultCameraMotionProvider;
				
				/*
                 * ScreenContext.CameraMotionProviderに代入されたインターフェースのUpdateCameraが毎回呼ばれることによりカメラを更新している。
                 * この変数の型はICameraMotionProviderのため、これを実装すればカメラの動きは容易に定義可能である。
                 */
			}

            Activate();
		}
	}
}
