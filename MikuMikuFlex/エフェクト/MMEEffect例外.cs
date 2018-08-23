using System;

namespace MikuMikuFlex.エフェクト
{
	/// <summary>
	///     MMEエフェクトのエラー
	/// </summary>
	public class MMEEffect例外 : Exception
	{
		private readonly string message = "MMEエフェクトの解析中に例外が発生しました。";

		public MMEEffect例外( string message )
		{
			this.message = message;
		}

		public override string Message
		{
			get { return message; }
		}
	}
}
