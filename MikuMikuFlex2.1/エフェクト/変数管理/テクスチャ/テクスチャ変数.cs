using System;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace MikuMikuFlex.エフェクト変数管理
{
	internal class テクスチャ変数 : 変数管理, IDisposable
	{
		public override string セマンティクス
			=> throw new InvalidOperationException( "このサブスクライバはセマンティクスを持ちません" );

        public override 変数型[] 使える型の配列
            => new[] { 変数型.Texture, 変数型.Texture2D, 変数型.Texture3D, 変数型.TextureCUBE };


		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			var subscriber = new テクスチャ変数();
            string typeName = variable.TypeInfo.Description.TypeName.ToLower();

            int width, height, depth, mip;
            Format textureFormat, viewFormat, resourceFormat;
			テクスチャのアノテーション解析.解析する( variable, Format.B8G8R8A8_UNorm, Vector2.Zero, true, out width, out height, out depth, out mip, out textureFormat, out viewFormat, out resourceFormat );

            EffectVariable rawTypeVariable = EffectParseHelper.アノテーションを取得する( variable, "ResourceType", "string" );
			EffectVariable rawStringVariable = EffectParseHelper.アノテーションを取得する( variable, "ResourceName", "string" );


            // type を決定

            string type;

            if( rawTypeVariable != null )
			{
				switch( rawTypeVariable.AsString().GetString().ToLower() )
				{
					case "cube":
                        type = ( typeName.Equals( "texturecube" ) ) ? "texturecube" : throw new InvalidMMEEffectShader例外( "ResourceTypeにはCubeが指定されていますが、型がtextureCUBEではありません。" );
						break;

					case "2d":
                        type = ( typeName.Equals( "texture2d" ) || typeName.Equals( "texture" ) ) ? "texture2d" : throw new InvalidMMEEffectShader例外( "ResourceTypeには2Dが指定されていますが、型がtexture2Dもしくはtextureではありません。" );
						break;

					case "3d":
                        type = ( typeName.Equals( "texture3d" ) ) ? "texture3d" : throw new InvalidMMEEffectShader例外( "ResourceTypeには3Dが指定されていますが、型がtexture3dではありません。" );
						break;

					default:
						throw new InvalidMMEEffectShader例外( "認識できないResourceTypeが指定されました。" );
				}
			}
			else
			{
				type = typeName;
			}


            // テクスチャリソースを読み込む

            if( rawStringVariable != null )
			{
				string resourceName = rawStringVariable.AsString().GetString();
				Stream stream;

                switch( type )
				{
					case "texture2d":
						stream = effect.テクスチャなどのパスの解決に利用するローダー.リソースのストリームを返す( resourceName );
						if( stream != null )
							subscriber._resourceTexture = MMFTexture2D.FromStream( RenderContext.Instance.DeviceManager.D3DDevice, stream );
						break;

					case "texture3d":
						stream = effect.テクスチャなどのパスの解決に利用するローダー.リソースのストリームを返す( resourceName );
						if( stream != null )
							subscriber._resourceTexture = MMFTexture3D.FromStream( RenderContext.Instance.DeviceManager.D3DDevice, stream );
						break;

					case "texturecube":
						//TODO CUBEの場合を実装する
						//stream = effectManager.SubresourceLoader.getSubresourceByName(resourceName);
						//subscriber.resourceTexture=.FromStream(context.DeviceManager.Device, stream, (int)stream.Length);
						break;
				}
			}

            // シェーダーリソースビューを割り当て
            subscriber._resourceView = new ShaderResourceView( RenderContext.Instance.DeviceManager.D3DDevice, subscriber._resourceTexture );

            return subscriber;
		}

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
            // シェーダーリソースビューを登録。
			変数.AsShaderResource().SetResource( _resourceView );
		}

		public void Dispose()
		{
            _resourceView?.Dispose();
            _resourceView = null;

            _resourceTexture?.Dispose();
            _resourceTexture = null;
		}


        private ShaderResourceView _resourceView;

        private Resource _resourceTexture;
    }
}
