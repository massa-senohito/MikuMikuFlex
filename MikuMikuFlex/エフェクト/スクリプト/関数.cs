using System;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.エフェクト.Script
{
    /// <summary>
    ///     string Script の中に記されるファンクションの基底クラス。
    /// </summary>
    /// <remarks>
    ///     構文: 名前[連番]=値
    /// </remarks>
	internal abstract class 関数
	{
		public abstract string 名前 { get; }


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
		public abstract 関数 ファンクションインスタンスを作成する( int index, string value, ScriptRuntime runtime, エフェクト effect, テクニック technique, パス pass );

		public abstract void 実行する( サブセット subset, Action<サブセット> action );

		public virtual void 次のファンクションへ遷移する( ScriptRuntime runtime )
		{
			runtime.現在実行中のファンクションのインデックス++;
		}
	}
}
