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

namespace ニコニ立体ちゃんサンプル
{
    /// <summary>
    ///    ニコニ立体ちゃんが、ニコニ立体ステージの上で簡単なステップモーションを踊ります。
    ///    画面上でマウスを操作することで、カメラ（視点等）を操作することができます。
    ///    右ボタンや中ボタンを押したままドラッグしてカメラ位置を移動したり、ホイールで拡大縮小したりできます。
    /// </summary>
    public partial class Form1 : RenderForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var sampleFolder = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location ) + @"\サンプルデータ";

            
            // ニコニ立体ちゃんのモデルを読み込んで、画面内のワールド空間に追加。

            this._Model = PMXModel物理変形付き.ファイルから読み込む( $@"{sampleFolder}\Alicia\MMD\Alicia_solid.pmx" );
            this.ScreenContext.ワールド空間.Drawableを追加する( this._Model );


            // ニコニ立体ステージのモデルを読み込んで、画面内のワールド空間に追加。

            this._Stage = PMXModel物理変形付き.ファイルから読み込む( $@"{sampleFolder}\nicosolid_stage\nicosolid_stage.pmx" );
            this.ScreenContext.ワールド空間.Drawableを追加する( this._Stage );

            
            // モーションを読み込んで、ニコニ立体ちゃんモデルに適用。

            モーション motion = this._Model.モーション管理.ファイルからモーションを生成し追加する( $@"{sampleFolder}\Alicia\MMD Motion\2分ループステップ1.vmd", true );
            this._Model.モーション管理.モーションを適用する( motion, 0, モーション再生終了後の挙動.Replay );

            
            // マウスで操作できるカメラモーションを読み込んで、画面（のカメラ）に適用。

            this.ScreenContext.カメラモーション = new マウスカメラモーション( this, this );


            // ウィンドウが後ろに隠れることがあるので、念のため。

            Activate();
        }

        protected override void OnClosed( EventArgs e )
        {
            this._Stage?.Dispose();
            this._Stage = null;

            this._Model?.Dispose();
            this._Model = null;

            base.OnClosed( e );
        }

        private PMXModel _Model;

        private PMXModel _Stage;
    }
}
