using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.材質
{
    internal sealed class EDGECOLOR変数 : 材質変数
    {
        public override string セマンティクス => "EDGECOLOR";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float3, 変数型.Float4 };


        public EDGECOLOR変数()
        {
        }

        private EDGECOLOR変数( ターゲット種別 target, bool isVector3 ) 
            : base( target, isVector3 )
        {
        }

        protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
        {
            return new EDGECOLOR変数( target, isVector3 );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            switch( ターゲットオブジェクト )
            {
                case ターゲット種別.ジオメトリ:
                    エフェクト変数にベクトルを設定する( 引数.材質.エッジ色, 変数, Vector3である );
                    break;

                case ターゲット種別.ライト:
                    break;      // TODO: 未実装？
            }
        }
    }
}
