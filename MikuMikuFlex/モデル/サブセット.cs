using System;
using SharpDX.Direct3D11;
using MikuMikuFlex.エフェクト変数管理;

namespace MikuMikuFlex
{
	public interface サブセット : IDisposable
	{
		エフェクト用材質情報 エフェクト用材質情報 { get; }

		int サブセットID { get; }

		IDrawable Drawable { get; set; }

		bool カリングする { get; }


		void 描画する( DeviceContext deviceContext );
	}
}
