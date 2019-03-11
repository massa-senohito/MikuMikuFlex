using System;
using BulletSharp;
using SharpDX;

namespace MikuMikuFlex.物理演算
{
	/// <summary>
	///     物理演算 Bullet の Wrapper クラス
	/// </summary>
	internal class Bullet管理 : IDisposable
	{
		public Bullet管理( Vector3 重力 )
		{
			_dynamicsWorld = _dynamicsWorldFactory.動的世界を作って返す( 重力 );
			_剛体ファクトリ = new 剛体ファクトリ( _dynamicsWorld );
			_拘束ファクトリ = new 拘束ファクトリ( _dynamicsWorld );
		}

		public void Dispose()
		{
			if( _Disposed済み )
                return;

			_拘束ファクトリ?.Dispose();
            _拘束ファクトリ = null;

            _剛体ファクトリ?.Dispose();
            _剛体ファクトリ = null;

            _dynamicsWorld?.Dispose();
            _dynamicsWorld = null;

            _dynamicsWorldFactory?.Dispose();
            _dynamicsWorldFactory = null;

            _Disposed済み = true;
		}

		public RigidBody 剛体を作成する( CollisionShape 剛体の形, Matrix 剛体のワールド行列, 剛体物性 剛体の物性, 物理演算を超越した特性 物理演算を超越した特性 )
		{
			return _剛体ファクトリ.剛体を作成して返す( 剛体の形, 剛体のワールド行列, 剛体の物性, 物理演算を超越した特性 );
		}

		public void 剛体に空間上の点への拘束を追加する( RigidBody 剛体, ref Vector3 拘束する点 )
		{
			_拘束ファクトリ.剛体に空間上の点への拘束を追加する( 剛体, ref 拘束する点 );
		}

		public void 剛体と剛体の間に点拘束を追加する( RigidBody 剛体A, RigidBody 剛体B, ref Vector3 剛体Aから見た拘束点の位置, ref Vector3 剛体Bから見た拘束点の位置 )
		{
			_拘束ファクトリ.剛体と剛体の間に点拘束を追加する( 剛体A, 剛体B, ref 剛体Aから見た拘束点の位置, ref 剛体Bから見た拘束点の位置 );
		}

		public void 剛体と剛体の間に6軸バネ拘束を追加する( 六軸ジョイントにつながる剛体のペア つなぐ剛体のペア, 六軸可動制限 六軸可動制限, 六軸バネ剛性 六軸バネ )
		{
			_拘束ファクトリ.剛体と剛体の間に6軸バネ拘束を追加する( つなぐ剛体のペア, 六軸可動制限, 六軸バネ );
		}

		public void 剛体を移動する( RigidBody 剛体, Matrix ワールド行列 )
		{
			剛体.MotionState.WorldTransform = ワールド行列.ToBulletSharp();
		}

		public void 物理演算の世界の時間を進める()
		{
			var 経過時間ms = _bulletTimer.経過時間msを返す();

			_dynamicsWorld.StepSimulation( 経過時間ms / 1000f, 10 );
		}

		public Matrix 物理演算結果のワールド行列を取得する( RigidBody 剛体 )
		{
			return 剛体.MotionState.WorldTransform.ToSharpDX();
		}


        /// <summary>
        ///     Bulletの世界
        /// </summary>
        private DiscreteDynamicsWorld _dynamicsWorld;

        /// <summary>
        /// Bulletの世界を作るクラス
        /// </summary>
        private 動的世界ファクトリ _dynamicsWorldFactory = new 動的世界ファクトリ();

        private 剛体ファクトリ _剛体ファクトリ;

        private 拘束ファクトリ _拘束ファクトリ;

        /// <summary>
        ///     経過時間を計るクラス
        /// </summary>
        private Bulletタイマ _bulletTimer = new Bulletタイマ();

        private bool _Disposed済み = false;
    }
}
