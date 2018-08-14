using MMF.行列;

namespace MMF.行列
{
	public interface カメラモーション
	{
		void モーションを更新する( カメラ camera, 射影 projection );
	}
}
