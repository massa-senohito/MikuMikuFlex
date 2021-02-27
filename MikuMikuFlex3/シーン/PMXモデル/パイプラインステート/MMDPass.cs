namespace MikuMikuFlex3
{
    /// <summary>
    ///     MMEのパスのタイプ。
    ///     テクニックを適用する描画対象を指定する。
    ///     詳しくはMME仕様158行目近辺参照
    /// </summary>
    public enum MMDPass
	{
		ObjectWithSelfShadow,

        Object,

        ZPlotForSelfShadow,
		
        /// <summary>
		///     セルフ影ではない簡単な影。
		/// </summary>
		Shadow,
		
        /// <summary>
		///     輪郭。PMDのみ。
		/// </summary>
		Edge,
	}
}
