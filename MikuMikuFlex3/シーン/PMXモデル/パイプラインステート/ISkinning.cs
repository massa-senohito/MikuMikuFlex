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
        /// <param name="NumberOfInputVertices">
        ///     モデルの全頂点数。
        /// </param>
        /// <param name="BoneModelPoseMatrixConstantBuffer">
        ///     ボーンのモデルポーズ行列の配列を格納された定数バッファ。
        ///     構造については <see cref="PMXModel.D3DBoneTrans"/> 参照。
        /// </param>
        /// <param name="BoneLocalPositionConstantBuffer">
        ///     ボーンのローカル位置の配列が格納された定数バッファ。
        ///     構造については <see cref="PMXModel.D3DBoneLocalPosition"/>　参照。
        /// </param>
        /// <param name="BoneRotationMatrixConstantBuffer">
        ///     ボーンの回転行列の配列が格納された定数バッファ。
        ///     構造については <see cref="PMXModel.D3DBoneLocalPosition"/> 参照。
        /// </param>
        /// <param name="PreDeformationVertexDataSRV">
        ///     スキニングの入力となる頂点データリソースのSRV。
        ///     構造については <see cref="CS_INPUT"/> 参照。
        /// </param>
        /// <param name="VertexDataAfterTransformationUAV">
        ///     スキニングの出力を書き込む頂点データリソースのUAV。
        ///     構造については <see cref="VS_INPUT"/> 参照。
        /// </param>
        /// <remarks>
        ///     このメソッドの呼び出し時には、<paramref name="d3ddc"/> には事前に以下のように設定される。
        ///     - ComputeShader
        ///         - slot( b1 ) …… <paramref name="BoneModelPoseMatrixConstantBuffer"/>
        ///         - slot( b2 ) …… <paramref name="BoneLocalPositionConstantBuffer"/>
        ///         - slot( b3 ) …… <paramref name="BoneRotationMatrixConstantBuffer"/>
        ///         - slot( t0 ) …… <paramref name="PreDeformationVertexDataSRV"/>
        ///         - slot( u0 ) …… <paramref name="VertexDataAfterTransformationUAV"/>
        /// </remarks>
        void Run(
            SharpDX.Direct3D11.DeviceContext d3ddc,
            int NumberOfInputVertices,
            SharpDX.Direct3D11.Buffer BoneModelPoseMatrixConstantBuffer,
            SharpDX.Direct3D11.Buffer BoneLocalPositionConstantBuffer,
            SharpDX.Direct3D11.Buffer BoneRotationMatrixConstantBuffer,
            SharpDX.Direct3D11.ShaderResourceView PreDeformationVertexDataSRV,
            SharpDX.Direct3D11.UnorderedAccessView VertexDataAfterTransformationUAV );
    }
}
