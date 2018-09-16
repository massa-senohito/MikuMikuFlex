using System;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.スプライト
{
    public interface 動的テクスチャ : IDisposable
    {
        Texture2D TextureResource { get; }

        ShaderResourceView TextureResourceView { get; }

        bool 更新が必要である { get; }


        void 更新する();
    }
}