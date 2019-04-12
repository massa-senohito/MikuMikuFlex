using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class エフェクトパス : パス
    {

        // 生成と終了


        public エフェクトパス( Device d3dDevice, IPostEffect postEffect )
        {
            this._PostEffect = postEffect;

            #region " グローバルパラメータ定数バッファを作成する。"
            //----------------
            this._GlobalParameters定数バッファ = new SharpDX.Direct3D11.Buffer(
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
            this._非順序アクセスビュー?.Dispose();

            foreach( var kvp in this._シェーダーリソースビュー )
                kvp.Value.Dispose();

            this._PostEffect = null;    // Disposeはしない

            this._GlobalParameters定数バッファ?.Dispose();
        }



        // 設定


        public void リソースをバインドする( Device d3dDevice, Texture2D unorderedAccess, params (int slot,Texture2D tex)[] shaderResources )
        {
            // 以前のビューを解放する。

            this._非順序アクセスビュー?.Dispose();

            foreach( var kvp in this._シェーダーリソースビュー )
                kvp.Value.Dispose();

            this._シェーダーリソースビュー.Clear();


            // 新しくビューを生成する。

            this._非順序アクセスビュー = new UnorderedAccessView( d3dDevice, unorderedAccess );

            foreach( var pair in shaderResources )
                this._シェーダーリソースビュー[ pair.slot ] = new ShaderResourceView( d3dDevice, pair.tex );
        }



        // 描画


        public override void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            #region " グローバルパラメータを定数バッファ b0 へ転送する。"
            //----------------
            d3ddc.UpdateSubresource( ref globalParameters, this._GlobalParameters定数バッファ );

            d3ddc.ComputeShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
            //----------------
            #endregion

            #region " 入出力ビューをシェーダーステージに設定する。"
            //----------------
            foreach( var kvp in this._シェーダーリソースビュー )
                d3ddc.ComputeShader.SetShaderResource( kvp.Key, kvp.Value );

            d3ddc.ComputeShader.SetUnorderedAccessView( 0, this._非順序アクセスビュー );
            //----------------
            #endregion

            this._PostEffect.Blit( d3ddc );
        }



        // private, protected


        protected IPostEffect _PostEffect;

        protected UnorderedAccessView _非順序アクセスビュー;

        protected Dictionary<int, ShaderResourceView> _シェーダーリソースビュー = new Dictionary<int, ShaderResourceView>();

        protected SharpDX.Direct3D11.Buffer _GlobalParameters定数バッファ;
    }
}
