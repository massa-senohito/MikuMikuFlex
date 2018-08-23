using System;
using MikuMikuFlex.エフェクト.変数管理.材質;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.モデル
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
