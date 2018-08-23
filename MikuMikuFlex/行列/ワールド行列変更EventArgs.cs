using System;

namespace MikuMikuFlex.行列
{
	public class ワールド行列変更EventArgs : EventArgs
	{
		public ワールド行列変数種別 変更された種別 { get; private set; }

		public ワールド行列変更EventArgs( ワールド行列変数種別 type )
		{
			変更された種別 = type;
		}
	}
}
