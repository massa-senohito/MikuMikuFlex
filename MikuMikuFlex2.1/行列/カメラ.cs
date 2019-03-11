using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex
{
	/// <summary>
	///     標準的なカメラ管理クラス
	/// </summary>
	public class カメラ
	{
        public Matrix ビュー行列 { get; private set; } = Matrix.Identity;

        public Vector3 カメラの位置
		{
			get => _カメラの位置;
			set
			{
				_カメラの位置 = value;

				_ビュー行列を更新する();
				_カメラの行列が変更されたことを通知する( カメラ変数種別.位置 );
			}
		}

        public Vector3 カメラの注視点
		{
			get => _カメラの注視点;
			set
			{
				_カメラの注視点 = value;

				_ビュー行列を更新する();
				_カメラの行列が変更されたことを通知する( カメラ変数種別.注視点 );
			}
		}

		public Vector3 カメラの上方向ベクトル
		{
			get => _カメラの上方向ベクトル;
			set
			{
				_カメラの上方向ベクトル = value;

				_ビュー行列を更新する();
				_カメラの行列が変更されたことを通知する( カメラ変数種別.上方向ベクトル );
			}
		}

        public event EventHandler<カメラ変更EventArgs> カメラが更新された;


        public カメラ( Vector3 カメラの初期位置, Vector3 カメラの初期注視点, Vector3 カメラの初期上方向ベクトル )
        {
            this.カメラの位置 = カメラの初期位置;
            this.カメラの注視点 = カメラの初期注視点;
            this.カメラの上方向ベクトル = カメラの初期上方向ベクトル;
        }

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        /// <param name="回転">ラジアン単位。VMDファイルの値はすべて符号を反転する必要があるので注意。</param>
        public void 移動する( float 注視点からの距離, Vector3 注視点の位置, Vector3 回転 )
            => this.移動する( 注視点からの距離, 注視点の位置, Quaternion.RotationYawPitchRoll( 回転.Y, 回転.X, 回転.Z ) );

        /// <summary>
        ///     VMDカメラモーション互換のカメラ移動。
        /// </summary>
        public void 移動する( float 注視点からの距離, Vector3 注視点の位置, Quaternion 回転 )
        {
            this.カメラの注視点 = 注視点の位置;

            var カメラのある方向 = Vector3.TransformCoordinate( new Vector3( 0, 0, 1 ), Matrix.RotationQuaternion( 回転 ) );
            this.カメラの位置 = this.カメラの注視点 + 注視点からの距離 * カメラのある方向;

            this.カメラの上方向ベクトル = Vector3.TransformCoordinate( new Vector3( 0, 1, 0 ), Matrix.RotationQuaternion( 回転 ) );
        }


        private Vector3 _カメラの注視点 = Vector3.Zero;

        private Vector3 _カメラの位置 = Vector3.Zero;

        private Vector3 _カメラの上方向ベクトル = Vector3.Zero;


        private void _ビュー行列を更新する()
        {
            this.ビュー行列 = Matrix.LookAtLH( _カメラの位置, _カメラの注視点, _カメラの上方向ベクトル );
        }

        private void _カメラの行列が変更されたことを通知する( カメラ変数種別 type )
        {
            カメラが更新された?.Invoke( this, new カメラ変更EventArgs( type ) );
        }
    }
}
