using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
    public interface IMaterialShader : IDisposable
    {
        /// <summary>
        ///     材質を描画する。
        /// </summary>
        /// <param name="d3ddc">
        ///     描画に使用するDeviceContext。
        /// </param>
        /// <param name="頂点数">
        ///     材質の頂点数。
        /// </param>
        /// <param name="頂点の開始インデックス">
        ///     頂点バッファにおける、材質の開始インデックス。
        /// </param>
        /// <param name="pass種別">
        ///     材質の描画種別。
        /// </param>
        /// <param name="グローバルパラメータ">
        ///     グローバルパラメータ。
        /// </param>
        /// <param name="グローバルパラメータ定数バッファ">
        ///     グローバルパラメータの内容が格納された定数バッファ。
        /// </param>
        /// <param name="テクスチャSRV">
        ///     材質が使用するテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <param name="スフィアマップテクスチャSRV">
        ///     材質が使用するスフィアマップテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <param name="トゥーンテクスチャSRV">
        ///     材質が使用するトゥーンテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <remarks>
        ///     このメソッドの呼び出し時には、<paramref name="d3ddc"/> には事前に以下のように設定される。
        ///     - InputAssembler
        ///         - 頂点バッファ（モデル全体）の割り当て
        ///         - 頂点インデックスバッファ（モデル全体）の割り当て
        ///         - 頂点レイアウトの割り当て
        ///         - PrimitiveTopology の割り当て(PatchListWith3ControlPoints固定)
        ///     - VertexShader
        ///         - slot( b0 ) …… <paramref name="グローバルパラメータ定数バッファ"/>
        ///     - HullShader
        ///         - slot( b0 ) …… <paramref name="グローバルパラメータ定数バッファ"/>
        ///     - DomainShader
        ///         - slot( b0 ) …… <paramref name="グローバルパラメータ定数バッファ"/>
        ///     - GeometryShader
        ///         - slot( b0 ) …… <paramref name="グローバルパラメータ定数バッファ"/>
        ///     - PixelShader
        ///         - slot( b0 ) …… <paramref name="グローバルパラメータ定数バッファ"/>
        ///         - slot( t0 ) …… <paramref name="テクスチャSRV"/>
        ///         - slot( t1 ) …… <paramref name="スフィアマップテクスチャSRV"/>
        ///         - slot( t2 ) …… <paramref name="トゥーンテクスチャSRV"/>
        ///         - slot( s0 ) …… ピクセルシェーダー用サンプルステート
        ///     - Rasterizer
        ///         - Viewport の設定
        ///         - RasterizerState の設定
        ///     - OutputMerger
        ///         - RengerTargetView の割り当て
        ///         - DepthStencilView の割り当て
        ///         - DepthStencilState の割り当て
        /// </remarks>
        void Draw(
            SharpDX.Direct3D11.DeviceContext d3ddc,
            int 頂点数,
            int 頂点の開始インデックス,
            MMDPass pass種別,
            in GlobalParameters グローバルパラメータ,
            SharpDX.Direct3D11.Buffer グローバルパラメータ定数バッファ,
            SharpDX.Direct3D11.ShaderResourceView テクスチャSRV,
            SharpDX.Direct3D11.ShaderResourceView スフィアマップテクスチャSRV,
            SharpDX.Direct3D11.ShaderResourceView トゥーンテクスチャSRV );
    }
}
