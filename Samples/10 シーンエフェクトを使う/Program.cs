using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _10_シーンエフェクトを使う
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            //Application.Run( new Form1() );

            // 一定間隔で RenderForm を更新・描画する。
            MikuMikuFlex.MessagePump.Run( new Form1() );
        }
    }
}
