using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
	/// <summary>
	///		階層構造順に要素を取得できる List 型。
	/// </summary>
	/// <typeparam name="T">
	///		要素の型。
	/// </typeparam>
	/// <remarks>
	///		階層構造（親子関係）を持つ要素配列（T[]）を、親→子の順番になるように並べ替えて自身に格納する。
	///		（例: E1,E2,E11,E12,E21,E22 → E1,E11,E12,E2,E21,E22）
	/// </remarks>
	public class 階層順リスト<T> : List<T>
	{
        /// <summary>
        ///		コンストラクタ。
        /// </summary>
        /// <param name="ソートしたい要素の配列">
        ///		要素はインデックス順に並んでいるものとする。
        ///	</param>
        public 階層順リスト( T[] ソートしたい要素の配列, Func<T,int> 親のインデックスを返す, Func<T,int> 自身のインデックスを返す )
        {
			var ソート済みインデックスキュー = new Queue<int>();

			var ソート済み要素s = new HashSet<int>();
			ソート済み要素s.Add( -1 );	// ルート要素のインデックス

			while( ソート済みインデックスキュー.Count != ソートしたい要素の配列.Length )	// 全要素が「ソート済み」になるまで、
			{
				foreach( var 要素 in ソートしたい要素の配列 )							// 全要素を繰り返し確認する。
				{
					int 自index = 自身のインデックスを返す( 要素 );
					int 親index = 親のインデックスを返す( 要素 );

					// 親要素がすでに格納済みの場合のみ、自要素をキューに格納する。
					if( ソート済み要素s.Contains( 親index ) && !ソート済み要素s.Contains( 自index ) )
					{
						ソート済みインデックスキュー.Enqueue( 自index );
						ソート済み要素s.Add( 自index );
					}
				}
			}

			// ソート済みキューの順番に、自分（List<T>）に要素を格納する。
			while( ソート済みインデックスキュー.Count != 0 )
			{
				this.Add( ソートしたい要素の配列[ ソート済みインデックスキュー.Dequeue() ] );
			}
		}
	}
}
