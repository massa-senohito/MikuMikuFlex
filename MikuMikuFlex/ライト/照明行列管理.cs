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
				return ビュー行列管理.カメラの位置;
			}
			set
			{
				ビュー行列管理.カメラの位置 = value;

                照明の方向 = Vector3.Normalize( -ビュー行列管理.カメラの位置 );
            }
        }

        public 射影 射影行列プロバイダ { get; private set; }

        public カメラ ビュー行列管理 { get; private set; }


        public 照明行列管理( 行列管理 manager )
        {
            _行列管理 = manager;

            ビュー行列管理 = new カメラ( new Vector3( 0, 0, -20 ), new Vector3( 0, 0, 0 ), new Vector3( 0, 1, 0 ) );
            射影行列プロバイダ = new 射影();
        }


        private 行列管理 _行列管理; // 現状未使用
    }
}
