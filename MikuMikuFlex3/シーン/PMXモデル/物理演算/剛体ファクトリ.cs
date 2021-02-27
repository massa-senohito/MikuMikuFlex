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
	internal class RigidBodyFactory : IDisposable
	{
		public RigidBodyFactory( DiscreteDynamicsWorld dynamicsWorld )
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

        public RigidBody CreateAndReturnARigidBody( CollisionShape RigidBodyShape, Matrix RigidBodyWorldTransformationMatrix, RigidPhysicalCharacteristics RigidBodyCharacteristics, CharacteristicsThatTranscendPhysics CharacteristicsThatTranscendPhysics )
        {
            var mass = ( CharacteristicsThatTranscendPhysics.NotAffectedByPhysicsKinematicRigidBody ) ? 0 : RigidBodyCharacteristics.Mass;

            this._CollisionShapes.Add( RigidBodyShape );

            var localInertia = new BulletSharp.Math.Vector3( 0, 0, 0 );

            if( mass != 0 )
                RigidBodyShape.CalculateLocalInertia( mass, out localInertia );

            var motionState = new DefaultMotionState( RigidBodyWorldTransformationMatrix.ToBulletSharp() );
            var rbInfo = new RigidBodyConstructionInfo( mass, motionState, RigidBodyShape, localInertia );

            var body = new RigidBody( rbInfo ) {
                Restitution = RigidBodyCharacteristics.CoefficientOfRestitution,
                Friction = RigidBodyCharacteristics.CoefficientOfFriction,
            };
            body.SetDamping( RigidBodyCharacteristics.MovementDampingCoefficient, RigidBodyCharacteristics.RotationDampingCoefficient );

            float linearDamp = body.LinearDamping;
            float angularDamp = body.AngularDamping;

            if( CharacteristicsThatTranscendPhysics.NotAffectedByPhysicsKinematicRigidBody )
                body.CollisionFlags = body.CollisionFlags | CollisionFlags.KinematicObject;

            body.ActivationState = ActivationState.DisableDeactivation;

            this._DynamicsWorld.AddRigidBody( body, CharacteristicsThatTranscendPhysics.OwnCollisionGroupNumber, CharacteristicsThatTranscendPhysics.OtherCollisionGroupNumbersThatCollideWithItself );

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
