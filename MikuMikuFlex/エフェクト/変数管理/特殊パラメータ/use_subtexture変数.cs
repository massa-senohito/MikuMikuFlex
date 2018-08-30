using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.特殊パラメータ
{
    /// <summary>
    ///     use_subtexture (bool型)
    ///     サブテクスチャ使用フラグ。
    ///     PMXモデルのサブテクスチャを使用する場合にtrue。
    /// </summary>
    public class use_subtexture変数 : 特殊パラメータ変数
    {
        public override string 変数名 => "use_subtexture";

        public override 変数型 変数型 => 変数型.Bool;


        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            throw new NotImplementedException();    // TODO: 未実装。
        }
    }
}
