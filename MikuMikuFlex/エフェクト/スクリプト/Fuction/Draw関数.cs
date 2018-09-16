using System;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクトスクリプト
{
    /// <summary>
    ///     指定されたターゲットにサブセットの描画を行う。
    ///     パスの string Script 内でのみ使用できる。
    /// </summary>
	internal class Draw関数 : 関数
	{
		public override string 名前 => "Draw";


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
			var func = new Draw関数();

            func._描画に使用するバス = pass ?? throw new InvalidMMEEffectShader例外( "Drawはテクニックのスクリプトに関して適用できません。" );

			switch( value )
			{
				case "Geometry":
                    func._描画するもの = 描画するもの.Geometry;
					break;

				case "Buffer":
                    func._描画するもの = 描画するもの.Buffer;
					break;

				default:
					throw new InvalidMMEEffectShader例外( $"Draw={value}が指定されましたが、Drawに指定できるのは\"Geometry\"または\"Buffer\"です。" );
			}

			if( func._描画するもの == 描画するもの.Geometry && effect.ScriptClass == ScriptClass.Scene )
				throw new InvalidMMEEffectShader例外( "Draw=Geometryが指定されましたが、STANDARDSGLOBALのScriptClassに\"scene\"を指定している場合、これはできません。" );

			if( func._描画するもの == 描画するもの.Buffer && effect.ScriptClass == ScriptClass.Object )
				throw new InvalidMMEEffectShader例外( "Draw=Bufferが指定されましたが、STANDARDSGLOBALのScriptClassに\"object\"を指定している場合、これはできません。" );

			return func;
		}

		public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
		{
            switch( _描画するもの )
            {
                case 描画するもの.Geometry:
                    _描画に使用するバス.適用して描画する( drawAction, ipmxSubset );
                    break;

                case 描画するもの.Buffer:


                    // TODO: Draw=Bufferの場合の処理


                    break;
            }
		}


        private パス _描画に使用するバス;

        private enum 描画するもの
        {
            Geometry,
            Buffer,
        }

        private 描画するもの _描画するもの;
    }
}
