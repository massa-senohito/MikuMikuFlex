using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="ISkinning"/> の既定の実装。
    /// </summary>
    public class DefaultSkinning : ISkinning
    {
        public DefaultSkinning( Device d3dDevice )
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using( var fs = assembly.GetManifestResourceStream( this.GetType(), this.CSOName ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );
                    this.ComputeShader = new ComputeShader( d3dDevice, buffer );
                }
            }
            catch( Exception e )
            {
                Trace.TraceError( $"リソースからのコンピュートシェーダーの作成に失敗しました。[{this.CSOName}][{e.Message}]" );
                this.ComputeShader = null;
            }
        }

        public virtual void Dispose()
        {
            this.ComputeShader?.Dispose();
        }

        /// <summary>
        ///     スキニングを実行する。
        /// </summary>
        /// <param name="d3ddc">
        ///     実行対象のDeviceContext。
        /// </param>
        /// <param name="入力頂点数">
        ///     モデルの全頂点数。
        /// </param>
        /// <param name="ボーンのモデルポーズ行列定数バッファ">
        ///     ボーンのモデルポーズ行列の配列を格納された定数バッファ。
        ///     構造については <see cref="PMXモデル.D3DBoneTrans"/> 参照。
        /// </param>
        /// <param name="ボーンのローカル位置定数バッファ">
        ///     ボーンのローカル位置の配列が格納された定数バッファ。
        ///     構造については <see cref="PMXモデル.D3DBoneLocalPosition"/>　参照。
        /// </param>
        /// <param name="ボーンの回転行列定数バッファ">
        ///     ボーンの回転行列の配列が格納された定数バッファ。
        ///     構造については <see cref="PMXモデル.D3DBoneLocalPosition"/> 参照。
        /// </param>
        /// <param name="変形前頂点データSRV">
        ///     スキニングの入力となる頂点データリソースのSRV。
        ///     構造については <see cref="CS_INPUT"/> 参照。
        /// </param>
        /// <param name="変形後頂点データUAV">
        ///     スキニングの出力を書き込む頂点データリソースのUAV。
        ///     構造については <see cref="VS_INPUT"/> 参照。
        /// </param>
        public void Run(
            SharpDX.Direct3D11.DeviceContext d3ddc,
            int 入力頂点数,
            SharpDX.Direct3D11.Buffer ボーンのモデルポーズ行列定数バッファ,
            SharpDX.Direct3D11.Buffer ボーンのローカル位置定数バッファ,
            SharpDX.Direct3D11.Buffer ボーンの回転行列定数バッファ,
            SharpDX.Direct3D11.ShaderResourceView 変形前頂点データSRV,
            SharpDX.Direct3D11.UnorderedAccessView 変形後頂点データUAV )
        {
            d3ddc.ComputeShader.SetConstantBuffer( 1, ボーンのモデルポーズ行列定数バッファ );
            d3ddc.ComputeShader.SetConstantBuffer( 2, ボーンのローカル位置定数バッファ );
            d3ddc.ComputeShader.SetConstantBuffer( 3, ボーンの回転行列定数バッファ );
            d3ddc.ComputeShader.SetShaderResource( 0, 変形前頂点データSRV );
            d3ddc.ComputeShader.SetUnorderedAccessView( 0, 変形後頂点データUAV );
            d3ddc.ComputeShader.Set( this.ComputeShader );

            d3ddc.Dispatch( ( 入力頂点数 / 64 ) + 1, 1, 1 );
        }


        protected ComputeShader ComputeShader;


        // 既定のシェーダーの CSO ファイル

        private readonly string CSOName = "Resources.Shaders.DefaultSkinningComputeShader.cso";
    }
}
