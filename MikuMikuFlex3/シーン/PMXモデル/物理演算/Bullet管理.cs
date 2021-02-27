using System;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex3
{
	/// <summary>
	///     Physics Bullet の Wrapper クラス
	/// </summary>
	internal class BulletManagement : IDisposable
	{
		public BulletManagement( Vector3 Gravity )
		{
			this._DynamicsWorld = this._DynamicsWorldFactory.CreateAndReturnADynamicWorld( Gravity );

            this._RigidBodyFactory = new RigidBodyFactory( this._DynamicsWorld );
			this._RestraintFactory = new RestraintFactory( this._DynamicsWorld );
		}

		public void Dispose()
		{
			if( this._DisposedDone )
                return;

            _DisposedDone = true;

            this._RestraintFactory?.Dispose();
            this._RigidBodyFactory?.Dispose();
            this._DynamicsWorld?.Dispose();
            this._DynamicsWorldFactory?.Dispose();
		}

		public RigidBody CreateARigidBody( CollisionShape RigidBodyShape, Matrix RigidBodyWorldMatrix, RigidPhysicalCharacteristics RigidBodyCharacteristics, CharacteristicsThatTranscendPhysics CharacteristicsThatTranscendPhysics )
			=> this._RigidBodyFactory.CreateAndReturnARigidBody( RigidBodyShape, RigidBodyWorldMatrix, RigidBodyCharacteristics, CharacteristicsThatTranscendPhysics );

		public void AddConstraintsToPointsInSpaceToARigidBody( RigidBody RigidBody, ref Vector3 PointsToRestrain )
			=> this._RestraintFactory.AddConstraintsToPointsInSpaceToARigidBody( RigidBody, ref PointsToRestrain );

		public void AddAPointConstraintBetweenRigidBodies( RigidBody RigidBodyA, RigidBody RigidBodyB, ref Vector3 RigidBodyAPositionOfRestraintPointSeenFrom, ref Vector3 RigidBodyBPositionOfRestraintPointSeenFrom )
			=> this._RestraintFactory.AddAPointConstraintBetweenRigidBodies( RigidBodyA, RigidBodyB, ref RigidBodyAPositionOfRestraintPointSeenFrom, ref RigidBodyBPositionOfRestraintPointSeenFrom );

		public void BetweenRigidBodies6AddShaftSpringRestraint( APairOfRigidBodiesConnectedToASixAxisJoint APairOfRigidBodiesToConnect, SixAxisMovableRestriction SixAxisMovableRestriction, SixAxisSpringRigidity SixAxisSpring )
			=> this._RestraintFactory.BetweenRigidBodies6AddShaftSpringRestraint( APairOfRigidBodiesToConnect, SixAxisMovableRestriction, SixAxisSpring );

		public void MoveARigidBody( RigidBody RigidBody, Matrix WorldMatrix )
		{
			RigidBody.MotionState.WorldTransform = WorldMatrix.ToBulletSharp();
		}

		public void AdvanceTimeInTheWorldOfPhysics()
		{
			var ElapsedTimems = this._BulletTimer.ElapsedTimemsを返す();

			this._DynamicsWorld.StepSimulation( ElapsedTimems / 1000f, 10 );
		}

		public Matrix GetTheWorldMatrixOfThePhysicsResult( RigidBody RigidBody )
			=> RigidBody.MotionState.WorldTransform.ToSharpDX();


        /// <summary>
        ///     Bulletの世界
        /// </summary>
        private DiscreteDynamicsWorld _DynamicsWorld;

        /// <summary>
        /// Bulletの世界を作るクラス
        /// </summary>
        private DynamicWorldFactory _DynamicsWorldFactory = new DynamicWorldFactory();

        private RigidBodyFactory _RigidBodyFactory;

        private RestraintFactory _RestraintFactory;

        /// <summary>
        ///     経過時間を計るクラス
        /// </summary>
        private BulletTimer _BulletTimer = new BulletTimer();

        private bool _DisposedDone = false;
    }
}
