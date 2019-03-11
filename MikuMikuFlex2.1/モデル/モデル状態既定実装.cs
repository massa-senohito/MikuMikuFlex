using SharpDX;

namespace MikuMikuFlex
{
	public class モデル状態既定実装 : モデル状態
	{
		public Vector3 位置
		{
			get { return _位置; }
			set
			{
				_位置 = value;
				_ローカル変換行列を計算して設定する();
			}
		}

		public Quaternion 回転
		{
			get { return _回転; }
			set
			{
				_回転 = value;
				_回転.Normalize();
				前方向 = Vector3.TransformCoordinate( 前方向の初期値, Matrix.RotationQuaternion( 回転 ) );
				上方向 = Vector3.TransformCoordinate( 上方向の初期値, Matrix.RotationQuaternion( 回転 ) );
				上方向.Normalize();
				前方向.Normalize();
				_ローカル変換行列を計算して設定する();
			}
		}

		public Vector3 倍率
		{
			get { return _倍率; }
			set
			{
				_倍率 = value;
				_ローカル変換行列を計算して設定する();
			}
		}

		public Vector3 前方向
		{
			get { return _前方向; }
			set
			{
				_前方向 = value;
				_前方向.Normalize();
			}
		}

		public Vector3 上方向
		{
			get { return _上方向; }
			set
			{
				_上方向 = value;
				_上方向.Normalize();
			}
		}

		public Vector3 上方向の初期値
		{
			get { return _上方向の初期値; }
			private set
			{
				_上方向の初期値 = value;
				_上方向の初期値.Normalize();
			}
		}

		public Vector3 前方向の初期値
		{
			get { return _前方向の初期値; }
			private set
			{
				_前方向の初期値 = value;
				前方向の初期値.Normalize();

			}
		}

        public Matrix ローカル変換行列 { get; private set; }


        public モデル状態既定実装( Vector3 上方向の初期値, Vector3 前方向の初期値 )
        {
            this.上方向の初期値 = 上方向の初期値;
            this.前方向の初期値 = 前方向の初期値;

            初期状態に戻す();
        }

        public モデル状態既定実装()
            : this( 上方向の初期値: new Vector3( 0, 1, 0 ), 前方向の初期値: new Vector3( 0, 0, -1 ) )
        {
        }

        public void 初期状態に戻す()
		{
			上方向 = _上方向の初期値;
			前方向 = _前方向の初期値;
			回転 = Quaternion.Identity;
			位置 = Vector3.Zero;
			倍率 = new Vector3( 1f );

            _ローカル変換行列を計算して設定する();
		}


        private Vector3 _前方向;
        private Vector3 _前方向の初期値;
        private Vector3 _上方向の初期値;
        private Quaternion _回転;
        private Vector3 _上方向;
        private Vector3 _位置;
        private Vector3 _倍率;

        private void _ローカル変換行列を計算して設定する()
		{
			ローカル変換行列 = Matrix.Scaling( 倍率 ) * Matrix.RotationQuaternion( 回転 ) * Matrix.Translation( 位置 );
		}
    }
}
