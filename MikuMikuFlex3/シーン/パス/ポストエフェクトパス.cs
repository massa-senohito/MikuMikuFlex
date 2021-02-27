using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class PostEffectPath : Pass
    {

        // 生成と終了


        public PostEffectPath( Device d3dDevice, IPostEffect postEffect )
        {
            this._PostEffect = postEffect;

            #region " CreateAGlobalParameterConstantBuffer。"
            //----------------
            this._GlobalParametersConstantBuffer = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new BufferDescription {
                    SizeInBytes = GlobalParameters.SizeInBytes,
                    BindFlags = BindFlags.ConstantBuffer,
                } );
            //----------------
            #endregion
        }

        public override void Dispose()
        {
            this._OutOfOrderAccessView?.Dispose();

            foreach( var kvp in this._ShaderResourceView )
                kvp.Value.Dispose();

            this._PostEffect = null;    // Disposeはしない

            this._GlobalParametersConstantBuffer?.Dispose();
        }



        // Setting


        /// <summary>
        ///     エフェクトの描画に必要なリソースを設定する。
        /// </summary>
        /// <param name="unorderedAccess">出力リソース。</param>
        /// <param name="shaderResources">入力リソース。</param>
        /// <remarks>
        ///     あたえられた非順序アクセスリソースとシェーダーリソース（０～）から、ビューを生成する。
        ///     ビューのみを保持し、リソース自体は保持しない。
        /// </remarks>
        public void BindResources( Device d3dDevice, Texture2D unorderedAccess, params (int slot,Texture2D tex)[] shaderResources )
        {
            // 古いビューを解放する。
            this._OutOfOrderAccessView?.Dispose();
            foreach( var kvp in this._ShaderResourceView )
                kvp.Value.Dispose();
            this._ShaderResourceView.Clear();

            // 新しくビューを生成する。
            this._OutOfOrderAccessView = new UnorderedAccessView( d3dDevice, unorderedAccess );
            foreach( var pair in shaderResources )
                this._ShaderResourceView[ pair.slot ] = new ShaderResourceView( d3dDevice, pair.tex );
        }



        // Drawing


        public override void Draw( double CurrentTimesec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // グローバルパラメータを定数バッファ b0 へ転送する。
            d3ddc.UpdateSubresource( ref globalParameters, this._GlobalParametersConstantBuffer );
            d3ddc.ComputeShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );

            // OMにビューが設定されてるリソースにはアクセスできないので、とりあえず外す。
            d3ddc.OutputMerger.SetRenderTargets( (DepthStencilView)null, (RenderTargetView)null );

            // 入出力ビューをCSステージに設定する。
            foreach( var kvp in this._ShaderResourceView )
                d3ddc.ComputeShader.SetShaderResource( kvp.Key, kvp.Value );
            d3ddc.ComputeShader.SetUnorderedAccessView( 0, this._OutOfOrderAccessView );

            // エフェクトを実行する。
            this._PostEffect.Blit( d3ddc );
        }



        // ローカル


        protected IPostEffect _PostEffect;


        protected UnorderedAccessView _OutOfOrderAccessView;

        protected Dictionary<int, ShaderResourceView> _ShaderResourceView = new Dictionary<int, ShaderResourceView>();


        protected SharpDX.Direct3D11.Buffer _GlobalParametersConstantBuffer;
    }
}
