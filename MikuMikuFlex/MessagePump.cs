using System;
using System.Windows.Forms;
using MMF;
using SharpDX.Windows;
using RenderForm = MMF.コントロール.Forms.RenderForm;

namespace MMF
{
	/// <summary>
	///		描画ループをまわすためのメソッドを提供するクラス
	/// </summary>
	public static class MessagePump
	{
		public static void Run( RenderForm form )
		{
			RenderLoop.Run( form, form.Render );
		}

		public static void Run( Form form, RenderLoop.RenderCallback renderMethod )
		{
			RenderLoop.Run( form, renderMethod );
		}
	}
}
