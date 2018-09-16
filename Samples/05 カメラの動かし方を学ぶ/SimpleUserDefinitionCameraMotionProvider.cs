using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using MikuMikuFlex;

namespace _05_HowToUpdateCamera
{
	//①-C 独自定義のカメラモーションを記述してみる
	class SimpleUserDefinitionCameraMotionProvider : カメラモーション
	{
		private float t;

		public void モーションを更新する( カメラ cp, 射影 proj )
		{
			t += 0.01f;

			//tに応じて三角関数によりカメラの位置を変更する
			cp.カメラの位置 = new Vector3( 0, 0, (float) Math.Sin( t ) * 50 );

			/*
			 * 説明：
			 * CameraProvider.CameraPositionがカメラの位置を指す
			 * CameraProvider.CameraLookAtがカメラの注視点を指す
			 * CameraProvider.CameraUpVecがカメラの上方向ベクトルを指す
			 * 
			 * IProjectionMatrixProviderのプロパティは以下
			 * ・ZNear 近クリップ距離
			 * ・ZFar 遠クリップ距離
			 * ・AspectRatio アスペクト比
			 * ・Fovy 視野角
			 */
		}
	}
}