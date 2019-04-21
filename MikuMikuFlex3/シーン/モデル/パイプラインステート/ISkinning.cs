using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3
{
    public interface ISkinning : IDisposable
    {
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
        /// <remarks>
        ///     このメソッドの呼び出し時には、<paramref name="d3ddc"/> には事前に以下のように設定される。
        ///     - ComputeShader
        ///         - slot( b1 ) …… <paramref name="ボーンのモデルポーズ行列定数バッファ"/>
        ///         - slot( b2 ) …… <paramref name="ボーンのローカル位置定数バッファ"/>
        ///         - slot( b3 ) …… <paramref name="ボーンの回転行列定数バッファ"/>
        ///         - slot( t0 ) …… <paramref name="変形前頂点データSRV"/>
        ///         - slot( u0 ) …… <paramref name="変形後頂点データUAV"/>
        /// </remarks>
        void Run(
            SharpDX.Direct3D11.DeviceContext d3ddc,
            int 入力頂点数,
            SharpDX.Direct3D11.Buffer ボーンのモデルポーズ行列定数バッファ,
            SharpDX.Direct3D11.Buffer ボーンのローカル位置定数バッファ,
            SharpDX.Direct3D11.Buffer ボーンの回転行列定数バッファ,
            SharpDX.Direct3D11.ShaderResourceView 変形前頂点データSRV,
            SharpDX.Direct3D11.UnorderedAccessView 変形後頂点データUAV );
    }
}
