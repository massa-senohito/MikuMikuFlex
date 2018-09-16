using System;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.スクリプト
{
    internal class LoopGetIndex関数 : 関数
    {
        public override string 名前 => "LoopGetIndex";


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
            var func = new LoopGetIndex関数();

            func._targetVariable = effect.D3DEffect.GetVariableByName( value );
            func._runtime = runtime;

            return func;
        }

        public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
        {
            int count = _runtime.LoopCounts.Pop();

            _targetVariable.AsScalar().Set( count );
            _runtime.LoopCounts.Push( count );
        }


        private EffectVariable _targetVariable;

        private ScriptRuntime _runtime;
    }
}
