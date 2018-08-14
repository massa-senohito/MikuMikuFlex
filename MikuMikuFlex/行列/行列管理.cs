using MMF.モデル;
using SharpDX;

namespace MMF.行列
{
	public class 行列管理
	{
        public ワールド行列 ワールド行列管理 { get; }

        public カメラ ビュー行列管理 { get; }

        public 射影 射影行列管理 { get; }


        public 行列管理( ワールド行列 world, カメラ camera, 射影 projection )
		{
			this.ワールド行列管理 = world;
			this.ビュー行列管理 = camera;
			this.射影行列管理 = projection;
		}

        public Matrix ワールドビュー射影行列を作成する( Vector3 ローカル拡大縮小, Quaternion ローカル回転, Vector3 ローカル移動 )
		{
			return
                this.ワールド行列管理.ローカル値とあわせたワールド変換行列を作成して返す( ローカル拡大縮小, ローカル回転, ローカル移動 ) *
                this.ビュー行列管理.ビュー行列 * 
                this.射影行列管理.射影行列;
		}

        public Matrix ワールドビュー射影行列を作成する( IDrawable drawable )
		{
			return 
                this.ワールド行列管理.モデルのワールド変換行列を作成して返す( drawable ) *
                this.ビュー行列管理.ビュー行列 * 
                this.射影行列管理.射影行列;
		}
	}
}
