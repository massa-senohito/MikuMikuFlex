using System;
using System.Collections.Generic;
using MikuMikuFlex.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace MikuMikuFlex.モデル.Other
{
	public class DebugDotManager
	{
		public Buffer VertexBuffer { get; private set; }

		public InputLayout VertexLayout { get; private set; }

		public Effect Effect { get; private set; }

		public const float dotlength = 0.8f;

        public EffectPass RenderPass { get; set; }


        public DebugDotManager()
		{
			var listBuffer = new List<byte>();

			CGHelper.AddListBuffer( new Vector3( -dotlength / 2, dotlength / 2, 0 ), listBuffer );
			CGHelper.AddListBuffer( new Vector3( dotlength / 2, dotlength / 2, 0 ), listBuffer );
			CGHelper.AddListBuffer( new Vector3( -dotlength / 2, -dotlength / 2, 0 ), listBuffer );
			CGHelper.AddListBuffer( new Vector3( dotlength / 2, dotlength / 2, 0 ), listBuffer );
			CGHelper.AddListBuffer( new Vector3( dotlength / 2, -dotlength / 2, 0 ), listBuffer );
			CGHelper.AddListBuffer( new Vector3( -dotlength / 2, -dotlength / 2, 0 ), listBuffer );

            VertexBuffer = CGHelper.D3Dバッファを作成する( listBuffer, RenderContext.Instance.DeviceManager.D3DDevice, BindFlags.VertexBuffer );

			Effect = CGHelper.EffectFx5を作成する( "Shader\\debugDot.fx", RenderContext.Instance.DeviceManager.D3DDevice );

			RenderPass = Effect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 );

			VertexLayout = new InputLayout(
                RenderContext.Instance.DeviceManager.D3DDevice,
                Effect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Description.Signature, 
                DebugDotInputLayout.InputElements );
		}

		public void 描画する( List<Vector3> positions, Vector4 color )
		{
			if( positions == null )
                return;

			Effect.GetVariableBySemantic( "COLOR" ).AsVector().Set( color );

			for( int i = 0; i < positions.Count; i++ )
			{
				Vector3 position = positions[ i ];
				Vector3 p2lp = Vector3.Normalize( RenderContext.Instance.行列管理.ビュー行列管理.カメラの位置 - position );
				Vector3 axis = Vector3.Cross( new Vector3( 0, 0, -1 ), p2lp );
				float angle = (float) Math.Acos( Vector3.Dot( new Vector3( 0, 0, -1 ), p2lp ) );
				Quaternion quat = Quaternion.RotationAxis( axis, angle );

				DeviceContext Context = RenderContext.Instance.DeviceManager.D3DDeviceContext;

                Effect.GetVariableBySemantic( "WORLDVIEWPROJECTION" ).AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールドビュー射影行列を作成する( new Vector3( 1f ), quat, position ) );

                Context.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( VertexBuffer, DebugDotInputLayout.SizeInBytes, 0 ) );
				Context.InputAssembler.InputLayout = VertexLayout;
				Context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

				RenderPass.Apply( Context );

				Context.Draw( 6, 0 );
			}
		}
	}
}
