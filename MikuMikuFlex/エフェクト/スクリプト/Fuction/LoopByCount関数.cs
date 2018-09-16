using System;
using MikuMikuFlex.モデル;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクトスクリプト
{
    /// <summary>
    ///     指定回数だけループする関数？
    /// </summary>
	internal class LoopByCount関数 : 関数
    {
        public override string 名前 => "LoopByCount";


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
            var func = new LoopByCount関数();

            func._runtime = runtime;

            if( string.IsNullOrWhiteSpace( value ) )
                throw new InvalidMMEEffectShader例外( "LoopByCount=;（空文字列）は指定できません。int,float,boolいずれかの変数名を伴う必要があります。" );

            EffectVariable d3dVariable = effect.D3DEffect.GetVariableByName( value );
            string typeName = d3dVariable.TypeInfo.Description.TypeName.ToLower();

            int loopCount = 0;

            switch( typeName )
            {
                case "bool":
                case "int":
                    loopCount = d3dVariable.AsScalar().GetInt();
                    break;

                case "float":
                    loopCount = (int) d3dVariable.AsScalar().GetFloat();
                    break;

                default:
                    throw new InvalidMMEEffectShader例外( "LoopByCountに指定できる変数の型はfloat,int,boolのいずれかです。" );
            }

            func._ループ回数 = loopCount;

            return func;
        }

        public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
        {
            _runtime.LoopBegins.Push( _runtime.実行するファンクションのリスト.Count );
            _runtime.LoopCounts.Push( 0 );
            _runtime.LoopEndCount.Push( _ループ回数 );
        }


        private ScriptRuntime _runtime;

        private int _ループ回数;
    }
}
