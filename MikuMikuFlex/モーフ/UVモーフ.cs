using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MikuMikuFlex.モデル;
using SharpDX;

namespace MikuMikuFlex
{
	public class UVモーフ : モーフ
	{
        public class UVモーフデータ
        {
            public List<UVモーフオフセット> MorphOffsets = new List<UVモーフオフセット>();

            public UVモーフデータ( MMDFileParser.PMXModelParser.モーフ morphData )
            {
                foreach( モーフオフセット morphOffsetBase in morphData.モーフオフセットリスト )
                {
                    MorphOffsets.Add( (UVモーフオフセット) morphOffsetBase );
                }
            }
        }

        public Dictionary<string, UVモーフデータ> Morphs = new Dictionary<string, UVモーフデータ>();


		public UVモーフ( PMXModel model, モーフ種類 targetType )
		{
			_bufferManager = model.バッファ管理;
			_targetMorph = targetType;
			this._model = model.モデル;
			if( model.モデル.ヘッダ.追加UV数 + 2 <= (int) targetType ) return;//このとき対応した追加UVは存在しない
			foreach( MMDFileParser.PMXModelParser.モーフ morphData in model.モデル.モーフリスト )
			{
				if( morphData.モーフ種類 == this._targetMorph )
				{
					this.Morphs.Add( morphData.モーフ名, new UVモーフデータ( morphData ) );
				}
			}
		}

		public void フレームを設定する( float frame, IEnumerable<モーフモーション> morphMotions )
		{
			foreach( モーフモーション morphMotion in morphMotions )
			{
				_進捗率を設定する( morphMotion.指定したフレームにおけるモーフ値を取得する( frame ), morphMotion.モーフ名 );
			}
		}

		public bool 進捗率を設定する( float 進捗率, string morphName )
		{
			return _進捗率を設定する( 進捗率, morphName );
		}

		public void 更新する()
		{
		}


        private PMXモデル _model;

        private バッファ管理 _bufferManager;

        private モーフ種類 _targetMorph;


        private bool _進捗率を設定する( float 進捗率, string morphName )
		{
			if( !Morphs.ContainsKey( morphName ) )
                return false;

			var data = Morphs[ morphName ];

			foreach( UVモーフオフセット uvMorphOffset in data.MorphOffsets )
			{
				switch( _targetMorph )
				{
                    case モーフ種類.UV:
						_bufferManager.入力頂点リスト[ uvMorphOffset.頂点インデックス ].UV = _model.頂点リスト[ (int) uvMorphOffset.頂点インデックス ].UV + new Vector2( uvMorphOffset.UVオフセット量.X, uvMorphOffset.UVオフセット量.Y ) * 進捗率;
						break;

                    case モーフ種類.追加UV1:
						_bufferManager.入力頂点リスト[ uvMorphOffset.頂点インデックス ].AddUV1 = _model.頂点リスト[ (int) uvMorphOffset.頂点インデックス ].追加UV[ 0 ] + uvMorphOffset.UVオフセット量 * 進捗率;
						break;

                    case モーフ種類.追加UV2:
						_bufferManager.入力頂点リスト[ uvMorphOffset.頂点インデックス ].AddUV2 = _model.頂点リスト[ (int) uvMorphOffset.頂点インデックス ].追加UV[ 1 ] + uvMorphOffset.UVオフセット量 * 進捗率;
						break;

                    case モーフ種類.追加UV3:
						_bufferManager.入力頂点リスト[ uvMorphOffset.頂点インデックス ].AddUV3 = _model.頂点リスト[ (int) uvMorphOffset.頂点インデックス ].追加UV[ 2 ] + uvMorphOffset.UVオフセット量 * 進捗率;
						break;

                    case モーフ種類.追加UV4:
						_bufferManager.入力頂点リスト[ uvMorphOffset.頂点インデックス ].AddUV4 = _model.頂点リスト[ (int) uvMorphOffset.頂点インデックス ].追加UV[ 3 ] + uvMorphOffset.UVオフセット量 * 進捗率;
						break;
					default:
						throw new ArgumentOutOfRangeException( "不適切なモーフタイプが渡されました" );
				}

			}

            return true;
		}
    }
}
