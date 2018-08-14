using System;
using System.Windows.Forms;
/*
 * *****************************************************************************************************************************************************************
 * MMFチュートリアル 05「カメラの動かし方を学ぶ」
 * 
 * ◎このセクションの目的
 * 1,MMFでどのようにカメラを動かすか学ぶ
 * 
 * ◎所要時間
 * 20分
 * 
 * ◎難易度
 * カンタン♪
 * 
 * ◎このチュートリアルの工程
 * ①～③
 * ・Form1.cs
 * ・CameraControlSelector.cs
 * ・SimpleUserDefinitionCameraMotionProvider.cs
 * の3ファイル
 * 
 * ◎必須ランタイム
 * DirectX エンドユーザーランタイム
 * .Net Framework 4.5
 * 
 * ◎終着点
 * カメラが動けばOK
 * 
 ********************************************************************************************************************************************************************/
using MMF;

namespace _05_HowToUpdateCamera
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			MessagePump.Run( new Form1() );
		}
	}
}
