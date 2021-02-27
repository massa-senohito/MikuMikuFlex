using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;  
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class Scene : IDisposable
    {
        public Size2F ViewportSize
        {
            get => new Size2F( this._GlobalParameters.ViewportSize.X, this._GlobalParameters.ViewportSize.Y );
            set => this._GlobalParameters.ViewportSize = new Vector2( value.Width, value.Height );
        }

        protected List<Camera> CameraList { get; private protected set; } = new List<Camera>();

        public Camera SelectedCamera { get; set; }
        
        protected List<Illumination> LightingList { get; private protected set; } = new List<Illumination>();

        protected List<Pass> PassList { get; private protected set; } = new List<Pass>();

        public Dictionary<object, (Resource tex, Color4 clearColor)> GlobalTextureList { get; protected set; } = new Dictionary<object, (Resource tex, Color4 clearColor)>();


        public Scene( Device d3dDevice, Texture2D depthStencil, Texture2D renderTarget )
        {
            this._D3DDevice = d3dDevice;
            this.CreateResourcesThatDependOnTheSwapChain( d3dDevice, depthStencil, renderTarget );
            this.PassList = new List<Pass>();
        }

        public virtual void Dispose()
        {
            this.CameraList = null;      // Dispose不要
            this.SelectedCamera = null;    //
            this.LightingList = null;        //

            this.FreeUpResourcesThatDependOnTheSwapChain();

            foreach( var pass in this.PassList )
                pass.Dispose();
            this.PassList = null;

            foreach( var kvp in this.GlobalTextureList )
                kvp.Value.tex?.Dispose();


            this._D3DDevice = null;     // Dispose はしない
        }

        public void CreateResourcesThatDependOnTheSwapChain( Device d3dDevice, Texture2D depthStencil, Texture2D renderTarget )
        {
            this.ViewportSize = new Size2F( renderTarget.Description.Width, renderTarget.Description.Height );

            this._DefaultDepthStencil = depthStencil;
            this._DefaultRenderTarget = renderTarget;
            this._DefaultDepthStencilView = new DepthStencilView( d3dDevice, depthStencil );
            this._DefaultRenderTargetView = new RenderTargetView( d3dDevice, renderTarget );

            foreach( var pass in this.PassList )
                this._BindResources( pass );
        }

        public void FreeUpResourcesThatDependOnTheSwapChain()
        {
            foreach( var pass in this.PassList )
                this._FreeResources( pass );

            this._DefaultDepthStencil = null;   // Dispose はしない
            this._DefaultRenderTarget = null;   // Dispose はしない
            this._DefaultDepthStencilView?.Dispose();
            this._DefaultRenderTargetView?.Dispose();
        }


        public void ToAdd( Pass pass )
        {
            this.PassList.Add( pass );

            this._BindResources( pass );
        }

        public void ToAdd( PMXModel model )
        {
            var modelPass = new PMXModelPath( model );
            this.ToAdd( modelPass );
        }

        public void ToAdd( Effekseer effekseer )
        {
            var effekseerPass = new EffekseerPass( effekseer );
            this.ToAdd( effekseerPass );
        }

        public void ToAdd( Camera camera )
        {
            this.CameraList.Add( camera );
        }

        public void ToAdd( Illumination light )
        {
            this.LightingList.Add( light );
        }


        public Texture2D CreateAGlobalTexture( Device d3dDevice, object key, Texture2DDescription desc, Color4? clearColor = null )
        {
            var tex2d = new Texture2D( d3dDevice, desc );

            if( this.GlobalTextureList.ContainsKey( key ) )
            {
                this.GlobalTextureList[ key ].tex.Dispose();
                this.GlobalTextureList.Remove( key );
            }

            this.GlobalTextureList[ key ] = (tex2d, (clearColor.HasValue ? clearColor.Value : Color4.Black ));

            return tex2d;
        }


        public void Draw( double CurrentTimesec, DeviceContext d3ddc )
        {
            // カメラを進行する。

            if( 0 == this.CameraList.Count )
                throw new Exception( "NoCameraSetInTheScene。" );
            if( 0 == this.LightingList.Count )
                throw new Exception( "NoLightingIsSetInTheScene。" );

            this.SelectedCamera = this.CameraList[ 0 ];
            this.SelectedCamera.Update( CurrentTimesec );


            // GlobalParameters の設定（シーン単位）

            this._GlobalParameters.ViewMatrix = this.SelectedCamera.ViewTransformationMatrix;
            this._GlobalParameters.ViewMatrix.Transpose();
            this._GlobalParameters.ProjectionMatrix = this.SelectedCamera.HomographicTransformationMatrix;
            this._GlobalParameters.ProjectionMatrix.Transpose();
            this._GlobalParameters.CameraPosition = new Vector4( this.SelectedCamera.CameraPosition, 0f );
            this._GlobalParameters.Light1Direction = new Vector4( this.LightingList[ 0 ].IrradiationDirection, 0f );


            // レンダーターゲットと深度ステンシルであるグローバルテクスチャをすべてクリア。

            foreach( var kvp in this.GlobalTextureList )
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

            for( int i = 0; i < this.PassList.Count; i++ )
            {
                this.PassList[ i ].Draw( CurrentTimesec, d3ddc, this._GlobalParameters );
            }


            // DeviceContext の復元。

            d3ddc.Rasterizer.State = orgRasterizerState;
            d3ddc.OutputMerger.BlendState = orgBlendState;
        }


        protected GlobalParameters _GlobalParameters = new GlobalParameters();

        protected Device _D3DDevice;

        protected Texture2D _DefaultDepthStencil;

        protected Texture2D _DefaultRenderTarget;

        protected DepthStencilView _DefaultDepthStencilView;

        protected RenderTargetView _DefaultRenderTargetView;


        private void _BindResources( Pass pass )
        {
            // PMXモデルの場合、ビューの設定がなければ既定のビューを設定する。
            if( pass is PMXModelPath objPass )
            {
                if( null == objPass.DepthStencilView || 0 == objPass.RenderTargetViews.Where( ( rtv ) => ( null != rtv ) ).Count() )
                    objPass.BindResources( this._D3DDevice, this._DefaultDepthStencil, this._DefaultRenderTarget );
            }
            // Effekseerパスの場合、ビューの設定がなければ既定のビューを設定する。
            else if( pass is EffekseerPass effekseerPass )
            {
                if( null == effekseerPass.DepthStencilView || 0 == effekseerPass.RenderTargetViews.Where( ( rtv ) => ( null != rtv ) ).Count() )
                    effekseerPass.BindResources( this._D3DDevice, this._DefaultDepthStencil, this._DefaultRenderTarget );
            }
        }

        private void _FreeResources( Pass pass )
        {
            if( pass is PMXModelPath objPass )
            {
                objPass.FreeResources();
            }
            else if( pass is EffekseerPass effekseerPass )
            {
                effekseerPass.FreeResources();
            }
        }
    }
}
