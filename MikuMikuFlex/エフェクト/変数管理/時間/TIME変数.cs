using MikuMikuFlex.モーション;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.時間
{
	internal sealed class TIME変数 : 時間変数
	{
        public override string セマンティクス => "TIME";


        private TIME変数( bool syncInEditMode )
            : base( syncInEditMode )
		{
		}

		public TIME変数()
		{
		}

        protected override 変数管理 時間変数登録インスタンスを生成して返す( bool syncInEditMode )
		{
			return new TIME変数( syncInEditMode );
		}

		protected override void 変数を更新する( EffectVariable 変数, モーション管理 モーション )
		{
			if( SyncInEditMode )
			{
				変数.AsScalar().Set( モーション.現在再生中のモーションのフレーム位置sec );
			}
			else
			{
				変数.AsScalar().Set( モーションタイマ.stopWatch.ElapsedMilliseconds / 1000f );
			}
		}
	}
}
