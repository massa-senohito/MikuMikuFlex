using System;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクトスクリプト
{
    /// <summary>
    ///     レンダーターゲットまたは深度ステンシルをクリアする関数。
    /// </summary>
    /// <remarks>
    ///     "Clear=Color[n];" でレンダーターゲットn（n=0～9; 省略値 0）、
    ///     "Clear=Depth;" で深度ステンシルをクリアする。
    /// </remarks>
	internal class Clear関数 : 関数
	{
		public override string 名前 => "Clear";


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
			var func = new Clear関数();

			switch( value )
			{
				case "Color":
                    func._対象 = 対象.Color;
					break;

				case "Depth":
                    func._対象 = 対象.Depth;
					break;

				default:
                    throw new InvalidMMEEffectShader例外( $"Clear={value}が指定されましたが、\"{value}\"は指定可能ではありません。\"Clear\"もしくは\"Depth\"が指定可能です。" );
			}

			func._index = index;

			return func;
		}

		public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
		{
            switch( _対象 )
            {
                case 対象.Color:
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.ClearRenderTargetView(
                        RenderContext.Instance.レンダーターゲット配列[ _index ],
                        RenderContext.Instance.クリア色 );
                    break;

                case 対象.Depth:
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.ClearDepthStencilView(
                        RenderContext.Instance.深度ステンシルターゲット,
                        DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                        RenderContext.Instance.クリア深度,
                        0 );
                    break;
            }
		}


        private enum 対象
        {
            Color,
            Depth,
        }

        private 対象 _対象;

        private int _index;
    }
}
