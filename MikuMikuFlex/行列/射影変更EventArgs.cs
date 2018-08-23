using System;

namespace MikuMikuFlex.行列
{
	public class 射影変更EventArgs : EventArgs
	{
		public 射影変数種別 変更された種別 { get; private set; }


		public 射影変更EventArgs( 射影変数種別 type )
		{
			this.変更された種別 = type;
		}
	}
}
