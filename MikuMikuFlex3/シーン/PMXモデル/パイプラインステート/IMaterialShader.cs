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
        /// <param name="NumberOfVertices">
        ///     材質の頂点数。
        /// </param>
        /// <param name="StartIndexOfVertices">
        ///     頂点バッファにおける、材質の開始インデックス。
        /// </param>
        /// <param name="passType">
        ///     材質の描画種別。
        /// </param>
        /// <param name="GlobalParameters">
        ///     GlobalParameters。
        /// </param>
        /// <param name="GlobalParameterConstantBuffer">
        ///     グローバルパラメータの内容が格納された定数バッファ。
        /// </param>
        /// <param name="TextureSRV">
        ///     材質が使用するテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <param name="SphereMapTextureSRV">
        ///     材質が使用するスフィアマップテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <param name="ToonTextureSRV">
        ///     材質が使用するトゥーンテクスチャリソースのSRV。未使用なら null。
        /// </param>
        /// <remarks>
        ///     このメソッドの呼び出し時には、<paramref name="d3ddc"/> には事前に以下のように設定される。
        ///     - InputAssembler
        ///         - VertexBuffer（モデル全体）の割り当て
        ///         - 頂点インデックスバッファ（モデル全体）の割り当て
        ///         - 頂点レイアウトの割り当て
        ///         - PrimitiveTopology の割り当て(PatchListWith3ControlPoints固定)
        ///     - VertexShader
        ///         - slot( b0 ) …… <paramref name="GlobalParameterConstantBuffer"/>
        ///     - HullShader
        ///         - slot( b0 ) …… <paramref name="GlobalParameterConstantBuffer"/>
        ///     - DomainShader
        ///         - slot( b0 ) …… <paramref name="GlobalParameterConstantBuffer"/>
        ///     - GeometryShader
        ///         - slot( b0 ) …… <paramref name="GlobalParameterConstantBuffer"/>
        ///     - PixelShader
        ///         - slot( b0 ) …… <paramref name="GlobalParameterConstantBuffer"/>
        ///         - slot( t0 ) …… <paramref name="TextureSRV"/>
        ///         - slot( t1 ) …… <paramref name="SphereMapTextureSRV"/>
        ///         - slot( t2 ) …… <paramref name="ToonTextureSRV"/>
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
            int NumberOfVertices,
            int StartIndexOfVertices,
            MMDPass passType,
            in GlobalParameters GlobalParameters,
            SharpDX.Direct3D11.Buffer GlobalParameterConstantBuffer,
            SharpDX.Direct3D11.ShaderResourceView TextureSRV,
            SharpDX.Direct3D11.ShaderResourceView SphereMapTextureSRV,
            SharpDX.Direct3D11.ShaderResourceView ToonTextureSRV );
    }
}
