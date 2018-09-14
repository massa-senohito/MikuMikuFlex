using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.テクスチャ
{
    internal class CURRENTSCENEDEPTHSTENCIL変数 : 変数管理, IDisposable
    {
        public override string セマンティクス => "CURRENTSCENEDEPTHSTENCIL";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Texture2D };


        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
        {
            var subscriber = new CURRENTSCENEDEPTHSTENCIL変数();

            // この時点ではまだ Texture2D は作成しない。

            return subscriber;
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            // 深度ステンシルビューを取得する。
            RenderContext.Instance.DeviceManager.D3DDeviceContext.OutputMerger.GetRenderTargets( out var depthStencilView );

            using( depthStencilView )
            using( var depthStencilBuffer = depthStencilView.ResourceAs<Texture2D>() )
            {
                #region " 未作成または深度ステンシルバッファといろいろ異なっている場合、Texture2D と ShaderResourceView を作成する。"
                //----------------
                if( null == _Texture2D ||
                    null == _ShaderResourceView ||
                    depthStencilBuffer.Description.Width != _Texture2D.Description.Width ||
                    depthStencilBuffer.Description.Height != _Texture2D.Description.Height ||
                    depthStencilBuffer.Description.Format != _Texture2D.Description.Format )
                {
                    変数.AsShaderResource().SetResource( null );
                    _ShaderResourceView?.Dispose();
                    _Texture2D?.Dispose();

                    // 深度ステンシルバッファと同じサイズ・同じフォーマットのテクスチャを作成する。

                    _Texture2D = new Texture2D(
                        RenderContext.Instance.DeviceManager.D3DDevice,
                        new Texture2DDescription {
                            ArraySize = 1,
                            BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                            CpuAccessFlags = CpuAccessFlags.None,   // CPUからアクセス不要
                            Format = SharpDX.DXGI.Format.R32_Typeless,
                            Width = depthStencilBuffer.Description.Width,
                            Height = depthStencilBuffer.Description.Height,
                            MipLevels = 1,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = new SharpDX.DXGI.SampleDescription( 1, 0 ),
                            Usage = ResourceUsage.Default,
                        } );

                    // テクスチャのシェーダーリソースビューを作成する。

                    _ShaderResourceView = new ShaderResourceView(
                        RenderContext.Instance.DeviceManager.D3DDevice,
                        _Texture2D,
                        new ShaderResourceViewDescription {
                            Format = SharpDX.DXGI.Format.R32_Float,
                            Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                            Texture2D = new ShaderResourceViewDescription.Texture2DResource {
                                MipLevels = 1,
                                MostDetailedMip = 0,
                            },
                        } );


                    // シェーダーリソースビューを変数に設定する。

                    変数.AsShaderResource().SetResource( _ShaderResourceView );
                }
                //----------------
                #endregion


                // 深度ステンシルバッファの内容を Texture2D に複写する。

                RenderContext.Instance.DeviceManager.D3DDeviceContext.CopyResource( depthStencilBuffer, _Texture2D );
            }
        }

        public void Dispose()
        {
            _ShaderResourceView?.Dispose();
            _ShaderResourceView = null;

            _Texture2D?.Dispose();
            _Texture2D = null;
        }


        private Texture2D _Texture2D;

        private ShaderResourceView _ShaderResourceView;
    }
}
