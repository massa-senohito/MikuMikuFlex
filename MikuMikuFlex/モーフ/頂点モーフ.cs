using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MMF.モデル;
using MMF.モデル.PMX;
using MMF.モーション;
using SharpDX;

namespace MMF.モーフ
{
	public class 頂点モーフ : モーフ
	{
        public class 頂点モーフデータ
        {
            public List<頂点モーフオフセット> MorphOffsets = new List<頂点モーフオフセット>();

            public 頂点モーフデータ( MMDFileParser.PMXModelParser.モーフ morphData )
            {
                foreach( モーフオフセット morphOffsetBase in morphData.モーフオフセットリスト )
                {
                    MorphOffsets.Add( (頂点モーフオフセット) morphOffsetBase );
                }
            }
        }

        public Dictionary<string, 頂点モーフデータ> MorphList = new Dictionary<string, 頂点モーフデータ>();


		public 頂点モーフ( PMXModel pmxModel )
		{
            _model = pmxModel.モデル;
            _Buffermanager = pmxModel.バッファ管理;

            foreach( MMDFileParser.PMXModelParser.モーフ morphData in this._model.モーフリスト )
			{
				if( morphData.モーフ種類 == モーフ種類.頂点 )
				{
					this.MorphList.Add( morphData.モーフ名, new 頂点モーフデータ( morphData ) );
				}
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
			_頂点位置をリセットする();
		}


        private バッファ管理 _Buffermanager;

        private HashSet<uint> _movedVertex = new HashSet<uint>();

        private PMXモデル _model;


        private void _頂点位置をリセットする()
		{
			foreach( int i in _movedVertex )
			{
				頂点 vertexData = _model.頂点リスト[ i ];
				_Buffermanager.入力頂点リスト[ i ].Position = new Vector4( vertexData.位置, 1f );
			}

			_movedVertex = new HashSet<uint>();
		}

		private bool _進捗率を設定する( float 進捗率, string モーフ名 )
		{
			if( !MorphList.ContainsKey( モーフ名 ) )
                return false;

			頂点モーフデータ data = MorphList[ モーフ名 ];

			foreach( 頂点モーフオフセット vertexMorph in data.MorphOffsets )
			{
				_movedVertex.Add( vertexMorph.頂点インデックス );
				_Buffermanager.入力頂点リスト[ vertexMorph.頂点インデックス ].Position += new Vector4( vertexMorph.座標オフセット量 * 進捗率, 0 );
			}

			_Buffermanager.リセットが必要である = true;

			return true;
		}
    }
}
