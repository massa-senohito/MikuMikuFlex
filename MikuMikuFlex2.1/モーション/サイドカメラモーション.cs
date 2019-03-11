using SharpDX;

namespace MikuMikuFlex
{
    /// <summary>
    ///     サイドカメラは、指定された追従先カメラの注視点から指定された量だけ回転した場所に配置されるカメラを表す。
    ///     サイドカメラの注視点は、追従先カメラと同じ。
    /// </summary>
	public class サイドカメラモーション : カメラモーション
	{
		public サイドカメラモーション( カメラモーション camera, Quaternion rotation )
		{
			_カメラモーション = camera;
			_回転量 = rotation;
		}

		public void モーションを更新する( カメラ camera, 射影 projection )
		{
			_カメラモーション.モーションを更新する( camera, projection );

			var サイドカメラの位置 = camera.カメラの位置 - camera.カメラの注視点;
			サイドカメラの位置 = Vector3.TransformNormal( サイドカメラの位置, Matrix.RotationQuaternion( _回転量 ) );

			camera.カメラの位置 = camera.カメラの注視点 + サイドカメラの位置;
		}


        private readonly カメラモーション _カメラモーション;

        private readonly Quaternion _回転量;
    }
}
