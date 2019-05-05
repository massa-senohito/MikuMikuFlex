using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex3
{
	internal class 拘束ファクトリ : IDisposable
	{

		public 拘束ファクトリ( DiscreteDynamicsWorld dynamicsWorld )
		{
			this._DynamicsWorld = dynamicsWorld;
		}

        public void Dispose()
        {
            for( int i = this._DynamicsWorld.NumConstraints - 1; i >= 0; --i )
            {
                var constraint = this._DynamicsWorld.GetConstraint( i );
                this._DynamicsWorld.RemoveConstraint( constraint );
                constraint.Dispose();
            }
        }

        public void 剛体に空間上の点への拘束を追加する( RigidBody 剛体, ref Vector3 拘束する点 )
			=> this._DynamicsWorld.AddConstraint( new Point2PointConstraint( 剛体, 拘束する点.ToBulletSharp() ) );

		public void 剛体と剛体の間に点拘束を追加する( RigidBody 剛体A, RigidBody 剛体B, ref Vector3 剛体Aから見た拘束点の位置, ref Vector3 剛体Bから見た拘束点の位置 )
            => this._DynamicsWorld.AddConstraint( new Point2PointConstraint( 剛体A, 剛体B, 剛体Aから見た拘束点の位置.ToBulletSharp(), 剛体Bから見た拘束点の位置.ToBulletSharp() ) );

		public void 剛体と剛体の間に6軸バネ拘束を追加する( 六軸ジョイントにつながる剛体のペア つなぐ剛体のペア, 六軸可動制限 六軸可動制限, 六軸バネ剛性 六軸バネ )
		{
			var bodyA = つなぐ剛体のペア.剛体A.剛体;
			var bodyB = つなぐ剛体のペア.剛体B.剛体;
			var frameInA = つなぐ剛体のペア.剛体A.ワールド変換行列;
			var frameInB = つなぐ剛体のペア.剛体B.ワールド変換行列;
			var 拘束 = new Generic6DofSpringConstraint( bodyA, bodyB, frameInA.ToBulletSharp(), frameInB.ToBulletSharp(), true ); // 第五引数の効果は謎。どちらでも同じ様に見える……。

			var c_p1 = 六軸可動制限.移動制限.移動制限1;
			var c_p2 = 六軸可動制限.移動制限.移動制限2;
			var c_r1 = 六軸可動制限.回転制限.回転制限1;
			var c_r2 = 六軸可動制限.回転制限.回転制限2;
			拘束.LinearLowerLimit = new BulletSharp.Math.Vector3( c_p1.X, c_p1.Y, c_p1.Z ); // 型はベクトルだがベクトル量ではないのでZは反転しない。
			拘束.LinearUpperLimit = new BulletSharp.Math.Vector3( c_p2.X, c_p2.Y, c_p2.Z );
			拘束.AngularLowerLimit = new BulletSharp.Math.Vector3( c_r1.X, c_r1.Y, c_r1.Z );
			拘束.AngularUpperLimit = new BulletSharp.Math.Vector3( c_r2.X, c_r2.Y, c_r2.Z );

			this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.平行移動成分.X, 0, 拘束 );
            this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.平行移動成分.Y, 1, 拘束 );
            this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.平行移動成分.Z, 2, 拘束 );
            this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.回転移動成分.X, 3, 拘束 );
            this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.回転移動成分.Y, 4, 拘束 );
            this._拘束にある一つの自由度へのバネを設定する( 六軸バネ.回転移動成分.Z, 5, 拘束 );

			this._DynamicsWorld.AddConstraint( 拘束 );
		}


        /// <summary>
        ///     物理演算の世界
        /// </summary>
        private DiscreteDynamicsWorld _DynamicsWorld;


        private void _拘束にある一つの自由度へのバネを設定する( float バネの値, int 自由度の種類, Generic6DofSpringConstraint 拘束 )
		{
			if( バネの値 == 0.0f )
                return;

			拘束.EnableSpring( 自由度の種類, true );
			拘束.SetStiffness( 自由度の種類, バネの値 );
		}
    }
}
