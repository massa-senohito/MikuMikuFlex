using MikuMikuFlex.モーション;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
	internal sealed class ELAPSEDTIME変数 : 時間変数
	{
        public override string セマンティクス => "ELAPSEDTIME";


        private ELAPSEDTIME変数( bool syncInEditMode )
            : base( syncInEditMode )
		{
		}

		public ELAPSEDTIME変数()
		{
		}

        protected override 変数管理 時間変数登録インスタンスを生成して返す( bool syncInEditMode )
        {
            return new ELAPSEDTIME変数( syncInEditMode );
        }

        protected override void 変数を更新する( EffectVariable 変数, モーション管理 モーション )
		{
			if( SyncInEditMode )
			{
				変数.AsScalar().Set( モーション.前回のフレームからの経過時間sec );
			}
			else
			{
				変数.AsScalar().Set( モーション.前回のフレームからの経過時間sec );
			}
		}
	}
}
