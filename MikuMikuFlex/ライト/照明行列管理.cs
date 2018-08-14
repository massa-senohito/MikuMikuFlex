using MMF.行列;
using SharpDX;

namespace MMF.ライト
{
	public class 照明行列管理
	{
        public カメラ カメラプロバイダ { get; private set; }

		public 射影 射影行列プロバイダ { get; private set; }

        public Vector3 カメラの方向 { get; private set; }

        public Vector3 カメラの位置
		{
			get
			{
				return カメラプロバイダ.カメラの位置;
			}
			set
			{
				カメラプロバイダ.カメラの位置 = value;
				_UpdateDirection();
			}
		}


        public 照明行列管理( 行列管理 manager )
        {
            _行列管理 = manager;
            this.カメラプロバイダ = new カメラ( new Vector3( 0, 0, -20 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );
            this.射影行列プロバイダ = new 射影();
        }


        private 行列管理 _行列管理; // 現状未使用

        private void _UpdateDirection()
		{
			this.カメラの方向 = Vector3.Normalize( -カメラプロバイダ.カメラの位置 );
		}
    }
}
