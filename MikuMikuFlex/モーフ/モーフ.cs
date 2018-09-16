using System.Collections.Generic;

namespace MikuMikuFlex
{
	public interface モーフ
	{
		void フレームを設定する( float フレーム, IEnumerable<モーフモーション> モーフモーションリスト );

		bool 進捗率を設定する( float 進捗率, string モーフ名 );

		void 更新する();
    }
}