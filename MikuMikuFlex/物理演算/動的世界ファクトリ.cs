using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.Utility;
using BulletSharp;

namespace MMF.物理演算
{
	internal class 動的世界ファクトリ : IDisposable
	{
		public 動的世界ファクトリ()
		{
			_collisionConfiguration = new DefaultCollisionConfiguration();
			_dispatcher = new CollisionDispatcher( _collisionConfiguration );
			_overlappingPairCache = new DbvtBroadphase();
			_solver = new SequentialImpulseConstraintSolver();
		}

        public void Dispose()
        {
            _solver?.Dispose();
            _solver = null;

            _overlappingPairCache?.Dispose();
            _overlappingPairCache = null;

            _dispatcher?.Dispose();
            _dispatcher = null;

            _collisionConfiguration?.Dispose();
            _collisionConfiguration = null;
        }

        public DiscreteDynamicsWorld 動的世界を作って返す( SharpDX.Vector3 重力 )
		{
			var dynamicsWorld = new DiscreteDynamicsWorld( _dispatcher, _overlappingPairCache, _solver, _collisionConfiguration );
			dynamicsWorld.Gravity = 重力.ToBulletSharp();
			return dynamicsWorld;
		}


        private DefaultCollisionConfiguration _collisionConfiguration;

        private CollisionDispatcher _dispatcher;

        private BroadphaseInterface _overlappingPairCache;

        private SequentialImpulseConstraintSolver _solver;
    }
}