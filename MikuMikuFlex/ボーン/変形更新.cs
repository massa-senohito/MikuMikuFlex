using MikuMikuFlex.モデル;

namespace MikuMikuFlex
{
	public interface 変形更新
	{
		/// <summary>
		/// ボーンの変化量などを計算し、ボーンに割り当てる
		/// </summary>
		/// <returns>この値を元に行列を即生成するかどうか</returns>
		bool 変形を更新する();
	}
}
