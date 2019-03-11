using System;

namespace MikuMikuFlex
{
	/// <summary>
	///     カメラの設定が変更された時のイベントアーギュメント
	/// </summary>
	public class カメラ変更EventArgs : EventArgs
	{
        public カメラ変数種別 変更された種別 { get; private set; }

        
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="type">変えられたカメラの内容</param>
        public カメラ変更EventArgs( カメラ変数種別 type )
		{
			変更された種別 = type;
		}
	}
}
