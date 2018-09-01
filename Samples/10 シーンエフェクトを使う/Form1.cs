using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _10_シーンエフェクトを使う
{
    public partial class Form1 : MikuMikuFlex.コントロール.Forms.RenderForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var exeFolder = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );

            this.レンダーターゲットのクリア色 = new SharpDX.Vector3( 1f, 1f, 1f );    // 白

            //ファイルを開くダイアログ
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
            if( ofd.ShowDialog() == DialogResult.OK )
            {
                // モデルを読み込んでワールド空間に追加。
                var model = MikuMikuFlex.モデル.PMX.PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
                ScreenContext.ワールド空間.Drawableを追加する( model );

                // サンプルのシーンエフェクトファイルを読み込んでワールド空間に追加。
                var scene = new MikuMikuFlex.モデル.シーンモデル( "グラデーションサンプル", $@"{exeFolder}\SampleSceneShader.fx", this.ScreenContext );
                this.ScreenContext.ワールド空間.Drawableを追加する( scene );
            }

            // マウスで操作できるカメラモーションを読み込んで、画面（のカメラ）に適用。
            this.ScreenContext.カメラモーション = new MikuMikuFlex.行列.CameraMotion.マウスカメラモーション( this, this );

            // ウィンドウが後ろに隠れることがあるので、念のため。
            Activate();
        }
    }
}
