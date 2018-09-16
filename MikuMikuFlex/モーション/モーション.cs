using System;
using MikuMikuFlex.モーフ;

namespace MikuMikuFlex.モーション
{
	public interface モーション
	{
		bool アタッチ済み { get; }

		float 現在のフレーム { get; set; }

		int このモーションにおける最終フレーム { get; }

		void モーションをアタッチする( PMXボーン[] bones );

        /// <summary>
        ///     モーションを再生します。
        /// </summary>
        /// <param name="frame">再生開始フレーム</param>
        /// <param name="action">モーション終了後の挙動</param>
        void モーションを再生する( float frame, モーション再生終了後の挙動 action );

		void モーションを停止する();

		/// <summary>
		/// モーションを１フレーム進めます。
		/// </summary>
		/// <param name="fps">秒間フレーム数</param>
		/// <param name="elapsedTime">前回のフレームからどれだけ時間がかかったか[秒]</param>
		/// <param name="morphManager">モーフの管理クラス</param>
		void モーションを1フレーム進める( int fps, float elapsedTime, モーフ管理 morphManager );

		event EventHandler<モーション再生終了後の挙動> モーションが終了した;

		event EventHandler<EventArgs> FrameTicked;
	}

    public enum モーション再生終了後の挙動
    {
        Nothing,
        Replay
    }
}