using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using BulletSharp;

namespace MikuMikuFlex.物理演算
{
	internal class 六軸ジョイントにつながる剛体
	{
		public RigidBody 剛体 { private set; get; }
		public Matrix ワールド変換行列 { private set; get; }

        public 六軸ジョイントにつながる剛体( RigidBody 剛体, Matrix ワールド変換行列 )
		{
			this.剛体 = 剛体;
			this.ワールド変換行列 = ワールド変換行列;
		}
	}


	internal class 六軸ジョイントの移動制限
	{
		public Vector3 移動制限1 { private set; get; }
		public Vector3 移動制限2 { private set; get; }

        public 六軸ジョイントの移動制限( Vector3 移動制限1, Vector3 移動制限2 )
		{
			this.移動制限1 = 移動制限1;
			this.移動制限2 = 移動制限2;
		}
	}


    internal class 六軸ジョイントの回転制限
	{
		public Vector3 回転制限1 { private set; get; }
		public Vector3 回転制限2 { private set; get; }

		public 六軸ジョイントの回転制限( Vector3 回転制限1, Vector3 回転制限2 )
		{
			this.回転制限1 = 回転制限1;
			this.回転制限2 = 回転制限2;
		}
	}


	internal class 六軸バネ剛性
	{
		public Vector3 平行移動成分 { private set; get; }
		public Vector3 回転移動成分 { private set; get; }

		public 六軸バネ剛性( Vector3 平行移動成分, Vector3 回転移動成分 )
		{
			this.平行移動成分 = 平行移動成分;
			this.回転移動成分 = 回転移動成分;
		}
	}


	internal class 六軸ジョイントにつながる剛体のペア
	{
		public 六軸ジョイントにつながる剛体 剛体A { private set; get; }
		public 六軸ジョイントにつながる剛体 剛体B { private set; get; }

		public 六軸ジョイントにつながる剛体のペア( 六軸ジョイントにつながる剛体 剛体A, 六軸ジョイントにつながる剛体 剛体B )
		{
			this.剛体A = 剛体A;
			this.剛体B = 剛体B;
		}
	}


	internal class 六軸可動制限
	{
		public 六軸ジョイントの移動制限 移動制限 { private set; get; }
		public 六軸ジョイントの回転制限 回転制限 { private set; get; }

		public 六軸可動制限( 六軸ジョイントの移動制限 移動制限, 六軸ジョイントの回転制限 回転制限 )
		{
			this.移動制限 = 移動制限;
			this.回転制限 = 回転制限;
		}
	}
}