using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
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
        /// <remarks>
        ///     このメソッドの呼び出し前に、<paramref name="d3ddc"/> には以下の設定が行われている。
        ///     - ComputeShader
        ///         - slot( b1 ) …… ボーンのモデルボーズ行列の配列
        ///         - slot( b2 ) …… ボーンのローカル位置の配列
        ///         - slot( b3 ) …… ボーンの回転の配列
        ///         - slot( t0 ) …… 変化前頂点データ CS_BDEF_INPUT の配列
        ///         - slot( u0 ) …… 頂点バッファの UAV
        /// </remarks>
        public void Run( DeviceContext d3ddc, int 入力頂点数 )
        {
            d3ddc.ComputeShader.Set( this.ComputeShader );
            d3ddc.Dispatch( ( 入力頂点数 / 64 ) + 1, 1, 1 );
        }


        protected ComputeShader ComputeShader;

        private readonly string CSOName = "Resources.Shaders.DefaultSkinningComputeShader.cso";
    }
}
