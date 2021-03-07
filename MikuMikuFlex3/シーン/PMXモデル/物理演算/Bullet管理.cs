using System;
using System.Collections.Generic;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex3
{
    public class RayResult
    {
        public List<CollisionObject> CollisionObjects;
    }

    //class OverUser :OverlappingPairCallback
    //{

    //}
    /// <summary>
    ///     Physics Bullet の Wrapper クラス
    /// </summary>
    internal class BulletManagement : IDisposable
	{
		public BulletManagement( Vector3 Gravity )
		{
			this._DynamicsWorld = this._DynamicsWorldFactory.CreateAndReturnADynamicWorld( Gravity );
            Drawer.DebugMode = DebugDrawModes.All;
            _DynamicsWorld.DebugDrawer = Drawer;
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

        public MMFlexUtil.BulletUtil.SharpDXBulletDrawer Drawer = new MMFlexUtil.BulletUtil.SharpDXBulletDrawer();
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

            Drawer.Present( );
			this._DynamicsWorld.StepSimulation( ElapsedTimems / 1000f, 10 );
            _DynamicsWorld.DebugDrawWorld();
            //_DynamicsWorld.DebugDrawer.DrawLine( ref zero , ref zero , ref zero );
		}

		public Matrix GetTheWorldMatrixOfThePhysicsResult( RigidBody RigidBody )
			=> RigidBody.MotionState.WorldTransform.ToSharpDX();


        /// <summary>
        ///     Bulletの世界
        /// </summary>
        private DiscreteDynamicsWorld _DynamicsWorld;
        public RayResult CastRay( Vector3 rayStart , Vector3 rayEnd )
        {
            BulletSharp.Math.Vector3 bRayS = new BulletSharp.Math.Vector3(rayStart.X,rayStart.Y , rayStart.Z);
            BulletSharp.Math.Vector3 bRayE = new BulletSharp.Math.Vector3(rayEnd.X,rayEnd.Y , rayEnd.Z);
            //var res = new ClosestRayResultCallback(ref rayStart , ref rayEnd);
            var res = new AllHitsRayResultCallback( bRayS , bRayE);
            _DynamicsWorld.RayTest( bRayS , bRayE , res );
            var obj = res.CollisionObjects;
            var hitPos = res.HitPointWorld;
            var hitNorm = res.HitNormalWorld;
            var rayres = new RayResult();
            rayres.CollisionObjects = obj;
            return rayres;
        }

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
