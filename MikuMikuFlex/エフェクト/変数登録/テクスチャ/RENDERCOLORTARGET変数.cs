using System;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MMF.エフェクト.変数管理.テクスチャ
{
	internal class RENDERCOLORTARGET変数 : 変数管理, IDisposable
	{
		public override string セマンティクス => "RENDERCOLORTARGET";

		public override 変数型[] 使える型の配列 => new[] { 変数型.Texture2D, 変数型.Texture };


		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			var subscriber = new RENDERCOLORTARGET変数();
			_variableName = variable.Description.Name;

			int width, height, depth, mip;
            Format textureformat, viewFormat, resourceFormat;
			テクスチャのアノテーション解析.解析する( variable, Format.R8G8B8A8_UNorm, new Vector2( 1f, 1f ), false, out width, out height, out depth, out mip, out textureformat, out viewFormat, out resourceFormat );

            if( depth != -1 )
				throw new InvalidMMEEffectShader例外( string.Format( "RENDERCOLORTARGETの型はTexture2Dである必要があるためアノテーション「int depth」は指定できません。" ) );

            // テクスチャの作成
			var tex2DDesc = new Texture2DDescription() {
				ArraySize = 1,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				Format = textureformat,
				Height = height,
				Width = width,
				MipLevels = mip,
				OptionFlags = ResourceOptionFlags.None,
				SampleDescription = new SampleDescription( 1, 0 ),
				Usage = ResourceUsage.Default
			};
            subscriber._renderTexture = new Texture2D( RenderContext.Instance.DeviceManager.D3DDevice, tex2DDesc );

            // レンダーリソースビューの作成
            var viewDesc = new RenderTargetViewDescription() {
                Format = viewFormat,
                Dimension = RenderTargetViewDimension.Texture2D,
            };
			subscriber._renderTarget = new RenderTargetView( RenderContext.Instance.DeviceManager.D3DDevice, subscriber._renderTexture, viewDesc );

            // シェーダリソースビューの作成
            var shaderDesc = new ShaderResourceViewDescription() {
                Format = resourceFormat,
                Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource() {
                    MipLevels = 1,
                    MostDetailedMip = 0,
                },
            };
            subscriber._shaderResource = new ShaderResourceView( RenderContext.Instance.DeviceManager.D3DDevice, subscriber._renderTexture, shaderDesc );


            effect.レンダーターゲットビューのマップ.Add( _variableName, subscriber._renderTarget );

			return subscriber;
		}

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsShaderResource().SetResource( _shaderResource );
		}

		public void Dispose()
		{
			_shaderResource?.Dispose();
            _shaderResource = null;

            _renderTarget?.Dispose();
            _renderTarget = null;

            _renderTexture?.Dispose();
            _renderTexture = null;
        }


        private RenderTargetView _renderTarget;

        private ShaderResourceView _shaderResource;

        private Texture2D _renderTexture;

        private string _variableName;
    }
}
