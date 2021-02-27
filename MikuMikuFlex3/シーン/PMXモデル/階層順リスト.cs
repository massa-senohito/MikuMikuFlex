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
	///		階層構造（親子関係）を持つ要素配列（T[]）を、Parent→子の順番になるように並べ替えて自身に格納する。
	///		（例: E1,E2,E11,E12,E21,E22 → E1,E11,E12,E2,E21,E22）
	/// </remarks>
	class HierarchicalList<T> : List<T>
	{
        /// <summary>
        ///		コンストラクタ。
        /// </summary>
        /// <param name="ArrayOfElementsYouWantToSort">要素はインデックス順に並んでいるものとする。</param>
        public HierarchicalList( T[] ArrayOfElementsYouWantToSort, Func<T,int> ReturnsTheParentIndex, Func<T,int> ReturnsItsOwnIndex )
        {
			var SortedIndexQueue = new Queue<int>();

			var SortedElementss = new HashSet<int>();
			SortedElementss.Add( -1 );	// ルート要素のインデックス

			while( SortedIndexQueue.Count != ArrayOfElementsYouWantToSort.Length )	// 全要素が「ソート済み」になるまで、
			{
				foreach( var Element in ArrayOfElementsYouWantToSort )							// 全要素を繰り返し確認する。
				{
					int Selfindex = ReturnsItsOwnIndex( Element );
					int Parentindex = ReturnsTheParentIndex( Element );

					// 親要素がすでに格納済みの場合のみ、自要素をキューに格納する。
					if( SortedElementss.Contains( Parentindex ) && !SortedElementss.Contains( Selfindex ) )
					{
						SortedIndexQueue.Enqueue( Selfindex );
						SortedElementss.Add( Selfindex );
					}
				}
			}

			// ソート済みキューの順番に、自分（List<T>）に要素を格納する。
			while( SortedIndexQueue.Count != 0 )
			{
				this.Add( ArrayOfElementsYouWantToSort[ SortedIndexQueue.Dequeue() ] );
			}
		}
	}
}
