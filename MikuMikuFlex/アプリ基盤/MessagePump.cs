using System;
using System.Windows.Forms;
using MikuMikuFlex;
using SharpDX.Windows;

namespace MikuMikuFlex
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
