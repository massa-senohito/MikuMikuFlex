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
        /// <remarks>
        ///     このメソッドの呼び出し前に、<paramref name="d3ddc"/> には以下の設定が行われている。
        ///     - InputAssembler
        ///         - 頂点バッファ（モデル全体）の割り当て
        ///         - 頂点インデックスバッファ（モデル全体）の割り当て
        ///         - 頂点レイアウトの割り当て
        ///         - PrimitiveTopology の割り当て(PatchListWith3ControlPoints固定)
        ///     - VertexShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///     - HullShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///     - DomainShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///     - GeometryShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///     - PixelShader
        ///         - slot( b0 ) …… グローバルパラメータ
        ///         - slot( t0 ) …… テクスチャ
        ///         - slot( t1 ) …… スフィアマップテクスチャ
        ///         - slot( t2 ) …… トゥーンテクスチャ
        ///     - Rasterizer
        ///         - Viewport の設定
        ///         - RasterizerState の設定（材質に応じた固定値）
        ///     - OutputMerger
        ///         - RengerTargetView の割り当て
        ///         - DepthStencilView の割り当て
        ///         - DepthStencilState の割り当て（固定）
        /// </remarks>
        void Draw( int 頂点数, int 頂点の開始インデックス, MMDPass pass種別, SharpDX.Direct3D11.DeviceContext d3ddc );
    }
}
