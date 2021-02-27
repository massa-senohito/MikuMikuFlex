using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace MikuMikuFlex3.Script
{
    /// <summary>
    ///     スクリプトを使った材質シェーダー。
    /// </summary>
    public class ScriptedMaterialShader : IMaterialShader
    {
        public ScriptedMaterialShader( string scriptPath, Device d3dDevice )
        {
            var scriptFolder = Path.GetDirectoryName( scriptPath );

            string code;
            using( var fs = new FileStream( scriptPath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            using( var sr = new StreamReader( fs ) )
            {
                code = sr.ReadToEnd() + @"
switch( Reason )
{
    case MikuMikuFlex3.Script.Reason.Initialize: Initialize(); break;
    case MikuMikuFlex3.Script.Reason.Run: Run(); break;
}
";
            }

            var scriptSourceResolver = string.IsNullOrEmpty( scriptFolder ) ? ScriptSourceResolver.Default : ScriptSourceResolver.Default.WithBaseDirectory( scriptFolder );
            var scriptOptions = ScriptOptions.Default
                .WithSourceResolver( scriptSourceResolver )
                .WithImports( "System", "MikuMikuFlex3", "MikuMikuFlex3.Script" )
                .WithReferences( System.Reflection.Assembly.GetExecutingAssembly() );

            this._ShaderScript = CSharpScript.Create( code, scriptOptions, typeof( PipelineState ) );
            var imms = this._ShaderScript.Compile();

            if( 0 != imms.Length )
                throw new Exception( $"ScriptCompilationFailed。- {imms[ 0 ].ToString()}" );

            this._PipelineState = new PipelineState( d3dDevice );

            this._PipelineState.Reason = Reason.Initialize;
            this._ShaderScript.RunAsync( this._PipelineState );
        }

        public void Dispose()
        {
            this._PipelineState?.Dispose();
            this._ShaderScript = null;
        }

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
        public void Draw(
            DeviceContext d3ddc,
            int NumberOfVertices, 
            int StartIndexOfVertices, 
            MMDPass passType, 
            in GlobalParameters GlobalParameters,
            SharpDX.Direct3D11.Buffer GlobalParameterConstantBuffer, 
            ShaderResourceView TextureSRV, 
            ShaderResourceView SphereMapTextureSRV, 
            ShaderResourceView ToonTextureSRV )
        {
            this._PipelineState.ResetDrawState( NumberOfVertices, StartIndexOfVertices, passType, d3ddc );

            this._PipelineState.Reason = Reason.Run;
            this._ShaderScript.RunAsync( this._PipelineState );
        }


        protected Microsoft.CodeAnalysis.Scripting.Script _ShaderScript;

        protected PipelineState _PipelineState;
    }
}
