using MikuMikuFlex.ボーン;

namespace MikuMikuFlex.モーション
{
	public interface IMovable
	{
		/// <summary>
		/// 動かす対象のボーン
		/// </summary>
		スキニング スキニング { get; }

		/// <summary>
		/// 動かす対象のモーションマネージャ
		/// </summary>
		モーション管理 モーション管理 { get; }
	}
}
