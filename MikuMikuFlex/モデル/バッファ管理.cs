using System;

namespace MikuMikuFlex.モデル
{
	public interface バッファ管理 : IDisposable
	{
        SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; }

        SharpDX.Direct3D11.Buffer D3Dインデックスバッファ { get; }

		CS_INPUT[] 入力頂点リスト { get; }

        SharpDX.Direct3D11.InputLayout D3D頂点レイアウト { get; }

        /// <summary>
        ///     頂点モーフなどにより頂点データが変更された場合に true にする。
        ///     すると、次回の更新時に <see cref="D3Dスキニングバッファを更新する"/> が読みだされる。
        /// </summary>
        bool D3Dスキニングバッファをリセットする { get; set; }


        void 初期化する( object model, SharpDX.Direct3D11.Effect d3dEffect );

        void D3Dスキニングバッファを更新する( スキニング skelton, エフェクト effect );
    }
}
