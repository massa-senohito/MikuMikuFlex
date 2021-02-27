using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using BulletSharp;

namespace MikuMikuFlex3
{
	class PMXPhysicalTransformationUpdate : IDisposable
	{
        public PMXPhysicalTransformationUpdate( PMXBoneControl[] BoneArray, List<PMXFormat.RigidBody> RigidBodyList, List<PMXFormat.Joint> JointList )
        {
            this._BoneArray = BoneArray;

            var Gravity = new Vector3( 0, -9.8f * 10f, 0 ); // 既定の重力値
            this._BulletManagement = new BulletManagement( Gravity );

            this._CreateARigidBody( RigidBodyList );
            this._CreateAJoint( JointList );
        }

        public void Dispose()
        {
            this._RigidBodyCacheList.Clear();

            foreach( var RigidBody in this._Bulletの剛体リスト )
                RigidBody.Dispose();
            this._Bulletの剛体リスト.Clear();

            this._BulletManagement?.Dispose();
        }

        public void UpdateTransformation()
        {
            // ここでは、
            // (1) 剛体タイプがボーン追従であるものすべてに対応するボーン行列を適用し、
            // (2) 物理演算を行った後に、
            // (3) 剛体タイプが Physics のものを設定、
            // (4) その後に剛体タイプが 物理＋ボーン位置あわせ の物を設定する。
            // 設定および計算を行う順番に注意すること。


            // (1) ボーン追従タイプの剛体にボーン行列を設定

            for( int i = 0; i < this._Bulletの剛体リスト.Count; ++i )
            {
                var RigidBody = this._RigidBodyCacheList[ i ];

                // 関連ボーン有りで剛体タイプがボーン追従の場合
                if( RigidBody.BoneIndex != -1 && 
                    RigidBody.PhysicalCalculationType == PMXFormat.RigidBodyPhysics.BoneTracking )
                {
                    var bone = this._BoneArray[ RigidBody.BoneIndex ];

                    this._BulletManagement.MoveARigidBody(     // ボーン行列を適用
                        this._Bulletの剛体リスト[ i ], 
                        RigidBody.InitialAttitudeMatrix * bone.ModelPoseMatrix );
                }
            }


            // (2) 物理演算シミュレーション

            this._BulletManagement.AdvanceTimeInTheWorldOfPhysics();


            // (3) 物理演算の結果に合わせてボーンの位置を修正

            for( int i = 0; i < this._Bulletの剛体リスト.Count; ++i )
            {
                var RigidBody = this._RigidBodyCacheList[ i ];

                if( RigidBody.BoneIndex == -1 )
                    continue;   // 関連ボーンがないなら次へ

                var bone = this._BoneArray[ RigidBody.BoneIndex ];
                var globalPose = RigidBody.OffsetMatrix * this._BulletManagement.GetTheWorldMatrixOfThePhysicsResult( this._Bulletの剛体リスト[ i ] );

                if( float.IsNaN( globalPose.M11 ) )
                {
                    if( !_physicsAsserted )
                        Debug.WriteLine( "TheResultOfThePhysicsOperationOutputAnInvalidResult。\nPMXの設定を見直してください。MotionMayNotWorkWell。" );
                    _physicsAsserted = true;
                    continue;
                }
                var localPose = globalPose * ( ( null != bone.ParentBone ) ? Matrix.Invert( bone.ParentBone.ModelPoseMatrix ) : Matrix.Identity );
                var mat = Matrix.Translation( bone.LocalLocation ) * localPose * Matrix.Translation( -bone.LocalLocation );
                bone.Move = new Vector3( mat.M41, mat.M42, mat.M43 );
                bone.Rotation = Quaternion.RotationMatrix( mat );
                bone.CalculateModelPose();
            }


            // (4) ボーン位置あわせタイプの剛体の位置移動量にボーンの位置移動量を設定

            for( int i = 0; i < this._Bulletの剛体リスト.Count; ++i )
            {
                var RigidBody = this._RigidBodyCacheList[ i ];

                // 関連ボーン有りで剛体タイプが物理＋ボーン位置あわせの場合ボーンの位置移動量を設定
                if( RigidBody.BoneIndex != -1 &&
                    RigidBody.PhysicalCalculationType == PMXFormat.RigidBodyPhysics.PhysicsAndBoneAlignment )
                {
                    var bone = this._BoneArray[ RigidBody.BoneIndex ];
                    var v = new Vector3( bone.ModelPoseMatrix.M41, bone.ModelPoseMatrix.M42, bone.ModelPoseMatrix.M43 );   // ボーンの移動量
                    var p = new Vector3( RigidBody.InitialAttitudeMatrix.M41, RigidBody.InitialAttitudeMatrix.M42, RigidBody.InitialAttitudeMatrix.M43 ) + v;
                    var m = this._BulletManagement.GetTheWorldMatrixOfThePhysicsResult( this._Bulletの剛体リスト[ i ] );
                    m.M41 = p.X;
                    m.M42 = p.Y;
                    m.M43 = p.Z;
                    this._BulletManagement.MoveARigidBody( this._Bulletの剛体リスト[ i ], m );
                }
            }
        }


        /// <summary>
        ///     最初に計算しておいてあとで繰り返し使う剛体データ
        /// </summary>
        private class RigidBodyCache
        {
            public readonly Vector3 InitialPosition;
            public readonly Matrix InitialAttitudeMatrix;
            public readonly Matrix OffsetMatrix;
            public readonly int BoneIndex;
            public readonly PMXFormat.RigidBodyPhysics PhysicalCalculationType;
            public readonly PMXFormat.RigidBodyShape RigidBodyShape;

            public RigidBodyCache( PMXFormat.RigidBody rigidBodyData )
            {
                InitialPosition = rigidBodyData.Position;

                var r = rigidBodyData.Rotationrad;
                InitialAttitudeMatrix = Matrix.RotationYawPitchRoll( r.Y, r.X, r.Z ) * Matrix.Translation( InitialPosition );

                OffsetMatrix = Matrix.Invert( InitialAttitudeMatrix );

                BoneIndex = rigidBodyData.RelatedBoneIndex;
                PhysicalCalculationType = rigidBodyData.Physics;
                RigidBodyShape = rigidBodyData.Shape;
            }
        }

        private PMXBoneControl[] _BoneArray;

        private List<RigidBodyCache> _RigidBodyCacheList;

		private BulletManagement _BulletManagement;

        private List<RigidBody> _Bulletの剛体リスト;

		private static bool _physicsAsserted;


		private void _CreateARigidBody( List<PMXFormat.RigidBody> RigidBodyList )
		{
            this._RigidBodyCacheList = new List<RigidBodyCache>( RigidBodyList.Count );
            this._Bulletの剛体リスト = new List<RigidBody>( RigidBodyList.Count );

			foreach( var RigidBody in RigidBodyList )
			{
				var TemporaryRigidBody = new RigidBodyCache( RigidBody );
				var InitialMatrix = TemporaryRigidBody.InitialAttitudeMatrix;

                _RigidBodyCacheList.Add( TemporaryRigidBody );

                CollisionShape bulletShape;
                switch( RigidBody.Shape )
				{
					case PMXFormat.RigidBodyShape.Ball:
						bulletShape = new SphereShape( RigidBody.Size.X );
						break;

                    case PMXFormat.RigidBodyShape.Box:
						bulletShape = new BoxShape( RigidBody.Size.X, RigidBody.Size.Y, RigidBody.Size.Z );
						break;

                    case PMXFormat.RigidBodyShape.Capsule:
						bulletShape = new CapsuleShape( RigidBody.Size.X, RigidBody.Size.Y );
						break;

                    default:
						throw new Exception( "ItIsAnUnknownRigidBodyShape。" );
				}

                var RigidBodyProperties = new RigidPhysicalCharacteristics( RigidBody.Mass, RigidBody.RepulsiveForce, RigidBody.FrictionForce, RigidBody.MovementAttenuation, RigidBody.RotationalDamping );

                var TranscendentalProperty = new CharacteristicsThatTranscendPhysics(
                    NotAffectedByPhysicsKinematicRigidBody: ( RigidBody.Physics == PMXFormat.RigidBodyPhysics.BoneTracking ),
                    OwnCollisionGroupNumber: (CollisionFilterGroups) ( 1 << RigidBody.Group ),
                    OtherCollisionGroupNumbersThatCollideWithItself: (CollisionFilterGroups) RigidBody.NonCollisionGroupFlag );

                var bulletRigidBody = this._BulletManagement.CreateARigidBody( bulletShape, InitialMatrix, RigidBodyProperties, TranscendentalProperty );

                this._Bulletの剛体リスト.Add( bulletRigidBody );
			}
		}

		private void _CreateAJoint( List<PMXFormat.Joint> JointList )
		{
			foreach( var jointData in JointList )
			{
                switch( jointData.Type )
                {
                    case PMXFormat.JointType.P2P:
                    case PMXFormat.JointType.Slider:
                    case PMXFormat.JointType.Hinge:
                    case PMXFormat.JointType.ConeRotation:
                    case PMXFormat.JointType.Basic6DOF:
                        break;  // TODO: WithSpring6DOF以外のジョイントへの対応

                    case PMXFormat.JointType.WithSpring6DOF:
                        {
                            var jointParam = (PMXFormat.WithSpring6DOFJointParameters) jointData.Parameters;

                            // 六軸ジョイントに繋がる剛体のペアを作成する。

                            var bodyA = this._Bulletの剛体リスト[ jointParam.RelatedRigidBodyAのインデックス ];
                            var bodyAworld_inv = Matrix.Invert( this._BulletManagement.GetTheWorldMatrixOfThePhysicsResult( bodyA ) );

                            var bodyB = this._Bulletの剛体リスト[ jointParam.RelatedRigidBodyBのインデックス ];
                            var bodyBworld_inv = Matrix.Invert( this._BulletManagement.GetTheWorldMatrixOfThePhysicsResult( bodyB ) );

                            var jointRotation = jointParam.Rotationrad;
                            var jointPosition = jointParam.Position;

                            var jointWorld = Matrix.RotationYawPitchRoll( jointRotation.Y, jointRotation.X, jointRotation.Z ) * Matrix.Translation( jointPosition.X, jointPosition.Y, jointPosition.Z );

                            var connectedBodyA = new RigidBodyConnectedToA6AxisJoint( bodyA, jointWorld * bodyAworld_inv );
                            var connectedBodyB = new RigidBodyConnectedToA6AxisJoint( bodyB, jointWorld * bodyBworld_inv );

                            var APairOfRigidBodiesToConnect = new APairOfRigidBodiesConnectedToASixAxisJoint( connectedBodyA, connectedBodyB );

                            // 六軸可動制限を作成する。

                            var movementRestriction = new RestrictionOnMovementOf6AxisJoint( jointParam.LowerLimitOfMovementLimit, jointParam.UpperLimitOfMovementLimit );
                            var rotationRestriction = new RotationLimitOf6AxisJoint( jointParam.LowerLimitOfRotationLimitrad, jointParam.UpperLimitOfRotationLimitrad );
                            var SixAxisMovableRestriction = new SixAxisMovableRestriction( movementRestriction, rotationRestriction );


                            // 六軸バネを作成する。

                            var SixAxisSpring = new SixAxisSpringRigidity( jointParam.SpringMovementConstant, jointParam.SpringRotationConstant );


                            this._BulletManagement.BetweenRigidBodies6AddShaftSpringRestraint( APairOfRigidBodiesToConnect, SixAxisMovableRestriction, SixAxisSpring );
                        }
                        break;
                }
			}
		}
	}
}
