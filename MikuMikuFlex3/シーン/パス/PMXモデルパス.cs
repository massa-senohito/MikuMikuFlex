using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class PMXModelPath : Pass
    {

        // 生成と終了


        public PMXModelPath( PMXModel model )
        {
            this._Object = model;
        }

        public override void Dispose()
        {
            this.FreeResources();

            this._Object = null;    // Disposeしない
        }



        // Setting


        /// <summary>
        ///     オブジェクトの描画に必要なリソースを設定する。
        /// </summary>
        /// <remarks>
        ///     あたえられた深度ステンシルリソースとレンダーターゲットリソース（１～８個）から、ビューを生成する。
        ///     ビューのみを保持し、リソース自体は保持しない。
        /// </remarks>
        public void BindResources( Device d3dDevice, Texture2D depthStencil, params Texture2D[] renderTargets )
        {
            // 古い深度ステンシルビューとレンダーターゲットビューを解放する。
            this.DepthStencilView?.Dispose();
            if( null != this.RenderTargetViews )
            {
                foreach( var rt in this.RenderTargetViews )
                    rt?.Dispose();
            }

            // 新しい深度ステンシルビューとレンダーターゲットビューを生成する。
            this.DepthStencilView = new DepthStencilView( d3dDevice, depthStencil );
            this.RenderTargetViews = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
            for( int i = 0; i < renderTargets.Length && i < this.RenderTargetViews.Length; i++ )
                this.RenderTargetViews[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }

        public void FreeResources()
        {
            this.DepthStencilView?.Dispose();
            this.DepthStencilView = null;

            if( null != this.RenderTargetViews )
            {
                foreach( var rt in this.RenderTargetViews )
                    rt?.Dispose();
                this.RenderTargetViews = null;
            }
        }



        // Drawing

        /// <summary>
        ///     オブジェクトを描画する。
        /// </summary>
        /// <remarks>
        ///     描画先は、<see cref="BindResources(Device, Texture2D, Texture2D[])"/> で設定された深度ステンシルとレンダーターゲット（のビュー）である。
        ///     これらは描画の前に設定されていること。
        /// </remarks>
        public override void Draw( double CurrentTimesec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // ターゲットをOMステージに設定する。
            d3ddc.OutputMerger.SetTargets( this.DepthStencilView, this.RenderTargetViews );

            this._Object.Draw( CurrentTimesec, d3ddc, globalParameters );
        }



        // ローカル


        protected PMXModel _Object = null;


        internal DepthStencilView DepthStencilView;

        internal RenderTargetView[] RenderTargetViews = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
    }
}
