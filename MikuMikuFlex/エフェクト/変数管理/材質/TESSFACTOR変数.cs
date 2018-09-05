using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.材質
{
    internal class TESSFACTOR変数 : 変数管理
    {
        public override string セマンティクス => "TESSFACTOR";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float };

        public override 更新タイミング 更新タイミング => 更新タイミング.材質ごと;


        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
        {
            return new TESSFACTOR変数();
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            変数.AsScalar().Set( 引数.材質.テッセレーション係数 );
        }
    }
}