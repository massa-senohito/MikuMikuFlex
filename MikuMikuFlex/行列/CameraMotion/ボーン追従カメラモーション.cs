using System;
using System.Linq;
using MikuMikuFlex.モデル.PMX;
using SharpDX;

namespace MikuMikuFlex.行列.CameraMotion
{
	public class ボーン追従カメラモーション : カメラモーション
	{
        public float カメラとボーンとの距離 { get; set; }

        public bool カメラから見てZ軸方向に回転する { get; set; }

		/// <summary>
		///     初期状態の時から見てどっちから見るか。
		///     (0,0,1)ならば前から、(0,0,-1)ならば後ろから
		/// </summary>
		public Vector3 見る方向 { get; set; }


        public ボーン追従カメラモーション( WeakReference<PMXModel> モデル, string ボーン名, float 距離, Vector3 見る方向, bool カメラから見てZ軸方向に回転する = false )
		{
			_追跡対象のモデル = モデル;

            if( !_追跡対象のモデル.TryGetTarget( out PMXModel model ) )
                return;

            var bones = ( from bone in model.スキニング.ボーン配列
                          where bone.ボーン名 == ボーン名
                          select bone ).ToArray();

            if( bones.Length == 0 )
                throw new InvalidOperationException( $"ボーン\"{ボーン名}\"は見つかりませんでした。" );

            _追跡対象のボーン = new WeakReference<PMXボーン>( bones[ 0 ] );

            this.カメラとボーンとの距離 = 距離;
			this.カメラから見てZ軸方向に回転する = カメラから見てZ軸方向に回転する;
			this.見る方向 = 見る方向;
		}

		void カメラモーション.モーションを更新する( カメラ camera, 射影 projection )
		{
            if( !_追跡対象のボーン.TryGetTarget( out PMXボーン bone ) ||
                !_追跡対象のモデル.TryGetTarget( out PMXModel model ) )
                return;

            // ボーンのワールド座標を求める行列を作成

            Matrix ボーンポーズ行列 =
                bone.モデルポーズ行列 * 
                Matrix.Scaling( model.モデル状態.倍率 ) *
                Matrix.RotationQuaternion( model.モデル状態.回転 ) *
                Matrix.Translation( model.モデル状態.位置 );

            var ボーン位置 = Vector3.TransformCoordinate( bone.ローカル位置, ボーンポーズ行列 );

            var 注視点からカメラの場所に向かうベクトル = Vector3.TransformNormal( -見る方向, ボーンポーズ行列 );
            注視点からカメラの場所に向かうベクトル.Normalize();

            camera.カメラの位置 = ボーン位置 + カメラとボーンとの距離 * 注視点からカメラの場所に向かうベクトル;
			camera.カメラの注視点 = ボーン位置;

            if( カメラから見てZ軸方向に回転する )
			{
				var newUp = Vector3.TransformNormal( new Vector3( 0, 1, 0 ), ボーンポーズ行列 );
				newUp.Normalize();

				camera.カメラの上方向ベクトル = newUp;
			}
		}


        private WeakReference<PMXModel> _追跡対象のモデル;

        private WeakReference<PMXボーン> _追跡対象のボーン;
    }
}
