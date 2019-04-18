using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex3
{
	/// <summary>
	///     剛体を作るクラス
	/// </summary>
	internal class 剛体ファクトリ : IDisposable
	{
		public 剛体ファクトリ( DiscreteDynamicsWorld dynamicsWorld )
		{
			this._DynamicsWorld = dynamicsWorld;
		}

		public void Dispose()
		{
			for( int i = this._DynamicsWorld.NumCollisionObjects - 1; i >= 0; --i )
			{
				CollisionObject obj = this._DynamicsWorld.CollisionObjectArray[ i ];

				var body = RigidBody.Upcast( obj );
				if( body != null && body.MotionState != null )
                    body.MotionState.Dispose();

				this._DynamicsWorld.RemoveCollisionObject( obj );
				obj.Dispose();
			}

            for( int i = 0; i < this._CollisionShapes.Count(); ++i )
			{
				CollisionShape collisionShape = this._CollisionShapes[ i ];
				this._CollisionShapes[ i ] = null;
				collisionShape.Dispose();
			}

            this._CollisionShapes.Clear();
		}

        public RigidBody 剛体を作成して返す( CollisionShape 剛体の形, Matrix 剛体のワールド変換行列, 剛体物性 剛体の物性, 物理演算を超越した特性 物理演算を超越した特性 )
        {
            var mass = ( 物理演算を超越した特性.物理演算の影響を受けないKinematic剛体である ) ? 0 : 剛体の物性.質量;

            this._CollisionShapes.Add( 剛体の形 );

            var localInertia = new BulletSharp.Math.Vector3( 0, 0, 0 );

            if( mass != 0 )
                剛体の形.CalculateLocalInertia( mass, out localInertia );

            var motionState = new DefaultMotionState( 剛体のワールド変換行列.ToBulletSharp() );
            var rbInfo = new RigidBodyConstructionInfo( mass, motionState, 剛体の形, localInertia );

            var body = new RigidBody( rbInfo ) {
                Restitution = 剛体の物性.反発係数,
                Friction = 剛体の物性.摩擦係数,
            };
            body.SetDamping( 剛体の物性.移動減衰係数, 剛体の物性.回転減衰係数 );

            float linearDamp = body.LinearDamping;
            float angularDamp = body.AngularDamping;

            if( 物理演算を超越した特性.物理演算の影響を受けないKinematic剛体である )
                body.CollisionFlags = body.CollisionFlags | CollisionFlags.KinematicObject;

            body.ActivationState = ActivationState.DisableDeactivation;

            this._DynamicsWorld.AddRigidBody( body, 物理演算を超越した特性.自身の衝突グループ番号, 物理演算を超越した特性.自身と衝突する他の衝突グループ番号 );

            return body;
        }


        /// <summary>
        ///     リソース開放のため、作った剛体を管理する配列
        /// </summary>
        private List<CollisionShape> _CollisionShapes = new List<CollisionShape>();

        /// <summary>
        ///     物理演算の世界
        /// </summary>
        private DiscreteDynamicsWorld _DynamicsWorld;
    }
}
