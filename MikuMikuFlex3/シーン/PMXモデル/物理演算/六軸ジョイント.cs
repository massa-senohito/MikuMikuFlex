using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using BulletSharp;

namespace MikuMikuFlex3
{
	internal class RigidBodyConnectedToA6AxisJoint
	{
		public RigidBody RigidBody { get; private protected set; }
        public Matrix WorldTransformationMatrix { get; private protected set; }

        public RigidBodyConnectedToA6AxisJoint( RigidBody RigidBody, Matrix WorldTransformationMatrix )
		{
			this.RigidBody = RigidBody;
			this.WorldTransformationMatrix = WorldTransformationMatrix;
		}
	}


	internal class RestrictionOnMovementOf6AxisJoint
	{
		public Vector3 MovementRestrictions1 { get; private protected set; }
        public Vector3 MovementRestrictions2 { get; private protected set; }

        public RestrictionOnMovementOf6AxisJoint( Vector3 MovementRestrictions1, Vector3 MovementRestrictions2 )
		{
			this.MovementRestrictions1 = MovementRestrictions1;
			this.MovementRestrictions2 = MovementRestrictions2;
		}
	}


    internal class RotationLimitOf6AxisJoint
	{
		public Vector3 RotationLimit1 { get; private protected set; }
        public Vector3 RotationLimit2 { get; private protected set; }

        public RotationLimitOf6AxisJoint( Vector3 RotationLimit1, Vector3 RotationLimit2 )
		{
			this.RotationLimit1 = RotationLimit1;
			this.RotationLimit2 = RotationLimit2;
		}
	}


	internal class SixAxisSpringRigidity
	{
		public Vector3 TranslationComponent { get; private protected set; }
        public Vector3 RotationalMovementComponent { get; private protected set; }

        public SixAxisSpringRigidity( Vector3 TranslationComponent, Vector3 RotationalMovementComponent )
		{
			this.TranslationComponent = TranslationComponent;
			this.RotationalMovementComponent = RotationalMovementComponent;
		}
	}


	internal class APairOfRigidBodiesConnectedToASixAxisJoint
	{
		public RigidBodyConnectedToA6AxisJoint RigidBodyA { get; private protected set; }
        public RigidBodyConnectedToA6AxisJoint RigidBodyB { get; private protected set; }

        public APairOfRigidBodiesConnectedToASixAxisJoint( RigidBodyConnectedToA6AxisJoint RigidBodyA, RigidBodyConnectedToA6AxisJoint RigidBodyB )
		{
			this.RigidBodyA = RigidBodyA;
			this.RigidBodyB = RigidBodyB;
		}
	}


	internal class SixAxisMovableRestriction
	{
		public RestrictionOnMovementOf6AxisJoint MovementRestrictions { get; private protected set; }
        public RotationLimitOf6AxisJoint RotationLimit { get; private protected set; }

        public SixAxisMovableRestriction( RestrictionOnMovementOf6AxisJoint MovementRestrictions, RotationLimitOf6AxisJoint RotationLimit )
		{
			this.MovementRestrictions = MovementRestrictions;
			this.RotationLimit = RotationLimit;
		}
	}
}
