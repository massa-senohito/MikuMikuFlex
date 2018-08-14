using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMF.物理演算
{
	internal class 剛体物性
	{
		public float 質量 { private set; get; }
		public float 反発係数 { private set; get; }
		public float 摩擦係数 { private set; get; }
		public float 移動減衰係数 { private set; get; }
		public float 回転減衰係数 { private set; get; }

		/// <param name="質量">0にすると動かないstatic剛体になる。</param>
		public 剛体物性( float 質量 = 0, float 反発係数 = 0, float 摩擦係数 = 0.5f, float 移動減衰係数 = 0, float 回転減衰係数 = 0 )
		{
			this.質量 = 質量;
			this.反発係数 = 反発係数;
			this.摩擦係数 = 摩擦係数;
			this.移動減衰係数 = 移動減衰係数;
			this.回転減衰係数 = 回転減衰係数;
		}
	}
}
