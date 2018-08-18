using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.材質
{
    // MMM 拡張
    internal class EDGEWIDTH変数 : 材質変数
    {
        //public override string セマンティクス => "EDGETHICKNESS";
        public override string セマンティクス => "EDGEWIDTH";  // MMMv2 に合わせて改名

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float };


        public EDGEWIDTH変数()
        {
        }

        protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
        {
            return new EDGEWIDTH変数();
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            // ターゲットオブジェクトに依存しない
            変数.AsScalar().Set( 引数.材質.エッジ幅 );    // エッジ幅は材質モーフ適用後の値。
        }
    }
}
