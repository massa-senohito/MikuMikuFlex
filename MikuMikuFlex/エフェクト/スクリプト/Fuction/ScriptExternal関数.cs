using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.エフェクト.Script.Function
{
    internal class ScriptExternal関数 : 関数
    {
        public override string 名前 => "ScriptExternal";


        public override 関数 ファンクションインスタンスを作成する( int index, string value, ScriptRuntime runtime, エフェクト effect, テクニック technique, パス pass )
        {
            return new ScriptExternal関数();  // TODO: 未実装
        }

        public override void 実行する( サブセット subset, Action<サブセット> action )
        {
            // TODO: 未実装
        }
    }
}
