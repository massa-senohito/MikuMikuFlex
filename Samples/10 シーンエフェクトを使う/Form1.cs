using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuFlex;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モデル.PMX;
using MikuMikuFlex.行列.CameraMotion;

namespace _10_シーンエフェクトを使う
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

            var exeFolder = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );

            this.レンダーターゲットのクリア色 = new SharpDX.Vector3( 1f, 1f, 1f );    // 白

            //ファイルを開くダイアログ
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "pmxモデルファイル(*.pmx)|*.pmx";
            if( ofd.ShowDialog() == DialogResult.OK )
            {
                // モデルを読み込んでワールド空間に追加。
                this._Model = PMXModel物理変形付き.ファイルから読み込む( ofd.FileName );
                this.ScreenContext.ワールド空間.Drawableを追加する( this._Model );

                // サンプルのシーンエフェクトファイルを読み込んでワールド空間に追加。
                this._Scene = new シーンモデル( "グラデーションサンプル", $@"{exeFolder}\SampleSceneShader.fx", this.ScreenContext );
                this.ScreenContext.ワールド空間.Drawableを追加する( this._Scene );
            }

            // マウスで操作できるカメラモーションを読み込んで、画面（のカメラ）に適用。
            this.ScreenContext.カメラモーション = new マウスカメラモーション( this, this );

            // ウィンドウが後ろに隠れることがあるので、念のため。
            Activate();
        }

        protected override void OnClosed( EventArgs e )
        {
            this._Scene?.Dispose();
            this._Scene = null;

            this._Model?.Dispose();
            this._Model = null;

            base.OnClosed( e );
        }

        private PMXModel _Model;

        private シーンモデル _Scene;
    }
}
