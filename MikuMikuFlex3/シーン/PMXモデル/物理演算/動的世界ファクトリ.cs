using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MikuMikuFlex3
{
	internal class DynamicWorldFactory : IDisposable
	{
		public DynamicWorldFactory()
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

        public DiscreteDynamicsWorld CreateAndReturnADynamicWorld( SharpDX.Vector3 Gravity )
		{
			var dynamicsWorld = new DiscreteDynamicsWorld( this._Dispatcher, this._OverlappingPairCache, this._Solver, this._CollisionConfiguration );

            dynamicsWorld.Gravity = Gravity.ToBulletSharp();

            return dynamicsWorld;
		}


        private DefaultCollisionConfiguration _CollisionConfiguration;

        private CollisionDispatcher _Dispatcher;

        private BroadphaseInterface _OverlappingPairCache;

        private SequentialImpulseConstraintSolver _Solver;
    }
}
