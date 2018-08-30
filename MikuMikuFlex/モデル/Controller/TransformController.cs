using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuFlex.DeviceManagement;
using MikuMikuFlex.モデル.Controller.ControllerComponent;
using MikuMikuFlex.モデル.Shape;
using SharpDX;

namespace MikuMikuFlex.モデル.Controller
{
	public class TransformController : IDrawable, ILockableController
	{
        public float TransformRotationSensibility { get; set; }

        public float TransformTranslationSensibility { get; set; }

        public float TransformScalingSensibility { get; set; }

        public TransformType Type
        {
            get => _type;
            set
            {
                _type = value;

                _xRotater.表示中 = 
                    _yRotater.表示中 = 
                    _zRotater.表示中 = _type.HasFlag( TransformType.Rotation );

                _xTranslater.表示中 =
                    _yTranslater.表示中 =
                    _zTranslater.表示中 = _type.HasFlag( TransformType.Translation );

                _scaler.表示中 = _type.HasFlag( TransformType.Scaling );
            }
        }

        public IDrawable targetModel { get; private set; }

        public bool 表示中 { get; set; }

        public string ファイル名 { get; private set; }

        public int サブセット数 { get; private set; }

        public int 頂点数 { get; private set; }

        public モデル状態 モデル状態 { get; private set; }

        public Vector4 セルフシャドウ色 { get; set; }

        public Vector4 地面影色 { get; set; }

        public bool ロック中 { get; set; }

        public event EventHandler<TransformChangedEventArgs> 変形された = delegate { };

        public event EventHandler ターゲットモデルが変更された = delegate { };


        public TransformController()
		{
			_type = TransformType.All;

			TransformRotationSensibility = 1.0f;
			TransformScalingSensibility = 1.0f;
			TransformTranslationSensibility = 1.0f;

            モデル状態 = new Transformer基本実装();


            // 回転コントロール

            _xRotater = new RotateRingController( this, new Vector4( 1, 0, 0, 0.7f ), new Vector4( 1, 1, 0, 0.7f ), new シリンダーシェイプ.SilinderShapeDescription( 0.02f, 30 ) );
			_yRotater = new RotateRingController( this, new Vector4( 0, 1, 0, 0.7f ), new Vector4( 1, 1, 0, 0.7f ), new シリンダーシェイプ.SilinderShapeDescription( 0.02f, 30 ) );
			_zRotater = new RotateRingController( this, new Vector4( 0, 0, 1, 0.7f ), new Vector4( 1, 1, 0, 0.7f ), new シリンダーシェイプ.SilinderShapeDescription( 0.02f, 30 ) );

            _cross = new CenterCrossLine();

            _xRotater.モデル状態.回転 *= Quaternion.RotationAxis( Vector3.UnitZ, (float) ( Math.PI / 2 ) );
			_zRotater.モデル状態.回転 *= Quaternion.RotationAxis( -Vector3.UnitX, -(float) ( Math.PI / 2 ) );
			_xRotater.モデル状態.倍率 = _yRotater.モデル状態.倍率 = _zRotater.モデル状態.倍率 = new Vector3( 1f, 0.1f, 1f ) * 20;
			_xRotater.モデル状態.倍率 *= 0.998f;
			_zRotater.モデル状態.倍率 *= 0.990f;

            _xRotater.初期化する();
			_yRotater.初期化する();
			_zRotater.初期化する();

            _xRotater.OnRotated += RotationChanged;
			_yRotater.OnRotated += RotationChanged;
			_zRotater.OnRotated += RotationChanged;

            _controllers.Add( _xRotater );
			_controllers.Add( _yRotater );
			_controllers.Add( _zRotater );


            // 平行移動コントロール

            _xTranslater = new TranslaterConeController( this, new Vector4( 1, 0, 0, 0.7f ), new Vector4( 1, 1, 0, 0.7f ) );
			_yTranslater = new TranslaterConeController( this, new Vector4( 0, 1, 0, 0.7f ), new Vector4( 1, 1, 0, 0.7f ) );
			_zTranslater = new TranslaterConeController( this, new Vector4( 0, 0, 1, 0.7f ), new Vector4( 1, 1, 0, 0.7f ) );

            _xTranslater.初期化する();
			_yTranslater.初期化する();
			_zTranslater.初期化する();

            _xTranslater.モデル状態.倍率 =
				_yTranslater.モデル状態.倍率 =
                _zTranslater.モデル状態.倍率 = new Vector3( 2f );
			_xTranslater.モデル状態.回転 *= Quaternion.RotationAxis( Vector3.UnitZ, (float) ( Math.PI / 2 ) );
			_zTranslater.モデル状態.回転 *= Quaternion.RotationAxis( -Vector3.UnitX, -(float) ( Math.PI / 2 ) );

            MoveTranslater( _xTranslater );
			MoveTranslater( _yTranslater );
			MoveTranslater( _zTranslater );

            _xTranslater.OnTranslated += OnTranslated;
			_yTranslater.OnTranslated += OnTranslated;
			_zTranslater.OnTranslated += OnTranslated;

            _controllers.Add( _xTranslater );
			_controllers.Add( _yTranslater );
			_controllers.Add( _zTranslater );


            // 拡大縮小コントロール

            _scaler = new ScalingCubeController( this, new Vector4( 0, 1, 1, 0.7f ), new Vector4( 1, 1, 0, 1 ) );
            _scaler.初期化する();
            _scaler.モデル状態.倍率 = new Vector3( 3 );
            _scaler.OnScalingChanged += OnScaling;
            _controllers.Add( _scaler );

            表示中 = true;
		}

        public void Dispose()
        {
            _xRotater.Dispose();
            _yRotater.Dispose();
            _zRotater.Dispose();
            _xTranslater.Dispose();
            _yTranslater.Dispose();
            _zTranslater.Dispose();
            _scaler.Dispose();
            _cross.Dispose();
        }

        public void ResetTransformedEventHandler()
        {
            変形された = delegate { };
        }

		public void setTargetModel( IDrawable drawable, モデル状態 transformer = null, Action<TransformController, TransformChangedEventArgs> action = null )
		{
			_transformChanged = action;

            var isChanged = ( drawable != targetModel );

			targetModel = null;
			setRotationProperty( Quaternion.Invert( _sumRotation ) );
			setTranslationProperty( -_sumTranslation );

            if( drawable == null )
                return;

            var trans = ( transformer == null ) ? drawable.モデル状態 : transformer;

            setRotationProperty( trans.回転 );
			setTranslationProperty( trans.位置 );

            targetModel = drawable;

            if( isChanged )
                ターゲットモデルが変更された?.Invoke( this, new EventArgs() );
		}

		public void 描画する()
		{
			if( targetModel == null || !targetModel.表示中 )
                return;

			_cross.描画する();

			foreach( var hitTestable in _controllers )
            {
				if( hitTestable.表示中 )
                    hitTestable.描画する();
			}

		}

		public void 更新する()
		{
		}

		public void Selected()
		{
		}


        private List<HitTestable> _controllers = new List<HitTestable>();

        private RotateRingController _xRotater;

        private RotateRingController _yRotater;

        private RotateRingController _zRotater;

        private TranslaterConeController _xTranslater;

        private TranslaterConeController _yTranslater;

        private TranslaterConeController _zTranslater;

        private ScalingCubeController _scaler;

        private CenterCrossLine _cross;

        private Action<TransformController, TransformChangedEventArgs> _transformChanged;

        private Quaternion _sumRotation = Quaternion.Identity;

        private Vector3 _sumTranslation;

        private float _sumScaling;

        private TransformType _type;


        private void OnScaling( object sender, ScalingCubeController.ScalingChangedEventArgs e )
        {
            float delta = e.Delta * TransformScalingSensibility;

            _sumScaling *= delta;

            if( targetModel != null )
            {
                targetModel.モデル状態.倍率 += new Vector3( delta );

                変形された?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Scaling ) );

                _transformChanged?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Scaling ) );
            }
        }

        private void OnTranslated( object sender, TranslaterConeController.TranslatedEventArgs e )
        {
            setTranslationProperty( e.Translation );
        }

        private void MoveTranslater( TranslaterConeController translater )
        {
            translater.モデル状態.位置 += translater.モデル状態.上方向 * 30;
        }

        private void RotationChanged( object sender, RotateRingController.RotationChangedEventArgs e )
        {
            setRotationProperty( Quaternion.RotationAxis( e.Axis, -e.Length / 100f ) );
        }

        private void setRotationProperty( Quaternion quat )
        {

            quat = Quaternion.Lerp( Quaternion.Identity, quat, TransformRotationSensibility );

            _sumRotation *= quat;
            モデル状態.回転 *= quat;
            _xRotater.モデル状態.回転 *= quat;
            _yRotater.モデル状態.回転 *= quat;
            _zRotater.モデル状態.回転 *= quat;

            if( targetModel != null )
            {
                targetModel.モデル状態.回転 *= quat;
                変形された?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Rotation ) );
                _transformChanged?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Rotation ) );
            }
        }

        private void setTranslationProperty( Vector3 trans )
        {
            trans = Vector3.Lerp( Vector3.Zero, trans, TransformTranslationSensibility );

            _sumTranslation += trans;
            foreach( var hitTestable in _controllers )
            {
                hitTestable.モデル状態.位置 += trans;
            }
            _cross.AddTranslation( trans );

            if( targetModel != null )
            {
                targetModel.モデル状態.位置 += trans;
                変形された?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Translation ) );
                _transformChanged?.Invoke( this, new TransformChangedEventArgs( targetModel, TransformType.Translation ) );
            }
        }
    }
}
