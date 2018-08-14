using SharpDX;

namespace MMF.ボーン
{
	public interface ボーン
	{
		Vector3 移動 { get; set; }

		Quaternion 回転 { get; set; }

		Matrix グローバルポーズ行列 { get; }


		void グローバルポーズを更新する();
	}
}
