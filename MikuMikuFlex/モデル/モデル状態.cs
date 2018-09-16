using SharpDX;

namespace MikuMikuFlex
{
	public interface モデル状態
	{
		/// <summary>
		///     モデルのワールド座標での位置を表します。
		/// </summary>
		Vector3 位置 { get; set; }

		/// <summary>
		///     モデルのワールド空間において向いている向きを表します。
		/// </summary>
		Vector3 前方向 { get; set; }

		/// <summary>
		///     ワールド空間において上の向きを表します。
		/// </summary>
		Vector3 上方向 { get; set; }

		/// <summary>
		///     モデルのワールド空間においての回転を表します。
		/// </summary>
		Quaternion 回転 { get; set; }

		/// <summary>
		///     モデルの倍率を表します
		/// </summary>
		Vector3 倍率 { get; set; }

		/// <summary>
		///     初期状態での上方向を表します
		/// </summary>
		Vector3 上方向の初期値 { get; }

		/// <summary>
		///     初期状態での向いている方向を表します。
		/// </summary>
		Vector3 前方向の初期値 { get; }


        void 初期状態に戻す();

		/// <summary>
		///     モデルの変換行列
		/// </summary>
		Matrix ローカル変換行列 { get; }
	}
}
