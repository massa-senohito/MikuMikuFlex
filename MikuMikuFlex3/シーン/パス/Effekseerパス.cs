using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class Effekseerパス : パス
    {

        // 生成と終了


        public Effekseerパス( Effekseer effekseer )
        {
            this._Effekseer = effekseer;
        }

        public override void Dispose()
        {
            this.リソースを解放する();

            this._Effekseer = null;  // Dispose しない
        }



        // 設定


        /// <summary>
        ///     Effekseer エフェクトの描画に必要なリソースを設定する。
        /// </summary>
        /// <remarks>
        ///     あたえられた深度ステンシルリソースとレンダーターゲットリソース（１～８個）から、ビューを生成する。
        ///     ビューのみを保持し、リソース自体は保持しない。
        /// </remarks>
        public void リソースをバインドする( Device d3dDevice, Texture2D depthStencil, params Texture2D[] renderTargets )
        {
            // 古いリソースを解放する。

            this.深度ステンシルビュー?.Dispose();
            foreach( var rt in this.レンダーターゲットビューs )
                rt?.Dispose();

            
            // 新しい深度ステンシルビューとレンダーターゲットビューを生成する。

            this.深度ステンシルビュー = new DepthStencilView( d3dDevice, depthStencil );
            for( int i = 0; i < renderTargets.Length && i < this.レンダーターゲットビューs.Length; i++ )
                this.レンダーターゲットビューs[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }

        public void リソースを解放する()
        {
            this.深度ステンシルビュー?.Dispose();
            foreach( var rt in this.レンダーターゲットビューs )
                rt?.Dispose();
        }



        // 描画


        /// <summary>
        ///     すべてのEffekseerエフェクトを描画する。
        /// </summary>
        /// <remarks>
        ///     描画先は、<see cref="リソースをバインドする(Device, Texture2D, Texture2D[])"/> で設定された深度ステンシルとレンダーターゲット（のビュー）である。
        ///     これらは描画の前に設定されていること。
        /// </remarks>
        public override void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // 現在時刻から経過フレーム数を算出し、進行する。

            float 経過フレーム数 = 0f;
            if( 0.0 <= this._最後の描画時刻sec )
            {
                double 経過時刻sec = 現在時刻sec - this._最後の描画時刻sec;
                経過フレーム数 = (float)( 経過時刻sec * 60.0 );    // Effekseer のフレームは 60fps 基準
            }
            this._最後の描画時刻sec = 現在時刻sec;

            this._Effekseer.進行する( 経過フレーム数 );


            // 描画する。

            d3ddc.OutputMerger.SetTargets( this.深度ステンシルビュー, this.レンダーターゲットビューs );

            this._Effekseer.描画する( d3ddc, globalParameters );
        }



        // ローカル


        internal DepthStencilView 深度ステンシルビュー;

        internal RenderTargetView[] レンダーターゲットビューs = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];

        private Effekseer _Effekseer;

        private double _最後の描画時刻sec = -1.0;
    }
}
