using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MikuMikuFlex3
{
	internal class 動的世界ファクトリ : IDisposable
	{
		public 動的世界ファクトリ()
		{
			this._CollisionConfiguration = new DefaultCollisionConfiguration();
			this._Dispatcher = new CollisionDispatcher( this._CollisionConfiguration );
			this._OverlappingPairCache = new DbvtBroadphase();
			this._Solver = new SequentialImpulseConstraintSolver();
		}

        public void Dispose()
        {
            this._Solver?.Dispose();
            this._OverlappingPairCache?.Dispose();
            this._Dispatcher?.Dispose();
            this._CollisionConfiguration?.Dispose();
        }

        public DiscreteDynamicsWorld 動的世界を作って返す( SharpDX.Vector3 重力 )
		{
			var dynamicsWorld = new DiscreteDynamicsWorld( this._Dispatcher, this._OverlappingPairCache, this._Solver, this._CollisionConfiguration );

            dynamicsWorld.Gravity = 重力.ToBulletSharp();

            return dynamicsWorld;
		}


        private DefaultCollisionConfiguration _CollisionConfiguration;

        private CollisionDispatcher _Dispatcher;

        private BroadphaseInterface _OverlappingPairCache;

        private SequentialImpulseConstraintSolver _Solver;
    }
}