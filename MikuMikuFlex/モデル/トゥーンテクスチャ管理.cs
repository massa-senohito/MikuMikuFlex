using System;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.モデル
{
	public interface トゥーンテクスチャ管理 : IDisposable
	{
		ShaderResourceView[] このアバターのトゥーンの配列 { get; }


        void 初期化する( サブリソースローダー subresourceManager );

		int トゥーンを追加で読み込み現在の最後のトゥーンインデックスを返す( string テクスチャ名 );
	}
}
