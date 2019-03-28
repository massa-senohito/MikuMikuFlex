using System;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
	/// <summary>
	///     エフェクトのパスを管理するクラス
	/// </summary>
	public class パス
	{
        /// <summary>
        ///     描画に利用されるパス
        /// </summary>
        public EffectPass D3DPass { get; private set; }


        public パス( Effect effect, EffectPass d3dPass )
		{
			this.D3DPass = d3dPass;

            if( !d3dPass.VertexShaderDescription.Variable.IsValid )
			{
				//TODO この場合標準シェーダーの頂点シェーダを利用する
			}

            if( !d3dPass.PixelShaderDescription.Variable.IsValid )
			{
				//TODO この場合標準シェーダーのピクセルシェーダを利用する
			}
		}

        public void 適用して描画する<T>( DeviceContext d3ddc, Action<T> drawAction, T drawArgument )
        {
            this.D3DPass.Apply( d3ddc );
            drawAction?.Invoke( drawArgument );
        }
	}
}
