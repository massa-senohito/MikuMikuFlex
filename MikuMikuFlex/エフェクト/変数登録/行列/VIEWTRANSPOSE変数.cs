using SharpDX;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.行列
{
	internal sealed class VIEWTRANSPOSE変数 : 行列変数
	{
        public override string セマンティクス => "VIEWTRANSPOSE";


        private VIEWTRANSPOSE変数( Object種別 Object )
            : base( Object )
		{
		}

		public VIEWTRANSPOSE変数()
		{
		}

        protected override 変数管理 行列変数登録インスタンスを生成して返す( Object種別 Object )
        {
            return new VIEWTRANSPOSE変数( Object );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
            switch( ターゲットオブジェクト )
            {
                case Object種別.カメラ:
                    行列を登録する( 
                        Matrix.Transpose( 
                            RenderContext.Instance.行列管理.ビュー行列管理.ビュー行列 ), 変数 );
                    break;

                case Object種別.ライト:
                    break;      //  TODO: 未実装
            }
		}
	}
}
