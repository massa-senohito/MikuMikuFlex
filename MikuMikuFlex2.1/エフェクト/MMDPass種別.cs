namespace MikuMikuFlex
{
    /// <summary>
    ///     MMEのパスのタイプ。
    ///     テクニックを適用する描画対象を指定する。
    ///     詳しくはMME仕様158行目近辺参照
    /// </summary>
    public enum MMDPass種別
	{
		オブジェクト本体_セルフ影あり,

        オブジェクト本体,

        セルフ影用Z値プロット,
		
        /// <summary>
		///     セルフ影ではない簡単な影。
		/// </summary>
		影,
		
        /// <summary>
		///     輪郭。PMDのみ。
		/// </summary>
		エッジ,

        スキニング,

        シーン,
	}
}
