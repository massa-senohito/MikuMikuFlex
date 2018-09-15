using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.DeviceManagement
{
	public class DrawableGroup : IComparable<DrawableGroup>, IDisposable
	{
        public string グループ名 { get; }

        public List<IDrawable> Drawableリスト = new List<IDrawable>();


        public DrawableGroup( int priorty, string groupName )
        {
            this._優先度 = priorty;
            this.グループ名 = groupName;
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

        public void 更新する()
        {
            foreach( var drawable in Drawableリスト )
            {
                drawable.更新する();
            }
        }

        public void 描画する()
		{
			this.PreDraw();

            foreach( var drawable in Drawableリスト )
			{
				if( drawable.表示中 )
                    drawable.描画する();
			}

            this.PostDraw();
		}

		protected virtual void PostDraw()
		{

		}

		protected virtual void PreDraw()
		{

		}

		public IDrawable Drawableを取得する( string ファイル名 )
		{
			return Drawableリスト.FirstOrDefault( drawable => drawable.ファイル名.Equals( ファイル名 ) );
		}

		public int CompareTo( DrawableGroup other )
		{
			return this._優先度 - other._優先度;
		}

		public void Dispose()
		{
			foreach( var drawable in Drawableリスト )
			{
				drawable.Dispose();
			}

            Drawableリスト.Clear();
		}


        protected int _優先度;
    }
}
