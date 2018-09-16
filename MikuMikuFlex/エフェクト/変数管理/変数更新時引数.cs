
namespace MikuMikuFlex.エフェクト変数管理
{
    public class 変数更新時引数
    {
        public IDrawable モデル { get; private set; }

        public エフェクト用材質情報 材質 { get; private set; }

        // Dispose を不要にするため WeakReference を使う。
        public System.WeakReference<SharpDX.DXGI.SwapChain> SwapChain { get; private set; }


        public 変数更新時引数( SharpDX.DXGI.SwapChain swapChain )
        {
            SwapChain = new System.WeakReference<SharpDX.DXGI.SwapChain>( swapChain );
        }

        public 変数更新時引数( IDrawable model )
		{
			モデル = model;
		}

		public 変数更新時引数( エフェクト用材質情報 info, IDrawable model )
		{
			材質 = info;
			モデル = model;
		}

	}
}
