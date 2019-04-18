using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="IMaterialShader"/> の既定の実装。
    /// </summary>
    public class DefaultMaterialShader : IMaterialShader, IDisposable
    {
        public VertexShader VertexShaderForObject { get; protected set; }

        public VertexShader VertexShaderForEdge { get; protected set; }

        public HullShader HullShader { get; protected set; }

        public DomainShader DomainShader { get; protected set; }

        public GeometryShader GeometryShader { get; protected set; }

        public PixelShader PixelShaderForObject { get; protected set; }

        public PixelShader PixelShaderForEdge { get; protected set; }


        public DefaultMaterialShader( Device d3dDevice )
        {
            this._CreateShader( this._VertexShaderForObjectCSOName, ( b ) => this.VertexShaderForObject = new VertexShader( d3dDevice, b ) );
            this._CreateShader( this._VertexShaderForEdgeCSOName, ( b ) => this.VertexShaderForEdge = new VertexShader( d3dDevice, b ) );
            this._CreateShader( this._HullShaderCSOName, ( b ) => this.HullShader = new HullShader( d3dDevice, b ) );
            this._CreateShader( this._DomainShaderCSOName, ( b ) => this.DomainShader = new DomainShader( d3dDevice, b ) );
            this._CreateShader( this._GeometryShaderCSOName, ( b ) => this.GeometryShader = new GeometryShader( d3dDevice, b ) );
            this._CreateShader( this._PixelShaderForObjectCSOName, ( b ) => this.PixelShaderForObject = new PixelShader( d3dDevice, b ) );
            this._CreateShader( this._PixelShaderForEdgeCSOName, ( b ) => this.PixelShaderForEdge = new PixelShader( d3dDevice, b ) );
        }

        protected void _CreateShader( string csoName, Action<byte[]> create )
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();

                using( var fs = assembly.GetManifestResourceStream( this.GetType(), csoName ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );
                    create( buffer );
                }
            }
            catch( Exception e )
            {
                Trace.TraceError( $"リソースからのシェーダーの作成に失敗しました。[{csoName}][{e.Message}]" );
            }
        }

        public virtual void Dispose()
        {
            this.VertexShaderForObject?.Dispose();
            this.VertexShaderForEdge?.Dispose();
            this.HullShader?.Dispose();
            this.DomainShader?.Dispose();
            this.GeometryShader?.Dispose();
            this.PixelShaderForObject?.Dispose();
            this.PixelShaderForEdge?.Dispose();
        }

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
        public void Draw( int 頂点数, int 頂点の開始インデックス, MMDPass pass種別, DeviceContext d3ddc )
        {
            switch( pass種別 )
            {
                case MMDPass.Object:
                    d3ddc.VertexShader.Set( this.VertexShaderForObject );
                    d3ddc.HullShader.Set( this.HullShader );
                    d3ddc.DomainShader.Set( this.DomainShader );
                    d3ddc.GeometryShader.Set( this.GeometryShader );
                    d3ddc.PixelShader.Set( this.PixelShaderForObject );
                    d3ddc.DrawIndexed( 頂点数, 頂点の開始インデックス, 0 );
                    break;

                case MMDPass.Edge:
                    d3ddc.VertexShader.Set( this.VertexShaderForEdge );
                    d3ddc.HullShader.Set( this.HullShader );
                    d3ddc.DomainShader.Set( this.DomainShader );
                    d3ddc.GeometryShader.Set( this.GeometryShader );
                    d3ddc.PixelShader.Set( this.PixelShaderForEdge );
                    d3ddc.DrawIndexed( 頂点数, 頂点の開始インデックス, 0 );
                    break;

                case MMDPass.ObjectWithSelfShadow:
                    break;

                case MMDPass.Shadow:
                    break;

                case MMDPass.ZPlotForSelfShadow:
                    break;
            }
        }


        // 既定のシェーダーの CSO ファイル

        private readonly string _VertexShaderForObjectCSOName = "Resources.Shaders.DefaultVertexShaderForObject.cso";

        private readonly string _VertexShaderForEdgeCSOName = "Resources.Shaders.DefaultVertexShaderForEdge.cso";

        private readonly string _HullShaderCSOName = "Resources.Shaders.DefaultHullShader.cso";

        private readonly string _DomainShaderCSOName = "Resources.Shaders.DefaultDomainShader.cso";

        private readonly string _GeometryShaderCSOName = "Resources.Shaders.DefaultGeometryShader.cso";

        private readonly string _PixelShaderForObjectCSOName = "Resources.Shaders.DefaultPixelShaderForObject.cso";

        private readonly string _PixelShaderForEdgeCSOName = "Resources.Shaders.DefaultPixelShaderForEdge.cso";
    }
}
