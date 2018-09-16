using SharpDX;

namespace MikuMikuFlex
{
	public class ボーン変形
	{
		public string ボーン名 { get; private set; }

		public Quaternion 回転 { get; set; }

		public Vector3 平行移動 { get; set; }


		public ボーン変形( string ボーン名, Quaternion 回転, Vector3 平行移動 )
		{
			this.ボーン名 = ボーン名;
			this.回転 = 回転;
			this.平行移動 = 平行移動;
		}
	}
}
