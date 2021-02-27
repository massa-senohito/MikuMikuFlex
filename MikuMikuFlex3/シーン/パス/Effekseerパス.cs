using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class EffekseerPass : Pass
    {

        // 生成と終了


        public EffekseerPass( Effekseer effekseer )
        {
            this._Effekseer = effekseer;
        }

        public override void Dispose()
        {
            this.FreeResources();

            this._Effekseer = null;  // Dispose しない
        }



        // Setting


        /// <summary>
        ///     Effekseer エフェクトの描画に必要なリソースを設定する。
        /// </summary>
        /// <remarks>
        ///     あたえられた深度ステンシルリソースとレンダーターゲットリソース（１～８個）から、ビューを生成する。
        ///     ビューのみを保持し、リソース自体は保持しない。
        /// </remarks>
        public void BindResources( Device d3dDevice, Texture2D depthStencil, params Texture2D[] renderTargets )
        {
            // 古いリソースを解放する。

            this.DepthStencilView?.Dispose();
            foreach( var rt in this.RenderTargetViews )
                rt?.Dispose();

            
            // 新しい深度ステンシルビューとレンダーターゲットビューを生成する。

            this.DepthStencilView = new DepthStencilView( d3dDevice, depthStencil );
            for( int i = 0; i < renderTargets.Length && i < this.RenderTargetViews.Length; i++ )
                this.RenderTargetViews[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }

        public void FreeResources()
        {
            this.DepthStencilView?.Dispose();
            foreach( var rt in this.RenderTargetViews )
                rt?.Dispose();
        }



        // Drawing


        /// <summary>
        ///     すべてのEffekseerエフェクトを描画する。
        /// </summary>
        /// <remarks>
        ///     描画先は、<see cref="BindResources(Device, Texture2D, Texture2D[])"/> で設定された深度ステンシルとレンダーターゲット（のビュー）である。
        ///     これらは描画の前に設定されていること。
        /// </remarks>
        public override void Draw( double CurrentTimesec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // 現在時刻から経過フレーム数を算出し、Proceed。

            float NumberOfElapsedFrames = 0f;
            if( 0.0 <= this._LastDrawingTimesec )
            {
                double ElapsedTimesec = CurrentTimesec - this._LastDrawingTimesec;
                NumberOfElapsedFrames = (float)( ElapsedTimesec * 60.0 );    // Effekseer のフレームは 60fps 基準
            }
            this._LastDrawingTimesec = CurrentTimesec;

            this._Effekseer.Proceed( NumberOfElapsedFrames );


            // Draw。

            d3ddc.OutputMerger.SetTargets( this.DepthStencilView, this.RenderTargetViews );

            this._Effekseer.Draw( d3ddc, globalParameters );
        }



        // ローカル


        internal DepthStencilView DepthStencilView;

        internal RenderTargetView[] RenderTargetViews = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];

        private Effekseer _Effekseer;

        private double _LastDrawingTimesec = -1.0;
    }
}
