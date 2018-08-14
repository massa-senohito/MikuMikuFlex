using System;
using MMF.モデル;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.Script.Function
{
    /// <summary>
    ///     　　深度ステンシルサーフェイス（いわゆるZバッファ）を <see cref="RenderContext"/> に設定する。
    /// </summary>
    /// <remarks>
    ///     　　通常、RenderColorTarget0コマンドとセットで使用する。
    ///     　　
    ///     　　引数には、RENDERDEPTHSTENCILTARGETセマンティクスで宣言されたtextureパラメータの名前を指定する。
    ///     　　デフォルトの深度ステンシルサーフェイスにリセットする場合は、空白を指定する。
    /// </remarks>
	internal class RenderDepthStencilTarget関数 : 関数
	{
		public override string 名前 => "RenderDepthStencilTarget";


        /// <summary>
        ///     自身のインスタンスを作成して返す。
        /// </summary>
        /// <param name="index">連番。ファンクション名の末尾に付与される１桁の数値（0～9）。省略時は 0 。</param>
        /// <param name="value">値。ファンクション名と '=' を挟んだ右辺に記された文字列。</param>
        /// <param name="runtime"></param>
        /// <param name="effect">ファンクションが属しているエフェクト。</param>
        /// <param name="technique">ファンクションが属しているテクニック。パスに属している場合には null を指定。</param>
        /// <param name="pass">ファンクションが属しているパス。テクニックに属している場合には null を指定。</param>
        /// <returns></returns>
		public override 関数 ファンクションインスタンスを作成する( int index, string value, ScriptRuntime runtime, エフェクト effect, テクニック technique, パス pass )
		{
			var func = new RenderDepthStencilTarget関数();

			if( index != 0 )
				throw new InvalidMMEEffectShader例外( "RenderDepthStencilTargetにはインデックス値を指定できません。" );

			if( !string.IsNullOrWhiteSpace( value ) && !effect.深度ステンシルビューのマップ.ContainsKey( value ) )
				throw new InvalidMMEEffectShader例外( "スクリプトに指定された名前の深度ステンシルバッファは存在しません。" );

            // valueが空なら→デフォルトの深度ステンシルバッファ
            // valueがあるなら→その名前のテクスチャ変数から取得

            if( string.IsNullOrWhiteSpace( value ) )
            {
                func._既定のターゲットを使う = true;
                func._DepthStencilView = null;
            }
            else
            {
                func._既定のターゲットを使う = false;
                func._DepthStencilView = effect.深度ステンシルビューのマップ[ value ];    // 名前で検索
            }

			return func;
		}

		public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
		{
			RenderContext.Instance.深度ステンシルターゲット = 
                _既定のターゲットを使う ? RenderContext.Instance.描画ターゲットコンテキスト.深度ステンシルビュー : _DepthStencilView;

			RenderContext.Instance.DeviceManager.D3DDeviceContext.OutputMerger.SetTargets( RenderContext.Instance.深度ステンシルターゲット, RenderContext.Instance.レンダーターゲット配列 );
		}


        private DepthStencilView _DepthStencilView;

        private bool _既定のターゲットを使う;
    }
}
