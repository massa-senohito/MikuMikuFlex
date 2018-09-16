using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex
{
    /// <summary>
    ///     <see cref="IDrawable"/> をグループ化する機能。
    ///     グループそれぞれに名前と優先度を持ち、優先度をキーとする <see cref="IComparable"/> を実装する。
    /// </summary>
	public class DrawableGroup : IComparable<DrawableGroup>, IDisposable
	{
        public string グループ名 { get; }

        /// <summary>
        ///     このグループに所属する <see cref="IDrawable"/> のリスト。
        /// </summary>
        public List<IDrawable> Drawableリスト = new List<IDrawable>();


        public DrawableGroup( int 優先度, string グループ名 )
        {
            this._優先度 = 優先度;
            this.グループ名 = グループ名;
        }

        public void Dispose()
        {
            foreach( var drawable in Drawableリスト )
            {
                drawable.Dispose();
            }

            Drawableリスト.Clear();
        }

        public void Drawableを追加する( IDrawable drawable )
		{
			Drawableリスト.Add( drawable );
		}

		public bool Drawableを削除する( IDrawable drawable )
		{
			if( Drawableリスト.Contains( drawable ) )
			{
				Drawableリスト.Remove( drawable );
				return true;
			}
			return false;
		}

        public IDrawable Drawableを取得する( string ファイル名 )
        {
            return Drawableリスト.FirstOrDefault( drawable => drawable.ファイル名.Equals( ファイル名 ) );
        }

        public void 更新する()
        {
            foreach( var drawable in Drawableリスト )
            {
                drawable.更新する();
            }
        }

        public void 描画する()
		{
			this.OnPreDraw();

            foreach( var drawable in Drawableリスト )
			{
				if( drawable.表示中 )
                    drawable.描画する();
			}

            this.OnPostDraw();
		}

		protected virtual void OnPostDraw()
		{
            // 描画前に行うべきグループ固有の処理があればここへ。
		}

		protected virtual void OnPreDraw()
		{
            // 描画後に行うべきグループ固有の処理があればここへ。
        }

        public int CompareTo( DrawableGroup other )
		{
			return this._優先度 - other._優先度;
		}


        protected int _優先度;
    }
}
