using MMF.モデル.PMX;
using MMF.モーション;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.時間
{
	public abstract class 時間変数 : 変数管理
	{
		/// <summary>
		///     MME仕様2.4参照
		/// </summary>
		protected bool SyncInEditMode;

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float };


        protected 時間変数( bool syncInEditMode )
		{
			SyncInEditMode = syncInEditMode;
		}

		protected 時間変数()
		{
		}

        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			EffectVariable annotation = variable.GetAnnotationByName( "SyncInEditMode" );

            bool syncMode = false;

            if( annotation != null )
                syncMode = ( annotation.AsScalar().GetInt() == 1 );

			return 時間変数登録インスタンスを生成して返す( syncMode );
		}

		protected abstract 変数管理 時間変数登録インスタンスを生成して返す( bool syncInEditMode );

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            if( !_isCached )
            {
                _modelCache = 引数.モデル as PMXModel;
                _isCached = true;
            }

            if( _modelCache != null )
                変数を更新する( 変数, _modelCache.モーション管理 );
        }

        protected abstract void 変数を更新する( EffectVariable 変数, モーション管理 モーション );


        private PMXModel _modelCache;

		private bool _isCached;

	}
}
