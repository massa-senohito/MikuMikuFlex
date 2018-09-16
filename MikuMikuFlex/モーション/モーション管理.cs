using System;
using System.Collections.Generic;
using System.IO;
using MMDFileParser.PMXModelParser;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex
{
	/// <summary>
	///     動かせるモデル用
	/// </summary>
	public interface モーション管理 : 変形更新
	{
		モーション 現在再生中のモーション { get; }

		float 現在再生中のモーションのフレーム位置sec { get; }

		float 前回のフレームからの経過時間sec { get; }

        /// <summary>
        ///     [Key: ファイル名, Value: モーション] のリスト。
        /// </summary>
		List<KeyValuePair<string, モーション>> モーションリスト { get; }


		void 初期化する( PMXモデル model, モーフ管理 morph, スキニング skinning, バッファ管理 bufferManager );

		モーション ファイルからモーションを生成し追加する( string filePath, bool ignoreParent );

        モーション ストリームからモーションを追加する( string fileName, Stream stream, bool ignoreParent );

        void モーションを適用する( モーション provider, int startFrame = 0, モーション再生終了後の挙動 setting = モーション再生終了後の挙動.Nothing );

		void モーションを停止する( bool toIdentity = false );


		event EventHandler<モーション再生終了後の挙動> モーションが再生終了した;

		event EventHandler モーションリストが更新された;
	}
}