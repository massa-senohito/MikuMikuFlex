using System.Collections.Generic;
using System.Drawing;
using MikuMikuFlex.DeviceManagement;
using MikuMikuFlex.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MikuMikuFlex.モデル.Shape
{
	public abstract class シェイプ : IDrawable, HitTestable
	{
        public bool 表示中 { get; set; }

        public abstract string ファイル名 { get; }

        public int サブセット数 { get; private set; }

        public abstract int 頂点数 { get; }

        public モデル状態 モデル状態 { get; private set; }

        public Vector4 セルフシャドウ色 { get; set; }

        public Vector4 地面影色 { get; set; }


        public シェイプ( Vector4 色 )
		{
			_color = 色;
            表示中 = true;
			サブセット数 = 1;
			モデル状態 = new Transformer基本実装();
		}

		public void 初期化する()
		{
            // シェイプシェーダーを作成する。

            _D3DEffect = CGHelper.EffectFx5を作成するFromResource( @"MMF.Resource.Shader.ShapeShader.fx", RenderContext.Instance.DeviceManager.D3DDevice );


            // 頂点リストを作成し、それをもとに頂点バッファを作成する。

			var 頂点リスト = new List<Vector4>();
			InitializePositions( 頂点リスト );
			_D3D頂点バッファ = CGHelper.D3Dバッファを作成する( 頂点リスト, RenderContext.Instance.DeviceManager.D3DDevice, BindFlags.VertexBuffer );


            // インデックスバッファを作成する。

            var builder = new インデックスバッファBuilder();
			InitializeIndex( builder );
            _D3Dインデックスバッファ = builder.インデックスバッファを作成する();


            // 入力レイアウトを作成する。

            _D3D入力レイアウト = new InputLayout(
                RenderContext.Instance.DeviceManager.D3DDevice, 
                _D3DEffect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Description.Signature,
                シェイプ用入力エレメント.VertexElements );
		}

        public void Dispose()
        {
            _D3Dインデックスバッファ?.Dispose();
            _D3Dインデックスバッファ = null;

            _D3D頂点バッファ?.Dispose();
            _D3D頂点バッファ = null;

            _D3DEffect?.Dispose();
            _D3DEffect = null;

            _D3D入力レイアウト?.Dispose();
            _D3D入力レイアウト = null;
        }

		public void 描画する()
		{
			var d3dContext = RenderContext.Instance.DeviceManager.D3DDevice.ImmediateContext;

            // エフェクトの変数の設定
			_D3DEffect.GetVariableBySemantic( "COLOR" ).AsVector().Set( _color );
            _D3DEffect.GetVariableBySemantic( "WORLDVIEWPROJECTION" ).AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールドビュー射影行列を作成する( this ) );

            // 入力アセンブラの設定
            d3dContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            d3dContext.InputAssembler.InputLayout = _D3D入力レイアウト;
			d3dContext.InputAssembler.SetIndexBuffer( _D3Dインデックスバッファ, Format.R32_UInt, 0 );
			d3dContext.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( _D3D頂点バッファ, シェイプ用入力エレメント.SizeInBytes, 0 ) );

            // エフェクトのテクニックを適用
            _D3DEffect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Apply( d3dContext );

            // シェイプを描画
            d3dContext.DrawIndexed( 頂点数, 0, 0 );
		}

		public void 更新する()
		{

		}

		public void RenderHitTestBuffer( float 色 )
		{
			var d3dContext = RenderContext.Instance.DeviceManager.D3DDevice.ImmediateContext;

			d3dContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

			_D3DEffect.GetVariableBySemantic( "COLOR" ).AsVector().Set( new Vector4( 色, 0, 0, 0 ) );
            _D3DEffect.GetVariableBySemantic( "WORLDVIEWPROJECTION" ).AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールドビュー射影行列を作成する( this ) );

			d3dContext.InputAssembler.InputLayout = _D3D入力レイアウト;
			d3dContext.InputAssembler.SetIndexBuffer( _D3Dインデックスバッファ, Format.R32_UInt, 0 );
			d3dContext.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( _D3D頂点バッファ, シェイプ用入力エレメント.SizeInBytes, 0 ) );

            _D3DEffect.GetTechniqueByIndex( 0 ).GetPassByIndex( 1 ).Apply( d3dContext );

            d3dContext.DrawIndexed( 頂点数, 0, 0 );
		}

		public virtual void HitTestResult( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
		}


        protected Vector4 _color;

        protected abstract void InitializeIndex( インデックスバッファBuilder builder );

        protected abstract void InitializePositions( List<Vector4> positions );


        private InputLayout _D3D入力レイアウト;

        private Effect _D3DEffect;

        private Buffer _D3Dインデックスバッファ;

        private Buffer _D3D頂点バッファ;
    }
}