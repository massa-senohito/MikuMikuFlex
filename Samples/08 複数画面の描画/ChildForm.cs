using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MikuMikuFlex;

namespace _08_MultiScreenRendering
{
    public partial class ChildForm : RenderForm
    {
        //③ RenderContextを渡すと、そのRenderContextを用いて初期化してくれる。
        //つまり、同じデバイスを用いて初期化する。
        public ChildForm() : base()
        {
            InitializeComponent();
        }
    }
}
