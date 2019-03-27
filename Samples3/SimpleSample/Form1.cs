using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleSample
{
    public partial class Form1 : Form
    {
        public string pmxファイルパス;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad( EventArgs e )
        {
            this._MainTask = new MainTask( this, pmxファイルパス );

            Task.Run( () => {
                this._MainTask.MainLoop();
            } );

            base.OnLoad( e );
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            this._MainTask.終了指示通知.Set();
            this._MainTask.終了完了通知.Wait( 5000 );

            base.OnClosing( e );
        }

        private MainTask _MainTask;
    }
}
