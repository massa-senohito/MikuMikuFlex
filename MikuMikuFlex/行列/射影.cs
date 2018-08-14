using System;
using SharpDX;

namespace MMF.行列
{
	/// <summary>
	///     一般的なプロジェクション行列の管理クラス
	/// </summary>
	public class 射影
	{
        public Matrix 射影行列 { get; private set; } = Matrix.Identity;

        public float 視野角rad
		{
			get => _視野角;
			set
			{
				_視野角 = value;

				_射影行列を更新する();
				_射影行列が変更されたことを通知する( 射影変数種別.Fovy );
			}
		}

		public float アスペクト比
		{
			get => _アスペクト比;
			set
			{
				_アスペクト比 = value;

				_射影行列を更新する();
				_射影行列が変更されたことを通知する( 射影変数種別.AspectRatio );
			}
		}

		public float ZNear
		{
			get => _ZNear;
			set
			{
				_ZNear = value;

				_射影行列を更新する();
				_射影行列が変更されたことを通知する( 射影変数種別.ZNear );
			}
		}

		public float ZFar
		{
			get => _ZFar;
			set
			{
				_ZFar = value;

				_射影行列を更新する();
				_射影行列が変更されたことを通知する( 射影変数種別.ZFar );
			}
		}

        public event EventHandler<射影変更EventArgs> 射影が変更された;


        public void 射影行列を初期化する( float 初期視野角, float 初期アスペクト比, float 初期ZNear, float 初期ZFar )
		{
			_視野角 = 初期視野角;
			_アスペクト比 = 初期アスペクト比;
			_ZNear = 初期ZNear;
			_ZFar = 初期ZFar;
			_射影行列を更新する();
		}


        private float _アスペクト比 = 1.618f;

        private float _視野角;

        private float _ZNear;

        private float _ZFar;


        private void _射影行列を更新する()
		{
			射影行列 = Matrix.PerspectiveFovLH( _視野角, _アスペクト比, _ZNear, _ZFar );
		}

		private void _射影行列が変更されたことを通知する( 射影変数種別 type )
		{
			射影が変更された?.Invoke( this, new 射影変更EventArgs( type ) );
		}
    }
}
