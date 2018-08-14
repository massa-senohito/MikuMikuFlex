using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.モデル;

namespace MMF.DeviceManager
{
	public class DrawableGroup : IComparable<DrawableGroup>, IDisposable
	{
        public DrawableGroup( int priorty, string groupName )
		{
			this._優先度 = priorty;
			this.グループ名 = groupName;
		}

        public string グループ名 { get; }


        public void Drawableを追加する( IDrawable drawable )
		{
			_drawables.Add( drawable );
		}

		public bool Drawableを削除する( IDrawable drawable )
		{
			if( _drawables.Contains( drawable ) )
			{
				_drawables.Remove( drawable );
				return true;
			}
			return false;
		}

        public void 更新する()
        {
            foreach( var drawable in _drawables )
            {
                drawable.更新する();
            }
        }

        public void 描画する()
		{
			this.PreDraw();

            foreach( var drawable in _drawables )
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
			return _drawables.FirstOrDefault( drawable => drawable.ファイル名.Equals( ファイル名 ) );
		}

		public int CompareTo( DrawableGroup other )
		{
			return this._優先度 - other._優先度;
		}

		public void Dispose()
		{
			foreach( var drawable in _drawables )
			{
				drawable.Dispose();
			}

            _drawables.Clear();
		}


        protected int _優先度;

        private List<IDrawable> _drawables = new List<IDrawable>();
    }
}
