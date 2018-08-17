using System;

namespace MMF.モデル
{
	public interface バッファ管理 : IDisposable
	{
        SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; }

        SharpDX.Direct3D11.Buffer D3Dインデックスバッファ { get; }

		MMM_SKINNING_INPUT[] 入力頂点リスト { get; }

        SharpDX.Direct3D11.InputLayout D3D頂点レイアウト { get; }

        int 頂点数 { get; }

        /// <summary>
        ///     頂点モーフなどにより頂点データが変更された場合に true にする。
        ///     すると、次回の更新時に <see cref="必要であれば頂点を再作成する"/> が読みだされる。
        /// </summary>
        bool リセットが必要である { get; set; }


        void 初期化する( object model, SharpDX.Direct3D11.Effect d3dEffect );

        void 必要であれば頂点を再作成する();
    }
}
