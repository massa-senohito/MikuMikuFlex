using System;
using System.Collections.Generic;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex
{
	public interface スキニング : IDisposable
	{
		PMXボーン[] ボーン配列 { get; }

		Dictionary<string, PMXボーン> ボーンマップ { get; }

		List<PMXボーン> IKボーンリスト { get; }

		List<変形更新> 変形更新リスト { get; }

        event EventHandler スケルトンが更新された;

        
        /// <summary>
        ///     スキニングのモーションデータをエフェクトに適用する際に呼び出します。
        /// </summary>
        /// <param name="d3dEffect"></param>
        void エフェクトを適用する( Effect d3dEffect );

        /// <summary>
        ///     フレームごとにスキニングのデータを更新するために呼び出します。
        /// </summary>
        /// <param name="morphManager"></param>
        void 更新する();

		void ボーンのすべての変形をリセットする();
	}
}
