using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MikuMikuFlex.エフェクト変数管理
{
	public class RENDERDEPTHSTENCILTARGET変数 : 変数管理, IDisposable
	{
        public override string セマンティクス => "RENDERDEPTHSTENCILTARGET";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Texture2D, 変数型.Texture };


		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			var subscriber = new RENDERDEPTHSTENCILTARGET変数();

            int width, height, depth, mip;
            Format textureFormat, viewFormat, resourceFormat;
            テクスチャのアノテーション解析.解析する( variable, Format.D24_UNorm_S8_UInt, new Vector2( 1f, 1f ), false, out width, out height, out depth, out mip, out textureFormat, out viewFormat, out resourceFormat );

            // テクスチャの作成
            var tex2DDesc = new Texture2DDescription() {
				ArraySize = 1,
				BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = textureFormat,
				Height = height,
				Width = width,
				MipLevels = mip,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = new SampleDescription( 1, 0 ),
				Usage = ResourceUsage.Default
			};
			subscriber._depthStencilTexture = new Texture2D( RenderContext.Instance.DeviceManager.D3DDevice, tex2DDesc );

            // レンダーリソースビューの作成
            var viewDesc = new DepthStencilViewDescription() {
                Format = viewFormat,
                Dimension = DepthStencilViewDimension.Texture2D,
            };
            subscriber._depthStencilView = new DepthStencilView( RenderContext.Instance.DeviceManager.D3DDevice, subscriber._depthStencilTexture, viewDesc );

            // シェーダリソースビューの作成
            var shaderDesc = new ShaderResourceViewDescription() {
                Format = resourceFormat,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() {
                    MipLevels = 1,
                    MostDetailedMip= 0,
                },
            };
            subscriber._shaderResource = new ShaderResourceView( RenderContext.Instance.DeviceManager.D3DDevice, subscriber._depthStencilTexture, shaderDesc );


            effect.深度ステンシルビューのマップ.Add( variable.Description.Name, subscriber._depthStencilView );

            return subscriber;
		}

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsShaderResource().SetResource( _shaderResource );
		}

		public void Dispose()
		{
            _depthStencilTexture?.Dispose();
            _depthStencilTexture = null;

			_depthStencilView?.Dispose();
            _depthStencilView = null;

            _shaderResource?.Dispose();
            _shaderResource = null;
        }


        private DepthStencilView _depthStencilView;

        private ShaderResourceView _shaderResource;

        private Texture2D _depthStencilTexture;
    }
}
