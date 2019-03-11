using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using MikuMikuFlex.モデル.シェイプ;

namespace MikuMikuFlex.モデル.コントローラ
{
	class RotateRingController : オーバーレイシリンダーシェイプ
	{
        public class RotationChangedEventArgs : EventArgs
        {
            public Vector3 Axis { get; }

            public float Length { get; }

            public RotationChangedEventArgs( Vector3 axis, float length )
            {
                this.Axis = axis;
                this.Length = length;
            }
        }


        public EventHandler<RotationChangedEventArgs> OnRotated = delegate { };


		public RotateRingController( ILockableController parent, Vector4 color, Vector4 overlayColor, SilinderShapeDescription desc )
            : base( color, overlayColor, desc )
		{
			_dragController = new DragControlManager( parent );
		}

		public override void HitTestResult( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			base.HitTestResult( _dragController.checkNeedHighlight( result ), mouseState, mousePosition );
			_dragController.checkBegin( result, mouseState, mousePosition );
			if( _dragController.IsDragging )
			{
				float t = _calculateLength( _dragController.Delta );
				var a = Vector3.TransformNormal( Vector3.UnitY, モデル状態.ローカル変換行列 );
				a.Normalize();
				OnRotated?.Invoke( this, new RotationChangedEventArgs( a, t ) );
			}
			_dragController.checkEnd( result, mouseState, mousePosition );
		}


        private readonly DragControlManager _dragController;

        /// <param name="delta">
        ///     マウスの画面上の座標の偏移量
        /// </param>
		private float _calculateLength( Vector2 delta )
		{
			カメラ cp = RenderContext.Instance.行列管理.ビュー行列管理;
			Vector3 transformedAxis = Vector3.TransformNormal( Vector3.UnitY,//UnitY=(0,1,0)
				モデル状態.ローカル変換行列 * cp.ビュー行列 );//カメラから見たシリンダの中心軸ベクトルを求める。
															 //Transformer.Transformはモデルのローカル変形行列,
															 //cp.ViewMatrixはカメラのビュー変換行列
			Vector3 cp2la = cp.カメラの注視点 - cp.カメラの位置;//カメラの注視点からカメラの位置を引き目線のベクトルを求める
																//画面上での奥行きにあたるZベクトル
			cp2la.Normalize();//正規化
			Vector3 transformUnit = Vector3.Cross( Vector3.UnitZ, transformedAxis );
			//カメラから見ているので(0,0,1)とシリンダの中心軸ベクトルの外積によって求まるベクトルが
			//このシリンダにとっての値を上下するときの方向ベクトルとして求まる。
			transformUnit.Normalize(); //正規化

			Vector3 xUnit = Vector3.Cross( Vector3.UnitZ, Vector3.TransformNormal( cp.カメラの上方向ベクトル, cp.ビュー行列 ) );//カメラの上方向ベクトルと目線のベクトルの外積を求め、
																													 //現在のカメラ位置における画面上のX軸方向が3DCG空間上で
																													 //どのベクトルで表されるか求める
			xUnit.Normalize();//正規化
			Vector3 yUnit = Vector3.Cross( xUnit, Vector3.UnitZ );//xUnitとcp2laにより画面上でのy軸が3DCG空間上でどのベクトルに
																  //移されるのか求める。|xUnit|=|cp2la|=1のため、正規化は不要
			Vector3 deltaInDim3 = xUnit * delta.X + yUnit * delta.Y;//マウスの移動ベクトルを3CCG空間上で表すベクトルを求める。
			return Vector3.Dot( deltaInDim3, transformUnit );//マウスの移動ベクトルとシリンダの値の上下のための方向ベクトルに
															 //どの程度含まれてるか求めるため内積を求め、これを偏移量の基準とする。
		}
    }
}
