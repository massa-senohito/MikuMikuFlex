using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace MMF.行列
{
	/// <summary>
	///     標準的なカメラ管理クラス
	/// </summary>
	public class カメラ
	{
		public Matrix ビュー行列 => _ビュー行列;

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

        public void エフェクトに登録する( Effect d3dEffect )
		{
			d3dEffect.GetVariableBySemantic( "CAMERAPOSITION" ).AsVector().Set( _カメラの位置 );
		}


        private Vector3 _カメラの注視点 = Vector3.Zero;

        private Vector3 _カメラの位置 = Vector3.Zero;

        private Vector3 _カメラの上方向ベクトル = Vector3.Zero;

        private Matrix _ビュー行列 = Matrix.Identity;


        private void _ビュー行列を更新する()
        {
            _ビュー行列 = Matrix.LookAtLH( _カメラの位置, _カメラの注視点, _カメラの上方向ベクトル );
        }

        private void _カメラの行列が変更されたことを通知する( カメラ変数種別 type )
        {
            カメラが更新された?.Invoke( this, new カメラ変更EventArgs( type ) );
        }
    }
}
