using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MikuMikuFlex3
{
	internal class CharacteristicsThatTranscendPhysics
	{
		public bool NotAffectedByPhysicsKinematicRigidBody { get; private protected set; }

        public CollisionFilterGroups OwnCollisionGroupNumber { get; private protected set; }

        public CollisionFilterGroups OtherCollisionGroupNumbersThatCollideWithItself { get; private protected set; }


        public CharacteristicsThatTranscendPhysics(
            bool NotAffectedByPhysicsKinematicRigidBody = false, 
            CollisionFilterGroups OwnCollisionGroupNumber = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroups OtherCollisionGroupNumbersThatCollideWithItself = CollisionFilterGroups.AllFilter )
		{
			this.NotAffectedByPhysicsKinematicRigidBody = NotAffectedByPhysicsKinematicRigidBody;
			this.OwnCollisionGroupNumber = OwnCollisionGroupNumber;
			this.OtherCollisionGroupNumbersThatCollideWithItself = OtherCollisionGroupNumbersThatCollideWithItself;
		}
	}
}
