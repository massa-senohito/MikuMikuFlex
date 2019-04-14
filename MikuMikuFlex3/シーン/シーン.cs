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

        public List<カメラ> カメラリスト { get; protected set; } = new List<カメラ>();

        public カメラ 選択中のカメラ { get; set; }

        public List<照明> 照明リスト { get; protected set; } = new List<照明>();

        public List<パス> パスリスト { get; protected set; } = new List<パス>();

        public Dictionary<object, (Resource tex, Color4 clearColor)> グローバルテクスチャリスト { get; protected set; } = new Dictionary<object, (Resource tex, Color4 clearColor)>();


        protected シーン()
        {
            this.パスリスト = new List<パス>();
        }

        public シーン( float width, float height )
            : this()
        {
            this.ViewportSize = new Size2F( width, height );
        }

        public シーン( System.Drawing.Size size )
            : this()
        {
            this.ViewportSize = new Size2F( size.Width, size.Height );
        }

        public virtual void Dispose()
        {
            foreach( var kvp in this.グローバルテクスチャリスト )
                kvp.Value.tex?.Dispose();
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

            this._GlobalParameters.ViewMatrix = this.選択中のカメラ.ビュー行列を取得する();
            this._GlobalParameters.ViewMatrix.Transpose();
            this._GlobalParameters.ProjectionMatrix = this.選択中のカメラ.射影行列を取得する();
            this._GlobalParameters.ProjectionMatrix.Transpose();
            this._GlobalParameters.CameraPosition = new Vector4( this.選択中のカメラ.位置, 0f );
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


            // リストの先頭から順番にパスを描画。

            for( int i = 0; i < this.パスリスト.Count; i++ )
            {
                this.パスリスト[ i ].描画する( 現在時刻sec, d3ddc, this._GlobalParameters );
            }
        }

        protected GlobalParameters _GlobalParameters = new GlobalParameters();
    }
}
