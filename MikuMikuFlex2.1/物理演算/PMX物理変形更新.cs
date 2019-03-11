using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using BulletSharp;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.JointParam;
using MikuMikuFlex.物理演算;
using IDisposable = System.IDisposable;

namespace MikuMikuFlex
{
	public class PMX物理変形更新 : 変形更新, IDisposable
	{
        public PMX物理変形更新( PMXボーン[] ボーン配列, List<剛体> 剛体リスト, List<ジョイント> ジョイントリスト )
        {
            _ボーン配列 = ボーン配列;

            var 重力 = new Vector3( 0, -9.8f * 10f, 0 ); // 既定の重力値
            _Bullet管理 = new Bullet管理( 重力 );

            _剛体を作成する( 剛体リスト );
            _ジョイントを作成する( ジョイントリスト );
        }

        public void Dispose()
        {
            _剛体キャッシュリスト.Clear();

            foreach( var 剛体 in _Bulletの剛体リスト )
                剛体.Dispose();
            _Bulletの剛体リスト.Clear();

            _Bullet管理?.Dispose();
            _Bullet管理 = null;
        }

        public bool 変形を更新する()
        {
            // ここでは、
            // (1) 剛体タイプがボーン追従であるものすべてに対応するボーン行列を適用し、
            // (2) 物理演算を行った後に、
            // (3) 剛体タイプが 物理演算 のものを設定、
            // (4) その後に剛体タイプが 物理＋ボーン位置あわせ の物を設定する。
            // 設定および計算を行う順番に注意すること。


            // (1) ボーン追従タイプの剛体にボーン行列を設定
            for( int i = 0; i < _Bulletの剛体リスト.Count; ++i )
            {
                var 剛体 = _剛体キャッシュリスト[ i ];

                // 関連ボーン有りで剛体タイプがボーン追従の場合
                if( 剛体.ボーンインデックス != -1 && 
                    剛体.物理計算種別 == MMDFileParser.PMXModelParser.剛体.剛体の物理演算.ボーン追従 )
                {
                    var bone = this._ボーン配列[ 剛体.ボーンインデックス ];

                    this._Bullet管理.剛体を移動する(     // ボーン行列を適用
                        _Bulletの剛体リスト[ i ], 
                        剛体.初期姿勢行列 * bone.モデルポーズ行列 );
                }
            }
            
            // (2) 物理演算シミュレーション
            _Bullet管理.物理演算の世界の時間を進める();

            // (3) 物理演算の結果に合わせてボーンの位置を修正
            for( int i = 0; i < _Bulletの剛体リスト.Count; ++i )
            {
                var 剛体 = _剛体キャッシュリスト[ i ];

                if( 剛体.ボーンインデックス == -1 )
                    continue;   // 関連ボーンがないなら次へ

                var bone = _ボーン配列[ 剛体.ボーンインデックス ];
                var globalPose = 剛体.オフセット行列 * _Bullet管理.物理演算結果のワールド行列を取得する( _Bulletの剛体リスト[ i ] );
                if( float.IsNaN( globalPose.M11 ) )
                {
                    if( !_physicsAsserted )
                        Debug.WriteLine( "物理演算の結果が不正な結果を出力しました。\nPMXの設定を見直してください。うまくモーション動作しない可能性があります。" );
                    _physicsAsserted = true;
                    continue;
                }
                var localPose = globalPose * ( ( bone.親ボーン != null ) ? Matrix.Invert( bone.親ボーン.モデルポーズ行列 ) : Matrix.Identity );
                var mat = Matrix.Translation( bone.ローカル位置 ) * localPose * Matrix.Translation( -bone.ローカル位置 );
                bone.移動 = new Vector3( mat.M41, mat.M42, mat.M43 );
                bone.回転 = Quaternion.RotationMatrix( mat );
                bone.モデルポーズを更新する();
            }

            // (4) ボーン位置あわせタイプの剛体の位置移動量にボーンの位置移動量を設定
            for( int i = 0; i < _Bulletの剛体リスト.Count; ++i )
            {
                var 剛体 = _剛体キャッシュリスト[ i ];

                // 関連ボーン有りで剛体タイプが物理＋ボーン位置あわせの場合ボーンの位置移動量を設定
                if( 剛体.ボーンインデックス != -1 &&
                    剛体.物理計算種別 == MMDFileParser.PMXModelParser.剛体.剛体の物理演算.物理演算とボーン位置合わせ )
                {
                    var bone = this._ボーン配列[ 剛体.ボーンインデックス ];
                    var v = new Vector3( bone.モデルポーズ行列.M41, bone.モデルポーズ行列.M42, bone.モデルポーズ行列.M43 );   // ボーンの移動量
                    var p = new Vector3( 剛体.初期姿勢行列.M41, 剛体.初期姿勢行列.M42, 剛体.初期姿勢行列.M43 ) + v;
                    var m = this._Bullet管理.物理演算結果のワールド行列を取得する( this._Bulletの剛体リスト[ i ] );
                    m.M41 = p.X;
                    m.M42 = p.Y;
                    m.M43 = p.Z;

                    this._Bullet管理.剛体を移動する( this._Bulletの剛体リスト[ i ], m );
                }
            }

            return false;
        }


        /// <summary>
        ///     最初に計算しておいてあとで繰り返し使う剛体データ
        /// </summary>
        private class 剛体キャッシュ
        {
            public readonly Vector3 初期位置;
            public readonly Matrix 初期姿勢行列;
            public readonly Matrix オフセット行列;
            public readonly int ボーンインデックス;
            public readonly 剛体.剛体の物理演算 物理計算種別;
            public readonly 剛体.剛体形状 剛体形状;

            public 剛体キャッシュ( 剛体 rigidBodyData )
            {
                初期位置 = rigidBodyData.位置;
                var r = rigidBodyData.回転rad;
                初期姿勢行列 = Matrix.RotationYawPitchRoll( r.Y, r.X, r.Z ) * Matrix.Translation( 初期位置 );
                オフセット行列 = Matrix.Invert( 初期姿勢行列 );
                ボーンインデックス = rigidBodyData.関連ボーンインデックス;
                物理計算種別 = rigidBodyData.物理演算;
                剛体形状 = rigidBodyData.形状;
            }
        }

        private PMXボーン[] _ボーン配列;

        private List<剛体キャッシュ> _剛体キャッシュリスト;

		private Bullet管理 _Bullet管理;

        private List<RigidBody> _Bulletの剛体リスト;

		private static bool _physicsAsserted;


		private void _剛体を作成する( List<剛体> 剛体リスト )
		{
            _剛体キャッシュリスト = new List<剛体キャッシュ>( 剛体リスト.Count );
            _Bulletの剛体リスト = new List<RigidBody>( 剛体リスト.Count );

			foreach( var 剛体 in 剛体リスト )
			{
				var 一時剛体 = new 剛体キャッシュ( 剛体 );
				var 初期行列 = 一時剛体.初期姿勢行列;

                _剛体キャッシュリスト.Add( 一時剛体 );

                CollisionShape bullet形状;
                switch( 剛体.形状 )
				{
					case 剛体.剛体形状.球:
						bullet形状 = new SphereShape( 剛体.サイズ.X );
						break;

                    case 剛体.剛体形状.箱:
						bullet形状 = new BoxShape( 剛体.サイズ.X, 剛体.サイズ.Y, 剛体.サイズ.Z );
						break;

                    case 剛体.剛体形状.カプセル:
						bullet形状 = new CapsuleShape( 剛体.サイズ.X, 剛体.サイズ.Y );
						break;

                    default:
						throw new System.Exception( "Invalid rigid body data" );
				}

                var 剛体プロパティ = new 剛体物性( 剛体.質量, 剛体.反発力, 剛体.摩擦力, 剛体.移動減衰, 剛体.回転減衰 );

                var 超越プロパティ = new 物理演算を超越した特性(
                    物理演算の影響を受けないKinematic剛体である: ( 剛体.物理演算 == 剛体.剛体の物理演算.ボーン追従 ),
                    自身の衝突グループ番号: (CollisionFilterGroups) ( 1 << 剛体.グループ ),
                    自身と衝突する他の衝突グループ番号: (CollisionFilterGroups) 剛体.非衝突グループフラグ );

                var bullet剛体 = _Bullet管理.剛体を作成する( bullet形状, 初期行列, 剛体プロパティ, 超越プロパティ );

                _Bulletの剛体リスト.Add( bullet剛体 );
			}
		}

		private void _ジョイントを作成する( List<ジョイント> ジョイントリスト )
		{
			foreach( var jointData in ジョイントリスト )
			{
                switch( jointData.種別 )
                {
                    case ジョイント.ジョイント種別.P2P:
                    case ジョイント.ジョイント種別.スライダー:
                    case ジョイント.ジョイント種別.ヒンジ:
                    case ジョイント.ジョイント種別.円錐回転:
                    case ジョイント.ジョイント種別.基本6DOF:
                        break;  // TODO: ばね付き6DOF以外のジョイントへの対応

                    case ジョイント.ジョイント種別.ばね付き6DOF:
                        {
                            var jointParam = (ばね付き6DOFジョイントパラメータ) jointData.パラメータ;

                            // 六軸ジョイントに繋がる剛体のペアを作成する。

                            var bodyA = _Bulletの剛体リスト[ jointParam.関連剛体Aのインデックス ];
                            var bodyAworld_inv = Matrix.Invert( _Bullet管理.物理演算結果のワールド行列を取得する( bodyA ) );

                            var bodyB = _Bulletの剛体リスト[ jointParam.関連剛体Bのインデックス ];
                            var bodyBworld_inv = Matrix.Invert( _Bullet管理.物理演算結果のワールド行列を取得する( bodyB ) );

                            var jointRotation = jointParam.回転rad;
                            var jointPosition = jointParam.位置;

                            var jointWorld = Matrix.RotationYawPitchRoll( jointRotation.Y, jointRotation.X, jointRotation.Z ) * Matrix.Translation( jointPosition.X, jointPosition.Y, jointPosition.Z );

                            var connectedBodyA = new 六軸ジョイントにつながる剛体( bodyA, jointWorld * bodyAworld_inv );
                            var connectedBodyB = new 六軸ジョイントにつながる剛体( bodyB, jointWorld * bodyBworld_inv );

                            var つなぐ剛体のペア = new 六軸ジョイントにつながる剛体のペア( connectedBodyA, connectedBodyB );

                            // 六軸可動制限を作成する。

                            var movementRestriction = new 六軸ジョイントの移動制限( jointParam.移動制限の下限, jointParam.移動制限の上限 );
                            var rotationRestriction = new 六軸ジョイントの回転制限( jointParam.回転制限の下限rad, jointParam.回転制限の上限rad );
                            var 六軸可動制限 = new 六軸可動制限( movementRestriction, rotationRestriction );


                            // 六軸バネを作成する。

                            var 六軸バネ = new 六軸バネ剛性( jointParam.バネ移動定数, jointParam.バネ回転定数 );


                            _Bullet管理.剛体と剛体の間に6軸バネ拘束を追加する( つなぐ剛体のペア, 六軸可動制限, 六軸バネ );
                        }
                        break;
                }
			}
		}
	}
}