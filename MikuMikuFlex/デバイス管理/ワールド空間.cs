using System;
using System.Collections.Generic;
using System.Linq;
using MikuMikuFlex.スプライト;

namespace MikuMikuFlex
{
	public class ワールド空間 : IDisposable
	{
        public IReadOnlyList<DrawableGroup> DrawableGroupリスト
            => _DrawableGroupリスト;

        public List<IMovable> Movableリスト
        {
            get;
            private set;
        } = new List<IMovable>();

        public List<動的テクスチャ> DynamicTextureリスト
        {
            get;
            private set;
        } = new List<動的テクスチャ>();


        public ワールド空間()
		{
            // 既定（"Default"）のグループを追加する。
			_DrawableGroupリスト.Add( new DrawableGroup( 0, "Default" ) );
		}

        public void Dispose()
        {
            foreach( var drawableGroup in _DrawableGroupリスト )
            {
                drawableGroup.Dispose();
            }

            foreach( var dynamicTexture in DynamicTextureリスト )
            {
                dynamicTexture?.Dispose();
            }
        }

        public void DrawableGroupを追加する( DrawableGroup group )
		{
            if( !( _DrawableGroupリスト.Contains( group ) ) )
            {
                _DrawableGroupリスト.Add( group );
                _DrawableGroupリスト.Sort();
            }
        }

		public void DrawableGroupを削除する( string key )
		{
            var 削除するグループ = _DrawableGroupリスト.FirstOrDefault( ( g ) => ( g.グループ名 == key ) );

            if( null != 削除するグループ )
    			_DrawableGroupリスト.Remove( 削除するグループ );
		}

        public void Drawableを追加する( IDrawable drawable, String groupName = "Default" )
		{
            // 指定された名前の描画対象グループに追加する
			_DrawableGroupリスト.First( group => group.グループ名.Equals( groupName ) ).Drawableを追加する( drawable );

            // 他のリストにも追加する

            if( drawable is IMovable )
				Movableリスト.Add( (IMovable) drawable );
		}

		public void Drawableを削除する( IDrawable drawable )
		{
			foreach( var drawableGroup in _DrawableGroupリスト )
			{
				if( drawableGroup.Drawableを削除する( drawable ) )
				{
					if( drawable is IMovable )
					{
						Movableリスト.Remove( (IMovable) drawable );
					}
				}
			}
		}

		public IDrawable Drawableを取得する( string fileName )
		{
			foreach( var drawableGroup in _DrawableGroupリスト )
			{
				var drawable = drawableGroup.Drawableを取得する( fileName );

                if( drawable != null )
                    return drawable;
			}
			return null;
		}

		public void DynamicTextureを追加する( 動的テクスチャ dtexture )
		{
			DynamicTextureリスト.Add( dtexture );
		}

		public void DynamicTextureを削除する( 動的テクスチャ dtexture )
		{
			if( DynamicTextureリスト.Contains( dtexture ) )
                DynamicTextureリスト.Remove( dtexture );
		}

        public void すべてのDynamicTextureを更新する()
		{
			foreach( var dynamicTexture in this.DynamicTextureリスト )
			{
				dynamicTexture.更新する();
			}
		}

        public void すべてのMovableを更新する()
        {
            foreach( var movable in this.Movableリスト )
            {
                movable.更新する();
            }
        }

        public void 登録されているすべての描画の必要があるものを描画する()
        {
            foreach( var 描画対象グループ in _DrawableGroupリスト )
            {
                RenderContext.Instance.ブレンドステート管理?.ブレンドステートを設定する( RenderContext.Instance.DeviceManager.D3DDeviceContext, ブレンドステート管理.BlendStates.Alignment );

                描画対象グループ.描画する();
            }
        }


        private List<DrawableGroup> _DrawableGroupリスト = new List<DrawableGroup>();
    }
}
