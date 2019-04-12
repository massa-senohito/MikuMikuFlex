using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public interface IPostEffect : IDisposable
    {
        /// <summary>
        ///     ポストエフェクト用のコンピュートシェーダーを実行する。
        /// </summary>
        /// <remarks>
        ///     このメソッドの呼び出し前に、<paramref name="d3ddc"/> には以下の設定が行われている。
        ///     - ComputeShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///         - slot( t0 ) …… 転送元バッファ(Texture2D)
        ///         - slot( u0 ) …… 転送先バッファ(RWTexture2D＜float4＞)
        /// </remarks>
        void Blit( SharpDX.Direct3D11.DeviceContext d3ddc );
    }
}
