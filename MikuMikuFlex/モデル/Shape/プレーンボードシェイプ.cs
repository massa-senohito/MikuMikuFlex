using System;
using System.Collections.Generic;
using MikuMikuFlex.スプライト;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.モデル.Shape
{
    /// <summary>
    ///     長方形または正方形のシェイプ。
    /// </summary>
	public class プレーンボードシェイプ : IDrawable
	{
        public bool 表示中 { get; set; }

        public string ファイル名 { get; private set; }

        public int サブセット数 { get; private set; }

        public int 頂点数 { get; private set; }

        public モデル状態 モデル状態 { get; private set; }

        public Vector4 セルフシャドウ色 { get; set; }

        public Vector4 地面影色 { get; set; }

        public SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; set; }


        public プレーンボードシェイプ( WeakReference<ShaderResourceView> resourceView ) 
            : this( resourceView, new Vector2( 200, 200 ) )
        {
        }

		public プレーンボードシェイプ( WeakReference<ShaderResourceView> resourceView, Vector2 プレーンボードのサイズ )
		{
			_シェーダーリソースビュー = resourceView;

			表示中 = true;

			スプライトの描画に利用するエフェクト = CGHelper.EffectFx5を作成するFromResource( "MikuMikuFlex.Resource.Shader.SpriteShader.fx", RenderContext.Instance.DeviceManager.D3DDevice );

			VertexInputLayout = new InputLayout(
                RenderContext.Instance.DeviceManager.D3DDevice,
                スプライトの描画に利用するエフェクト.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Description.Signature,
                スプライトの頂点レイアウト.InputElements );

			_描画パス = スプライトの描画に利用するエフェクト.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 );


            // 頂点リストを作成

            float width = プレーンボードのサイズ.X / 2f;
            float height = プレーンボードのサイズ.Y / 2f;

			var 頂点リスト = new List<byte>();

            // 三角形１
			CGHelper.AddListBuffer( new Vector3( -width, height, 0 ), 頂点リスト );    // x, y, z
			CGHelper.AddListBuffer( new Vector2( 0, 0 ), 頂点リスト );                 // u, v         以下同
            CGHelper.AddListBuffer( new Vector3( width, height, 0 ), 頂点リスト );
			CGHelper.AddListBuffer( new Vector2( 1, 0 ), 頂点リスト );
            CGHelper.AddListBuffer( new Vector3( -width, -height, 0 ), 頂点リスト );
			CGHelper.AddListBuffer( new Vector2( 0, 1 ), 頂点リスト );
            
            // 三角形２
            CGHelper.AddListBuffer( new Vector3( width, height, 0 ), 頂点リスト );
			CGHelper.AddListBuffer( new Vector2( 1, 0 ), 頂点リスト );
            CGHelper.AddListBuffer( new Vector3( width, -height, 0 ), 頂点リスト );
			CGHelper.AddListBuffer( new Vector2( 1, 1 ), 頂点リスト );
            CGHelper.AddListBuffer( new Vector3( -width, -height, 0 ), 頂点リスト );
			CGHelper.AddListBuffer( new Vector2( 0, 1 ), 頂点リスト );


            // 頂点リストから頂点バッファを作成

            using( DataStream ds = DataStream.Create( 頂点リスト.ToArray(), true, true ) )
			{
				var bufDesc = new BufferDescription() {
					BindFlags = BindFlags.VertexBuffer,
					SizeInBytes = (int) ds.Length
                };

                D3D頂点バッファ = new SharpDX.Direct3D11.Buffer( RenderContext.Instance.DeviceManager.D3DDevice, ds, bufDesc );
			}

			モデル状態 = new Transformer基本実装();
			モデル状態.倍率 = new Vector3( 0.2f );     // さいしょっから 0.2 倍？
		}

		public void Dispose()
		{
			D3D頂点バッファ?.Dispose();
            D3D頂点バッファ = null;

            VertexInputLayout?.Dispose();
            VertexInputLayout = null;
        }

        public void 描画する()
        {
            if( !_シェーダーリソースビュー.TryGetTarget( out ShaderResourceView srv ) )
                return; 

            スプライトの描画に利用するエフェクト.GetVariableBySemantic( "WORLDVIEWPROJECTION" ).AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールドビュー射影行列を作成する( this ) );
            スプライトの描画に利用するエフェクト.GetVariableBySemantic( "SPRITETEXTURE" ).AsShaderResource().SetResource( srv );

            RenderContext.Instance.DeviceManager.D3DDeviceContext.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( D3D頂点バッファ, スプライトの頂点レイアウト.SizeInBytes, 0 ) );
            RenderContext.Instance.DeviceManager.D3DDeviceContext.InputAssembler.InputLayout = VertexInputLayout;
            RenderContext.Instance.DeviceManager.D3DDeviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            _描画パス.Apply( RenderContext.Instance.DeviceManager.D3DDeviceContext );

            RenderContext.Instance.DeviceManager.D3DDeviceContext.Draw( 12, 0 );
        }

        public void 更新する()
        {
        }


        private EffectPass _描画パス;

        private WeakReference<ShaderResourceView> _シェーダーリソースビュー;

        private InputLayout VertexInputLayout;

        private Effect スプライトの描画に利用するエフェクト;
    }
}
