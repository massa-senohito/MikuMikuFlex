using System;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.エフェクト.スクリプト
{
    internal class LoopEnd関数 : 関数
    {
        public override string 名前 => "LoopEnd";


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
            var func = new LoopEnd関数();
            return func;
        }

        public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
        {
            // 何もしない
        }

        public override void 次のファンクションへ遷移する( ScriptRuntime runtime )
        {
            int loopCount = runtime.LoopEndCount.Pop();
            int count = runtime.LoopCounts.Pop();
            int begin = runtime.LoopBegins.Pop();

            if( count < loopCount )
            {
                // 継続
                runtime.現在実行中のファンクションのインデックス = begin + 1;    // 最初の部分+1しておく
                runtime.LoopCounts.Push( count + 1 );
                runtime.LoopEndCount.Push( loopCount );
                runtime.LoopBegins.Push( begin );
            }
            else
            {
                //終了
                runtime.現在実行中のファンクションのインデックス++;
            }
        }
    }
}
