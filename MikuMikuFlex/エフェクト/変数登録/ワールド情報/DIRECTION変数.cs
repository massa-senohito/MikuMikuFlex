using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.ワールド情報
{
	internal sealed class DIRECTION変数 : ワールド情報変数
	{
        public override string セマンティクス => "DIRECTION";


        private DIRECTION変数( Object種別 type )
            : base( type )
		{
		}

		public DIRECTION変数()
		{
		}

        protected override 変数管理 GetSubscriberInstance( Object種別 type )
        {
            return new DIRECTION変数( type );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
            // 変数には、登録値に関係なく、現在の RenderContext の内容を反映する。

            switch( Object )
            {
                case Object種別.カメラ:
                    {
                        Vector3 方向 = Vector3.Normalize(
                            RenderContext.Instance.行列管理.ビュー行列管理.カメラの注視点 -
                            RenderContext.Instance.行列管理.ビュー行列管理.カメラの位置 );
                        変数.AsVector().Set( 方向 );
                    }
                    break;

                case Object種別.ライト:
                    変数.AsVector().Set( RenderContext.Instance.照明行列管理.照明の方向 );
                    break;

                default:
                    throw new System.NotSupportedException();
            }
		}
	}
}
