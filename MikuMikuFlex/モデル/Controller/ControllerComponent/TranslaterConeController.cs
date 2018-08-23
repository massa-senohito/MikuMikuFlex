using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuFlex.行列;
using MikuMikuFlex.モデル.Shape.Overlay;
using SharpDX;

namespace MikuMikuFlex.モデル.Controller.ControllerComponent
{
	class TranslaterConeController : OverlayConeShape
	{
		public event EventHandler<TranslatedEventArgs> OnTranslated = delegate { };


		public TranslaterConeController( ILockableController locker, Vector4 color, Vector4 overlayColor )
            : base( color, overlayColor )
		{
			_dragController = new DragControlManager( locker );
		}

		public override void HitTestResult( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			base.HitTestResult( _dragController.checkNeedHighlight( result ), mouseState, mousePosition );
			_dragController.checkBegin( result, mouseState, mousePosition );
			if( _dragController.IsDragging )
			{
				checkTranslation();
			}
			_dragController.checkEnd( result, mouseState, mousePosition );
		}

		private void checkTranslation()
		{
			Vector2 delta = _dragController.Delta;
			カメラ cp = RenderContext.Instance.行列管理.ビュー行列管理;
			Vector3 transformedAxis = Vector3.TransformNormal( Vector3.UnitY,//UnitY=(0,1,0)
				モデル状態.ローカル変換行列 * cp.ビュー行列 );//カメラから見たコーンの中心軸ベクトルを求める。
															 //Transformer.Transformはモデルのローカル変形行列,
															 //cp.ViewMatrixはカメラのビュー変換行列
			transformedAxis.Normalize();
			Vector3 cp2la = cp.カメラの注視点 - cp.カメラの位置;//カメラの注視点からカメラの位置を引き目線のベクトルを求める
																//画面上での奥行きにあたるZベクトル
			cp2la.Normalize();//正規化

			Vector3 xUnit = Vector3.Cross( Vector3.UnitZ, Vector3.TransformNormal( cp.カメラの上方向ベクトル, cp.ビュー行列 ) );//カメラの上方向ベクトルと目線のベクトルの外積を求め、
																													 //現在のカメラ位置における画面上のX軸方向が3DCG空間上で
																													 //どのベクトルで表されるか求める
			xUnit.Normalize();//正規化
			Vector3 yUnit = Vector3.Cross( xUnit, Vector3.UnitZ );//xUnitとcp2laにより画面上でのy軸が3DCG空間上でどのベクトルに
																  //移されるのか求める。|xUnit|=|cp2la|=1のため、正規化は不要
			Vector3 deltaInDim3 = xUnit * delta.X + yUnit * delta.Y;//マウスの移動ベクトルを3CCG空間上で表すベクトルを求める。
			float dist = -Vector3.Dot( deltaInDim3, transformedAxis ) / 10f;
			OnTranslated?.Invoke( this, new TranslatedEventArgs( dist * Vector3.TransformNormal( Vector3.UnitY, モデル状態.ローカル変換行列 ) ) );
		}


		public class TranslatedEventArgs : EventArgs
		{
            public Vector3 Translation { get; }

            public TranslatedEventArgs( Vector3 translation )
			{
				this.Translation = translation;
			}
        }


        private DragControlManager _dragController;
    }
}
