using System;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.スクリプト
{
    /// <summary>
    ///     レンダリングターゲットを <see cref="RenderContext"/> に設定する。
    ///     ・RenderColorTarget=(テクスチャ名 or 空白)
    ///     ・RenderColorTarget0=(テクスチャ名 or 空白)
    ///     ・RenderColorTarget1=(テクスチャ名 or 空白)
    ///     ・RenderColorTarget2=(テクスチャ名 or 空白)
    ///     ・RenderColorTarget3=(テクスチャ名 or 空白)
    ///     RenderColorTargetは、RenderColorTarget0の別名である。
    /// </summary>
    /// <remarks>
    ///     通常、RenderDepthStencilTargetコマンドとセットで使用する。
    ///     また、RenderColorTarget1～3は単独で使用することはできず、必ずRenderColorTarget0とセットで使用する。
    ///     
    ///     引数には、RENDERCOLORTARGETセマンティクスで宣言されたtextureパラメータの名前を指定する。
    ///     デフォルトのレンダリングターゲットにリセットする場合は、空白を指定する。
    ///     
    ///     なお、設定されたレンダリングターゲットは、再度これらのコマンドを実行しなければ、
    ///     テクニックの処理完了まで変更されたままである。
    /// </remarks>
	internal class RenderColorTarget関数 : 関数
	{
		public override string 名前 => "RenderColorTarget";


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
			var func = new RenderColorTarget関数();

			if( index < 0 || index > 7 )
                throw new InvalidMMEEffectShader例外( "RenderColorTarget[n](0<=n<=7)のnの制約が満たされていません。" );

			func._index = index;

			if( !string.IsNullOrWhiteSpace( value ) && !effect.レンダーターゲットビューのマップ.ContainsKey( value ) )
				throw new InvalidMMEEffectShader例外( "指定されたRENDERCOLORTARGETの変数は存在しません。" );

            // 右辺が空白だったら、既定のターゲットを設定する。
			if( string.IsNullOrWhiteSpace( value ) )
				func._既定のターゲットを使う = true;

			func._renderTargetView = string.IsNullOrWhiteSpace( value ) ?
                index == 0 ? RenderContext.Instance.描画ターゲットコンテキスト.D3Dレンダーターゲットビュー : null :
                effect.レンダーターゲットビューのマップ[ value ];

			return func;
		}

		public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
		{
			RenderContext.Instance.レンダーターゲット配列[ _index ] =
                _既定のターゲットを使う ? RenderContext.Instance.描画ターゲットコンテキスト.D3Dレンダーターゲットビュー : _renderTargetView;

			RenderContext.Instance.DeviceManager.D3DDeviceContext.OutputMerger.SetTargets( RenderContext.Instance.深度ステンシルターゲット, RenderContext.Instance.レンダーターゲット配列 );
		}


        private int _index;

        private RenderTargetView _renderTargetView;

        private bool _既定のターゲットを使う;
    }
}
