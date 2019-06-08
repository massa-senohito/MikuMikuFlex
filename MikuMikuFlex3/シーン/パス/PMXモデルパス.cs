using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class PMXモデルパス : パス
    {

        // 生成と終了


        public PMXモデルパス( PMXモデル model )
        {
            this._オブジェクト = model;
        }

        public override void Dispose()
        {
            this.リソースを解放する();

            this._オブジェクト = null;    // Disposeしない
        }



        // 設定


        /// <summary>
        ///     オブジェクトの描画に必要なリソースを設定する。
        /// </summary>
        /// <remarks>
        ///     あたえられた深度ステンシルリソースとレンダーターゲットリソース（１～８個）から、ビューを生成する。
        ///     ビューのみを保持し、リソース自体は保持しない。
        /// </remarks>
        public void リソースをバインドする( Device d3dDevice, Texture2D depthStencil, params Texture2D[] renderTargets )
        {
            // 古い深度ステンシルビューとレンダーターゲットビューを解放する。
            this.深度ステンシルビュー?.Dispose();
            if( null != this.レンダーターゲットビューs )
            {
                foreach( var rt in this.レンダーターゲットビューs )
                    rt?.Dispose();
            }

            // 新しい深度ステンシルビューとレンダーターゲットビューを生成する。
            this.深度ステンシルビュー = new DepthStencilView( d3dDevice, depthStencil );
            this.レンダーターゲットビューs = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
            for( int i = 0; i < renderTargets.Length && i < this.レンダーターゲットビューs.Length; i++ )
                this.レンダーターゲットビューs[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }

        public void リソースを解放する()
        {
            this.深度ステンシルビュー?.Dispose();
            this.深度ステンシルビュー = null;

            if( null != this.レンダーターゲットビューs )
            {
                foreach( var rt in this.レンダーターゲットビューs )
                    rt?.Dispose();
                this.レンダーターゲットビューs = null;
            }
        }



        // 描画

        /// <summary>
        ///     オブジェクトを描画する。
        /// </summary>
        /// <remarks>
        ///     描画先は、<see cref="リソースをバインドする(Device, Texture2D, Texture2D[])"/> で設定された深度ステンシルとレンダーターゲット（のビュー）である。
        ///     これらは描画の前に設定されていること。
        /// </remarks>
        public override void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // ターゲットをOMステージに設定する。
            d3ddc.OutputMerger.SetTargets( this.深度ステンシルビュー, this.レンダーターゲットビューs );

            this._オブジェクト.描画する( 現在時刻sec, d3ddc, globalParameters );
        }



        // ローカル


        protected PMXモデル _オブジェクト = null;


        internal DepthStencilView 深度ステンシルビュー;

        internal RenderTargetView[] レンダーターゲットビューs = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
    }
}
