using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MikuMikuFlex.エフェクト.変数管理.材質;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モデル.PMX;
using MikuMikuFlex.モーション;
using SharpDX;

namespace MikuMikuFlex.モーフ
{
    /// <summary>
    ///     外部には非公開。
    /// </summary>
	internal class 材質モーフ : モーフ
	{
        internal class 材質モーフデータ
        {
            public List<材質モーフオフセット> Morphoffsets = new List<材質モーフオフセット>();

            public 材質モーフデータ( MMDFileParser.PMXModelParser.モーフ morph )
            {
                foreach( モーフオフセット morphOffsetBase in morph.モーフオフセットリスト )
                {
                    Morphoffsets.Add( (材質モーフオフセット) morphOffsetBase );
                }
            }
        }

        public Dictionary<string, 材質モーフデータ> Morphs = new Dictionary<string, 材質モーフデータ>();


		public 材質モーフ( PMXModel model )
		{
			this._model = model;

			foreach( MMDFileParser.PMXModelParser.モーフ materialMorphData in model.モデル.モーフリスト )
			{
				if( materialMorphData.モーフ種類 == モーフ種類.材質 )
                    this.Morphs.Add( materialMorphData.モーフ名, new 材質モーフデータ( materialMorphData ) );
			}
		}

		public void フレームを設定する( float フレーム, IEnumerable<モーフモーション> モーフモーションリスト )
		{
			foreach( var morphMotion in モーフモーションリスト )
			{
				_進捗率を設定する( morphMotion.指定したフレームにおけるモーフ値を取得する( フレーム ), morphMotion.モーフ名 );
			}
		}

		public bool 進捗率を設定する( float 進捗率, string モーフ名 )
		{
			return _進捗率を設定する( 進捗率, モーフ名 );
		}

		public void 更新する()
		{
		}


        private PMXModel _model;

        private bool _進捗率を設定する( float progress, string morphName )
		{
			if( !Morphs.ContainsKey( morphName ) )
                return false;

			材質モーフデータ data = Morphs[ morphName ];

			foreach( var materialMorphOffset in data.Morphoffsets )
			{
				if( materialMorphOffset.材質インデックス == -1 )
				{
					foreach( var pmxSubset in _model.サブセット管理.サブセットリスト )
					{
						エフェクト用材質情報 matInfo = pmxSubset.エフェクト用材質情報;
						matInfo = materialMorphOffset.オフセット演算形式 == 0 ? matInfo.乗算差分 : matInfo.加算差分;//0の場合は対象を乗算、1なら対象を加算にセット
						matInfo.拡散色 += materialMorphOffset.拡散色 * progress;
						matInfo.環境色 += new Vector4( materialMorphOffset.環境色, 1f ) * progress;
						matInfo.反射色 += new Vector4( materialMorphOffset.反射色, 1f ) * progress;
						matInfo.反射係数 += materialMorphOffset.反射強度 * progress;
						matInfo.エッジ色 += materialMorphOffset.エッジ色 * progress;
					}
				}
				else
				{
					エフェクト用材質情報 matInfo = _model.サブセット管理.サブセットリスト[ materialMorphOffset.材質インデックス ].エフェクト用材質情報;
					matInfo = materialMorphOffset.オフセット演算形式 == 0 ? matInfo.乗算差分 : matInfo.加算差分;//0の場合は対象を乗算、1なら対象を加算にセット
					matInfo.拡散色 += materialMorphOffset.拡散色 * progress;
					matInfo.環境色 += new Vector4( materialMorphOffset.環境色, 1f ) * progress;
					matInfo.反射色 += new Vector4( materialMorphOffset.反射色, 1f ) * progress;
					matInfo.反射係数 += materialMorphOffset.反射強度 * progress;
					matInfo.エッジ色 += materialMorphOffset.エッジ色 * progress;
				}
			}

            return true;
		}
	}
}
