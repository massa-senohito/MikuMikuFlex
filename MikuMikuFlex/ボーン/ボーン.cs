using SharpDX;

namespace MikuMikuFlex
{
	public interface ボーン
	{
		Vector3 移動 { get; set; }

		Quaternion 回転 { get; set; }

		Matrix モデルポーズ行列 { get; }


		void モデルポーズを更新する();
	}
}
