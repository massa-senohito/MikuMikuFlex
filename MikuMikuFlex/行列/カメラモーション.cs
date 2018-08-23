using MikuMikuFlex.行列;

namespace MikuMikuFlex.行列
{
	public interface カメラモーション
	{
		void モーションを更新する( カメラ camera, 射影 projection );
	}
}
