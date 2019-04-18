using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class オブジェクトパス : パス
    {

        // 生成と終了


        public オブジェクトパス( PMXモデル model )
        {
            this._オブジェクト = model;
        }

        public override void Dispose()
        {
            this.深度ステンシルビュー?.Dispose();

            foreach( var rt in this.レンダーターゲットビューs )
                rt?.Dispose();

            this._オブジェクト = null;    // Disposeしない
        }


        
        // 設定


        public void リソースをバインドする( Device d3dDevice, Texture2D depthStencil, params Texture2D[] renderTargets )
        {
            // 以前のビューを解放する。

            this.深度ステンシルビュー?.Dispose();

            foreach( var rt in this.レンダーターゲットビューs )
                rt?.Dispose();


            // 新しくビューを生成する。

            this.深度ステンシルビュー = new DepthStencilView( d3dDevice, depthStencil );

            for( int i = 0; i < renderTargets.Length && i < this.レンダーターゲットビューs.Length; i++ )
                this.レンダーターゲットビューs[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }



        // 描画


        public override void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // パスに指定されたターゲットに変更。
            d3ddc.OutputMerger.SetTargets( this.深度ステンシルビュー, this.レンダーターゲットビューs );

            this._オブジェクト.描画する( 現在時刻sec, d3ddc, globalParameters );
        }



        // private, protected


        protected PMXモデル _オブジェクト = null;

        internal DepthStencilView 深度ステンシルビュー;

        internal RenderTargetView[] レンダーターゲットビューs = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
    }
}
