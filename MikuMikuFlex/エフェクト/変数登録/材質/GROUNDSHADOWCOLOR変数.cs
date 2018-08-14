using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.材質
{
    internal sealed class GROUNDSHADOWCOLOR変数 : 材質変数
    {
        public override string セマンティクス => "GROUNDSHADOWCOLOR";


        private GROUNDSHADOWCOLOR変数( ターゲット種別 target, bool isVector3 )
            : base( target, isVector3 )
        {
        }

        public GROUNDSHADOWCOLOR変数()
        {
        }

        protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
        {
            return new GROUNDSHADOWCOLOR変数( target, isVector3 );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            switch( ターゲットオブジェクト )
            {
                case ターゲット種別.ジオメトリ:
                    エフェクト変数にベクトルを設定する( 引数.材質.地面影色, 変数, Vector3である );
                    break;

                case ターゲット種別.ライト:
                    break;      // TODO: 未実装？
            }

        }
    }
}
