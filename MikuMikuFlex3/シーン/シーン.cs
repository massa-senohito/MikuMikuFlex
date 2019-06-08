using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;  
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class シーン : IDisposable
    {
        public Size2F ViewportSize
        {
            get => new Size2F( this._GlobalParameters.ViewportSize.X, this._GlobalParameters.ViewportSize.Y );
            set => this._GlobalParameters.ViewportSize = new Vector2( value.Width, value.Height );
        }

        protected List<カメラ> カメラリスト { get; private protected set; } = new List<カメラ>();

        public カメラ 選択中のカメラ { get; set; }
        
        protected List<照明> 照明リスト { get; private protected set; } = new List<照明>();

        protected List<パス> パスリスト { get; private protected set; } = new List<パス>();

        public Dictionary<object, (Resource tex, Color4 clearColor)> グローバルテクスチャリスト { get; protected set; } = new Dictionary<object, (Resource tex, Color4 clearColor)>();


        public シーン( Device d3dDevice, Texture2D depthStencil, Texture2D renderTarget )
        {
            this._D3DDevice = d3dDevice;
            this.スワップチェーンに依存するリソースを作成する( d3dDevice, depthStencil, renderTarget );
            this.パスリスト = new List<パス>();
        }

        public virtual void Dispose()
        {
            this.カメラリスト = null;      // Dispose不要
            this.選択中のカメラ = null;    //
            this.照明リスト = null;        //

            foreach( var pass in this.パスリスト )
                pass.Dispose();
            this.パスリスト = null;

            foreach( var kvp in this.グローバルテクスチャリスト )
                kvp.Value.tex?.Dispose();

            this.スワップチェーンに依存するリソースを解放する();

            this._D3DDevice = null;     // Dispose はしない
        }

        public void スワップチェーンに依存するリソースを作成する( Device d3dDevice, Texture2D depthStencil, Texture2D renderTarget )
        {
            this.ViewportSize = new Size2F( renderTarget.Description.Width, renderTarget.Description.Height );

            this._既定のDepthStencil = depthStencil;
            this._既定のRenderTarget = renderTarget;
            this._既定のDepthStencilView = new DepthStencilView( d3dDevice, depthStencil );
            this._既定のRenderTargetView = new RenderTargetView( d3dDevice, renderTarget );
        }

        public void スワップチェーンに依存するリソースを解放する()
        {
            this._既定のDepthStencil = null;   // Dispose はしない
            this._既定のRenderTarget = null;   // Dispose はしない
            this._既定のDepthStencilView?.Dispose();
            this._既定のRenderTargetView?.Dispose();
        }


        public void 追加する( パス pass )
        {
            this.パスリスト.Add( pass );

            // PMXモデルの場合、ビューの設定がなければ既定のビューを設定する。
            if( pass is PMXモデルパス objPass )
            {
                if( null == objPass.深度ステンシルビュー || 0 == objPass.レンダーターゲットビューs.Where( ( rtv ) => ( null != rtv ) ).Count() )
                    objPass.リソースをバインドする( this._D3DDevice, this._既定のDepthStencil, this._既定のRenderTarget );
            }
            // Effekseerパスの場合、ビューの設定がなければ既定のビューを設定する。
            else if( pass is Effekseerパス effekseerPass )
            {
                if( null == effekseerPass.深度ステンシルビュー || 0 == effekseerPass.レンダーターゲットビューs.Where( ( rtv ) => ( null != rtv ) ).Count() )
                    effekseerPass.リソースをバインドする( this._D3DDevice, this._既定のDepthStencil, this._既定のRenderTarget );
            }
        }

        public void 追加する( PMXモデル model )
        {
            var modelPass = new PMXモデルパス( model );
            this.追加する( modelPass );
        }

        public void 追加する( Effekseer effekseer )
        {
            var effekseerPass = new Effekseerパス( effekseer );
            this.追加する( effekseerPass );
        }

        public void 追加する( カメラ camera )
        {
            this.カメラリスト.Add( camera );
        }

        public void 追加する( 照明 light )
        {
            this.照明リスト.Add( light );
        }


        public Texture2D グローバルテクスチャを作成する( Device d3dDevice, object key, Texture2DDescription desc, Color4? clearColor = null )
        {
            var tex2d = new Texture2D( d3dDevice, desc );

            if( this.グローバルテクスチャリスト.ContainsKey( key ) )
            {
                this.グローバルテクスチャリスト[ key ].tex.Dispose();
                this.グローバルテクスチャリスト.Remove( key );
            }

            this.グローバルテクスチャリスト[ key ] = (tex2d, (clearColor.HasValue ? clearColor.Value : Color4.Black ));

            return tex2d;
        }


        public void 描画する( double 現在時刻sec, DeviceContext d3ddc )
        {
            // カメラを進行する。

            if( null == this.選択中のカメラ )
                this.選択中のカメラ = this.カメラリスト[ 0 ];

            this.選択中のカメラ.更新する( 現在時刻sec );


            // GlobalParameters の設定（シーン単位）

            this._GlobalParameters.ViewMatrix = this.選択中のカメラ.ビュー変換行列;
            this._GlobalParameters.ViewMatrix.Transpose();
            this._GlobalParameters.ProjectionMatrix = this.選択中のカメラ.射影変換行列;
            this._GlobalParameters.ProjectionMatrix.Transpose();
            this._GlobalParameters.CameraPosition = new Vector4( this.選択中のカメラ.カメラ位置, 0f );
            this._GlobalParameters.Light1Direction = new Vector4( this.照明リスト[ 0 ].照射方向, 0f );


            // レンダーターゲットと深度ステンシルであるグローバルテクスチャをすべてクリア。

            foreach( var kvp in this.グローバルテクスチャリスト )
            {
                if( kvp.Value.tex is Texture2D tex2d && tex2d.Description.BindFlags.HasFlag( BindFlags.RenderTarget ) )
                {
                    using( var rtv = new RenderTargetView( d3ddc.Device, tex2d ) )
                    {
                        d3ddc.ClearRenderTargetView( rtv, kvp.Value.clearColor );
                    }
                }
            }


            // DeviceContext の設定とバックアップ。

            var orgRasterizerState = d3ddc.Rasterizer.State;
            var orgBlendState = d3ddc.OutputMerger.BlendState;


            // リストの先頭から順番にパスを描画。

            for( int i = 0; i < this.パスリスト.Count; i++ )
            {
                this.パスリスト[ i ].描画する( 現在時刻sec, d3ddc, this._GlobalParameters );
            }


            // DeviceContext の復元。

            d3ddc.Rasterizer.State = orgRasterizerState;
            d3ddc.OutputMerger.BlendState = orgBlendState;
        }


        protected GlobalParameters _GlobalParameters = new GlobalParameters();

        protected Device _D3DDevice;

        protected Texture2D _既定のDepthStencil;

        protected Texture2D _既定のRenderTarget;

        protected DepthStencilView _既定のDepthStencilView;

        protected RenderTargetView _既定のRenderTargetView;
    }
}
