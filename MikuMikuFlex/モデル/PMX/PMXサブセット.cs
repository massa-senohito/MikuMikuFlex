using System;
using SharpDX.Direct3D11;
using MMDFileParser.PMXModelParser;
using MikuMikuFlex.エフェクト変数管理;

namespace MikuMikuFlex.モデル
{
	/// <summary>
	///     描画単位
	/// </summary>
	public class PMXサブセット : IDisposable, サブセット
	{
		public エフェクト用材質情報 エフェクト用材質情報 { get; private set; }

        public int インデックスバッファにおける開始位置 { get; set; }

        public int 頂点数 { get; set; }

        public IDrawable Drawable { get; set; }

        public bool カリングする { get; set; }

        public int サブセットID { get; private set; }


        public PMXサブセット( IDrawable drawable, 材質 data, int subsetId )
		{
			Drawable = drawable;
			エフェクト用材質情報 = エフェクト用材質情報.作成する( Drawable, data );
			サブセットID = subsetId;
		}

        public void Dispose()
        {
            エフェクト用材質情報?.Dispose();
            エフェクト用材質情報 = null;
        }

        public void 描画する( DeviceContext deviceContext )
		{
			deviceContext.DrawIndexed( 3 * 頂点数, インデックスバッファにおける開始位置, 0 );
		}
	}
}