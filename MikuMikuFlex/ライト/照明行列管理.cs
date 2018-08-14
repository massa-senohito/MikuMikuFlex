using MMF.行列;
using SharpDX;

namespace MMF.ライト
{
	public class 照明行列管理
	{
        public Vector3 照明の方向 { get; private set; }

        public Vector3 照明の位置
		{
			get
			{
				return _カメラプロバイダ.カメラの位置;
			}
			set
			{
				_カメラプロバイダ.カメラの位置 = value;

                照明の方向 = Vector3.Normalize( -_カメラプロバイダ.カメラの位置 );
            }
        }

        public 射影 射影行列プロバイダ { get; private set; }


        public 照明行列管理( 行列管理 manager )
        {
            _行列管理 = manager;

            _カメラプロバイダ = new カメラ( new Vector3( 0, 0, -20 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );
            射影行列プロバイダ = new 射影();
        }


        private 行列管理 _行列管理; // 現状未使用

        private カメラ _カメラプロバイダ;
    }
}
