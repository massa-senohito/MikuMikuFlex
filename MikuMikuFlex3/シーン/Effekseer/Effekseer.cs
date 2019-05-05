using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class Effekseer : IDisposable
    {

        public EffekseerRendererDX11NET.Manager Manager { get; protected set; }

        public EffekseerRendererDX11NET.Renderer Renderer { get; protected set; }



        // 生成と終了


        public Effekseer( Device d3dDevice )
        {
            // 描画用インスタンスを生成する。
            this.Renderer = EffekseerRendererDX11NET.Renderer.Create( d3dDevice, d3dDevice.ImmediateContext, 2000, Comparison.Less );

            // エフェクト管理用インスタンスを生成する。
            this.Manager = EffekseerRendererDX11NET.Manager.Create( 2000, true );

            // 描画用インスタンスから描画機能を設定する。
            this.Manager.SetSpriteRenderer( this.Renderer.CreateSpriteRenderer() );
            this.Manager.SetRibbonRenderer( this.Renderer.CreateRibbonRenderer() );
            this.Manager.SetRingRenderer( this.Renderer.CreateRingRenderer() );
            this.Manager.SetTrackRenderer( this.Renderer.CreateTrackRenderer() );
            this.Manager.SetModelRenderer( this.Renderer.CreateModelRenderer() );

            // 描画用インスタンスからテクスチャの読込機能を設定する。
            // 独自拡張可能、現在はファイルから読み込んでいる。
            this.Manager.SetTextureLoader( this.Renderer.CreateTextureLoader() );
            this.Manager.SetModelLoader( this.Renderer.CreateModelLoader() );


            // 加算合成用のブレンドステートを作成する。

            var BlendStateAdd = new BlendStateDescription() {
                AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
            };
            BlendStateAdd.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
            BlendStateAdd.RenderTarget[ 0 ].RenderTargetWriteMask = ColorWriteMaskFlags.All;        // RGBA の書き込みマスク。
            // アルファ値のブレンディング設定 ... 特になし
            BlendStateAdd.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;
            BlendStateAdd.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
            BlendStateAdd.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;
            // 色値のブレンディング設定 ... 加算合成
            BlendStateAdd.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;
            BlendStateAdd.RenderTarget[ 0 ].DestinationBlend = BlendOption.One;
            BlendStateAdd.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;
            this._BlendState加算合成 = new BlendState( d3dDevice, BlendStateAdd );


            // ラスタライザステートを作成する。

            this._RasterizerState = new RasterizerState(
                d3dDevice,
                new RasterizerStateDescription {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                } );
        }

        public void Dispose()
        {
            // 先にエフェクト管理用インスタンスを破棄
            this.Manager.Destroy();

            // 次に描画用インスタンスを破棄
            this.Renderer.Destroy();

            // 最後に D3D 関連を破棄
            this._BlendState加算合成?.Dispose();
            this._RasterizerState?.Dispose();
        }



        // 描画


        /// <summary>
        ///     描画する。
        ///     事前に <see cref="Manager"/>.Update() を実行しておくこと。
        /// </summary>
        public void Draw( DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // ビュー変換行列と射影変換行列を設定する。

            this.Manager.SetCoordinateSystem( EffekseerRendererDX11NET.CoordinateSystem.LeftHand );  // 左手座標系を使う（Effekseerの既定値は右手座標系）

            var vm = globalParameters.ViewMatrix;
            vm.Transpose(); // HLSL用に転置されているので元に戻す。
            this.Renderer.SetCameraMatrix( vm );

            var pm = globalParameters.ProjectionMatrix;
            pm.Transpose(); // HLSL用に転置されているので元に戻す。
            this.Renderer.SetProjectionMatrix( pm );


            // D3Dパイプラインを設定する。

            d3ddc.HullShader.Set( null );
            d3ddc.DomainShader.Set( null );
            d3ddc.GeometryShader.Set( null );
            d3ddc.OutputMerger.BlendState = this._BlendState加算合成;
            d3ddc.OutputMerger.DepthStencilState = null;
            d3ddc.Rasterizer.State = this._RasterizerState;

            // 描画する。

            //this._Renderer.BeginRendering();  --> DX9 で必要
            this.Manager.Draw();
            //this._Renderer.EndRendering();    --> DX9 で必要
        }



        // ローカル


        private BlendState _BlendState加算合成;

        private RasterizerState _RasterizerState;
    }
}
