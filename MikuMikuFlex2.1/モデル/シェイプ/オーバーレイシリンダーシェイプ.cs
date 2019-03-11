using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex.モデル.シェイプ
{
	public class オーバーレイシリンダーシェイプ : シリンダーシェイプ
	{
		private readonly Vector4 baseColor;

		private readonly Vector4 _overlayColor;

		public オーバーレイシリンダーシェイプ( Vector4 color, Vector4 overlayColor, SilinderShapeDescription desc )
            : base( color, desc )
		{
			baseColor = color;
			_overlayColor = overlayColor;
		}

		public override void HitTestResult( bool result, bool mouseState, System.Drawing.Point mousePosition )
		{
			base.HitTestResult( result, mouseState, mousePosition );
			_color = result ? _overlayColor : baseColor;
		}
	}
}
