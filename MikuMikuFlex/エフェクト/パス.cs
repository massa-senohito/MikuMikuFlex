using System;
using MikuMikuFlex.エフェクト.スクリプト;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト
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

        /// <summary>
        ///     "string Script" アノテーション
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        ///     スクリプトランタイム。
        /// </summary>
        internal ScriptRuntime ScriptRuntime { get; private set; }


        public パス( エフェクト effect, EffectPass d3dPass )
		{
			D3DPass = d3dPass;

            EffectVariable commandAnnotation = EffectParseHelper.アノテーションを取得する( d3dPass, "Script", "string" );
            Command = ( commandAnnotation == null ) ? "" : commandAnnotation.AsString().GetString();

            if( !d3dPass.VertexShaderDescription.Variable.IsValid )
			{
				//TODO この場合標準シェーダーの頂点シェーダを利用する
			}

            if( !d3dPass.PixelShaderDescription.Variable.IsValid )
			{
				//TODO この場合標準シェーダーのピクセルシェーダを利用する
			}

            ScriptRuntime = new ScriptRuntime( Command, effect, null, this );
		}

		public void 適用して描画する<T>( Action<T> drawAction, T drawArgument )
		{
			if( string.IsNullOrWhiteSpace( ScriptRuntime.ScriptCode ) )
			{
                // このパスに Script が存在しない場合は、そのまま描画する。
				D3DPass.Apply( RenderContext.Instance.DeviceManager.D3DDeviceContext );
				drawAction?.Invoke( drawArgument );
			}
			else
			{
                // このパスに Script が存在する場合は、処理をスクリプトランタイムに任せる。
                ScriptRuntime.実行する( drawAction, drawArgument );
			}
		}
	}
}
