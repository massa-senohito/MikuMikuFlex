using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class オブジェクトパス : パス, IDisposable
    {

        // 生成と終了


        public オブジェクトパス( PMXモデル model )
        {
            this._オブジェクト = model;
        }

        public virtual void Dispose()
        {
            this._深度ステンシルビュー?.Dispose();

            foreach( var rt in this._レンダーターゲットビュー )
                rt?.Dispose();

            this._オブジェクト = null;    // Disposeしない
        }


        
        // 設定


        public void RenderTargetを設定する( Device d3dDevice, Texture2D depthStencil, Texture2D[] renderTargets )
        {
            // 以前のビューを解放する。

            this._深度ステンシルビュー?.Dispose();

            foreach( var rt in this._レンダーターゲットビュー )
                rt?.Dispose();


            // 新しくビューを生成する。

            this._深度ステンシルビュー = new DepthStencilView( d3dDevice, depthStencil );

            for( int i = 0; i < renderTargets.Length && i < this._レンダーターゲットビュー.Length; i++ )
                this._レンダーターゲットビュー[ i ] = ( null != renderTargets[ i ] ) ? new RenderTargetView( d3dDevice, renderTargets[ i ] ) : null;
        }



        // 描画


        public override void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // パスに指定されたターゲットに変更。
            d3ddc.OutputMerger.SetTargets( this._深度ステンシルビュー, this._レンダーターゲットビュー );

            this._オブジェクト.描画する( 現在時刻sec, d3ddc, globalParameters );
        }



        // private, protected


        protected PMXモデル _オブジェクト = null;

        protected DepthStencilView _深度ステンシルビュー;

        protected RenderTargetView[] _レンダーターゲットビュー = new RenderTargetView[ OutputMergerStage.SimultaneousRenderTargetCount ];
    }
}
