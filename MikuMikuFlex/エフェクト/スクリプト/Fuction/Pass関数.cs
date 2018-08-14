using System;
using MMF.モデル;

namespace MMF.エフェクト.Script.Function
{
    /// <summary>
    ///     指定された名前のパスを実行する。
    /// </summary>
	internal class Pass関数 : 関数
	{
		public override string 名前 => "Pass";


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
			var func = new Pass関数();

			if( technique == null )
				throw new InvalidMMEEffectShader例外( "スクリプト内でPassを利用できるのはテクニックに適用されたスクリプトのみです。パスのスクリプトでは実行できません。" );

            // このメソッドの引数のパスは使わない。

			if( !technique.パスリスト.ContainsKey( value ) )
				throw new InvalidMMEEffectShader例外( $"スクリプトで指定されたテクニック中では指定されたパス\"{value}\"は見つかりませんでした。(スペルミス?)" );

			func._実行対象のパス = technique.パスリスト[ value ];

			return func;
		}

		public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
		{
			_実行対象のパス.適用して描画する( drawAction, ipmxSubset );
		}


        private パス _実行対象のパス;
    }
}
