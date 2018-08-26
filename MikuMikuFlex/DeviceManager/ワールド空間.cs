using System;
using System.Collections.Generic;
using System.Linq;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モーション;
using MikuMikuFlex.スプライト;

namespace MikuMikuFlex.DeviceManager
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

        public List<IGroundShadowDrawable> GroundShadowDrawableリスト
        {
            get;
            private set;
        } = new List<IGroundShadowDrawable>();

        public List<IEdgeDrawable> EdgeDrawableリスト
        {
            get;
            private set;
        } = new List<IEdgeDrawable>();

        public bool IsDisposed { get; private set; }


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

            IsDisposed = true;
        }

        public void DrawableGroupを追加する( DrawableGroup group )
		{
			_DrawableGroupリスト.Add( group );
            _DrawableGroupリスト.Sort();
        }

		public void DrawableGroupを削除する( string key )
		{
			DrawableGroup removeTarget = null;

			foreach( var drawableGroup in _DrawableGroupリスト )
			{
				if( drawableGroup.グループ名.Equals( key ) )
				{
					removeTarget = drawableGroup;
					break;
				}
			}

            if( removeTarget != null )
    			_DrawableGroupリスト.Remove( removeTarget );
		}

        public void Drawableを追加する( IDrawable drawable, String groupName = "Default" )
		{
            // 指定された名前の描画対象グループに追加する
			_DrawableGroupリスト.First( group => group.グループ名.Equals( groupName ) ).Drawableを追加する( drawable );

            // 他のリストにも追加する

            if( drawable is IMovable )
				Movableリスト.Add( (IMovable) drawable );

			if( drawable is IEdgeDrawable )
				EdgeDrawableリスト.Add( (IEdgeDrawable) drawable );

			if( drawable is IGroundShadowDrawable )
				GroundShadowDrawableリスト.Add( (IGroundShadowDrawable) drawable );
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

        public void すべてのDrawableGroupを更新する()
        {
            foreach( var drawableGroup in this.DrawableGroupリスト )
            {
                drawableGroup.更新する();
            }
        }

        public void 登録されているすべての描画の必要があるものを描画する()
        {

            // (1) エッジ
            foreach( var エッジ描画リソース in EdgeDrawableリスト )
            {
                RenderContext.Instance.ブレンドステート管理?.ブレンドステートを設定する( RenderContext.Instance.DeviceManager.D3DDeviceContext, ブレンドステート管理.BlendStates.Disable );

                if( エッジ描画リソース.表示中 )
                    エッジ描画リソース.エッジを描画する();
            }

            // (2) 描画グループ
            foreach( var 描画対象グループ in _DrawableGroupリスト )
            {
                RenderContext.Instance.ブレンドステート管理?.ブレンドステートを設定する( RenderContext.Instance.DeviceManager.D3DDeviceContext, ブレンドステート管理.BlendStates.Alignment );

                描画対象グループ.描画する();
            }

            // (3) 地面影
            foreach( var 地面影描画リソース in GroundShadowDrawableリスト )
            {
                RenderContext.Instance.ブレンドステート管理?.ブレンドステートを設定する( RenderContext.Instance.DeviceManager.D3DDeviceContext, ブレンドステート管理.BlendStates.Alignment );

                if( 地面影描画リソース.表示中 )
                    地面影描画リソース.地面影を描画する();
            }
        }


        private List<DrawableGroup> _DrawableGroupリスト = new List<DrawableGroup>();
    }
}
