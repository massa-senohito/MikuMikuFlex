using SharpDX;

namespace MMF.Bone
{
	public class BoneTransformer
	{
		public string ボーン名 { get; private set; }

		public Quaternion 回転行列 { get; set; }

		public Vector3 平行移動位置ベクタ { get; set; }

		public BoneTransformer( string ボーン名, Quaternion 初期回転行列, Vector3 初期平行移動位置ベクタ )
		{
			this.ボーン名 = ボーン名;
			this.回転行列 = 初期回転行列;
			this.平行移動位置ベクタ = 初期平行移動位置ベクタ;
		}
	}
}
