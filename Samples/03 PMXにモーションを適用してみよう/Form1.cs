using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.コントロール.Forms;
using MikuMikuFlex.モデル.PMX;
using MikuMikuFlex.モーション;

namespace _03_ApplyVMDToPMX
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

			// モデルファイルの読み込み用ダイアログの設置
			var ofd = new OpenFileDialog();
			ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
            if( ofd.ShowDialog() == DialogResult.OK )
            {
                PMXModel model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );

                // モーションファイルの読み込み用ダイアログの設置
                var ofd2 = new OpenFileDialog();
                ofd2.Filter = "vmdモーションファイル(*.vmd)|*.vmd";
                if( ofd2.ShowDialog() == DialogResult.OK )
                {
                    // ダイアログの返値がいずれもOKの場合、モデルの読み込み処理をする

                    //①モーションファイルを読み込む
                    モーション motion = model.モーション管理.ファイルからモーションを生成し追加する( ofd2.FileName, true );
                    //適用したい対象のモデルのモーションマネージャに対して追加します。
                    //IMotionProvider AddMotionFromFile(string ファイル名,bool すべての親ボーンを無視するかどうか);
                    //第二引数は歩きモーションなどで、移動自体はプログラムで指定したいとき、すべての親ボーンのモーションを無視することで、
                    //モーションでモデル全体が動いてしまうのを防ぎます。

                    //②モーションファイルをモデルに対して適用する。
                    model.モーション管理.モーションを適用する( motion, 0, モーション再生終了後の挙動.Replay );
                    //第二引数は、再生を始めるフレーム番号、第三引数は再生後にリプレイするかどうか。
                    //リプレイせず放置する場合はActionAfterMotion.Nothingを指定する

                    //ｵﾏｹ
                    //(1) モーションをとめるときは?
                    //model.MotionManager.StopMotion();と記述すれば止まります
                    //(2) 現在何フレーム目なの?
                    //model.MotionManager.CurrentFrameによって取得できます。
                }
                ScreenContext.ワールド空間.Drawableを追加する( model );

            }

            Activate();
        }
	}
}
