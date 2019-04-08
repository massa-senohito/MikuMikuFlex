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
        /// <param name="d3ddc"></param>
        /// <param name="入力頂点数"></param>
        /// <remarks>
        ///     このメソッドの呼び出し前に、<paramref name="d3ddc"/> には以下の設定が行われている。
        ///     slot( b1 ) …… ボーンのモデルボーズ行列の配列
        ///     slot( b2 ) …… ボーンのローカル位置の配列
        ///     slot( b3 ) …… ボーンの回転の配列
        ///     slot( t0 ) …… 変化前頂点データ CS_BDEF_INPUT の配列
        ///     slot( u0 ) …… 頂点バッファの UAV
        /// </remarks>
        void Run( SharpDX.Direct3D11.DeviceContext d3ddc, int 入力頂点数 );
    }
}
