using System;
using MikuMikuFlex.モデル;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.Script.Function
{
    /// <summary>
    ///     <see cref="RenderContext"/> のクリア色を設定する関数。
    /// </summary>
    internal class ClearSetColor関数 : 関数
    {
        public override string 名前 => "ClearSetColor";


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
            var func = new ClearSetColor関数();

            func._sourceVariable = effect.D3DEffect.GetVariableByName( value );

            if( func._sourceVariable == null )
                throw new InvalidMMEEffectShader例外( $"ClearSetColor={value};が指定されましたが、変数\"{value}\"は見つかりませんでした。" );

            if( !func._sourceVariable.TypeInfo.Description.TypeName.ToLower().Equals( "float4" ) )
                throw new InvalidMMEEffectShader例外( $"ClearSetColor={value};が指定されましたが、変数\"{value}\"はfloat4型ではありません。" );

            return func;
        }

        public override void 実行する( サブセット ipmxSubset, Action<サブセット> drawAction )
        {
            RenderContext.Instance.クリア色 = new Color4( _sourceVariable.AsVector().GetVector<Color4>() );
        }


        private EffectVariable _sourceVariable;  // 不変なら先に取得スべき？
    }
}
