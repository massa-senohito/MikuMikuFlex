using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMF.Bone
{
	public class MorphTransformer
	{
		public string モーフ名 { get; private set; }

		public float モーフ値 { get; set; }

		public MorphTransformer( string モーフ名 )
		{
			this.モーフ名 = モーフ名;
		}
	}
}
