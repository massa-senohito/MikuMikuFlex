using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    internal sealed class AMBIENT変数 : 材質変数
    {
        public override string セマンティクス => "AMBIENT";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float3, 変数型.Float4 };


        public AMBIENT変数()
        {
        }

        private AMBIENT変数( ターゲット種別 target, bool isVector3 ) 
            : base( target, isVector3 )
        {
        }

        protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
        {
            return new AMBIENT変数( target, isVector3 );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            switch( ターゲットオブジェクト )
            {
                case ターゲット種別.ジオメトリ:
                    エフェクト変数にベクトルを設定する( new Vector4( 引数.材質.拡散色.X, 引数.材質.拡散色.Y, 引数.材質.拡散色.Z, 0 ), 変数, Vector3である );
                    break;

                case ターゲット種別.ライト:
                    break;      // TODO: 未実装？
            }
        }
    }
}
