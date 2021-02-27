using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex3
{
	internal class RestraintFactory : IDisposable
	{

		public RestraintFactory( DiscreteDynamicsWorld dynamicsWorld )
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

        public void AddConstraintsToPointsInSpaceToARigidBody( RigidBody RigidBody, ref Vector3 PointsToRestrain )
			=> this._DynamicsWorld.AddConstraint( new Point2PointConstraint( RigidBody, PointsToRestrain.ToBulletSharp() ) );

		public void AddAPointConstraintBetweenRigidBodies( RigidBody RigidBodyA, RigidBody RigidBodyB, ref Vector3 RigidBodyAPositionOfRestraintPointSeenFrom, ref Vector3 RigidBodyBPositionOfRestraintPointSeenFrom )
            => this._DynamicsWorld.AddConstraint( new Point2PointConstraint( RigidBodyA, RigidBodyB, RigidBodyAPositionOfRestraintPointSeenFrom.ToBulletSharp(), RigidBodyBPositionOfRestraintPointSeenFrom.ToBulletSharp() ) );

		public void BetweenRigidBodies6AddShaftSpringRestraint( APairOfRigidBodiesConnectedToASixAxisJoint APairOfRigidBodiesToConnect, SixAxisMovableRestriction SixAxisMovableRestriction, SixAxisSpringRigidity SixAxisSpring )
		{
			var bodyA = APairOfRigidBodiesToConnect.RigidBodyA.RigidBody;
			var bodyB = APairOfRigidBodiesToConnect.RigidBodyB.RigidBody;
			var frameInA = APairOfRigidBodiesToConnect.RigidBodyA.WorldTransformationMatrix;
			var frameInB = APairOfRigidBodiesToConnect.RigidBodyB.WorldTransformationMatrix;
			var Restraint = new Generic6DofSpringConstraint( bodyA, bodyB, frameInA.ToBulletSharp(), frameInB.ToBulletSharp(), true ); // 第五引数の効果は謎。どちらでも同じ様に見える……。

			var c_p1 = SixAxisMovableRestriction.MovementRestrictions.MovementRestrictions1;
			var c_p2 = SixAxisMovableRestriction.MovementRestrictions.MovementRestrictions2;
			var c_r1 = SixAxisMovableRestriction.RotationLimit.RotationLimit1;
			var c_r2 = SixAxisMovableRestriction.RotationLimit.RotationLimit2;
			Restraint.LinearLowerLimit = new BulletSharp.Math.Vector3( c_p1.X, c_p1.Y, c_p1.Z ); // 型はベクトルだがベクトル量ではないのでZは反転しない。
			Restraint.LinearUpperLimit = new BulletSharp.Math.Vector3( c_p2.X, c_p2.Y, c_p2.Z );
			Restraint.AngularLowerLimit = new BulletSharp.Math.Vector3( c_r1.X, c_r1.Y, c_r1.Z );
			Restraint.AngularUpperLimit = new BulletSharp.Math.Vector3( c_r2.X, c_r2.Y, c_r2.Z );

			this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.TranslationComponent.X, 0, Restraint );
            this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.TranslationComponent.Y, 1, Restraint );
            this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.TranslationComponent.Z, 2, Restraint );
            this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.RotationalMovementComponent.X, 3, Restraint );
            this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.RotationalMovementComponent.Y, 4, Restraint );
            this._SetASpringToOneDegreeOfFreedomInRestraint( SixAxisSpring.RotationalMovementComponent.Z, 5, Restraint );

			this._DynamicsWorld.AddConstraint( Restraint );
		}


        /// <summary>
        ///     物理演算の世界
        /// </summary>
        private DiscreteDynamicsWorld _DynamicsWorld;


        private void _SetASpringToOneDegreeOfFreedomInRestraint( float SpringValue, int TypeOfFreedom, Generic6DofSpringConstraint Restraint )
		{
			if( SpringValue == 0.0f )
                return;

			Restraint.EnableSpring( TypeOfFreedom, true );
			Restraint.SetStiffness( TypeOfFreedom, SpringValue );
		}
    }
}
