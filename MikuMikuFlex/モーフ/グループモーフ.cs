using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モデル.PMX;

namespace MikuMikuFlex
{
	public class グループモーフ : モーフ
	{
        public class GroupMorphData
        {
            public List<グループモーフオフセット> MorphOffsets = new List<グループモーフオフセット>();

            public GroupMorphData( MMDFileParser.PMXModelParser.モーフ data )
            {
                foreach( モーフオフセット morphOffsetBase in data.モーフオフセットリスト )
                {
                    MorphOffsets.Add( (グループモーフオフセット) morphOffsetBase );
                }
            }
        }

        public Dictionary<string, GroupMorphData> Morphs = new Dictionary<string, GroupMorphData>();


        public グループモーフ( PMXModel model, モーフ morph )
		{
			this._morph = morph;

			int i = 0;
			foreach( var morphData in model.モデル.モーフリスト )
			{
				if( morphData.モーフ種類 == モーフ種類.グループ )
				{
					this.Morphs.Add( morphData.モーフ名, new GroupMorphData( morphData ) );
				}

				this._モーフ名リスト.Add( i, morphData.モーフ名 );

				i++;
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


        private モーフ _morph;

        private Dictionary<int, string> _モーフ名リスト = new Dictionary<int, string>();


        private bool _進捗率を設定する( float 進捗率, string モーフ名 )
		{
			if( !Morphs.ContainsKey( モーフ名 ) )
                return false;

			GroupMorphData data = Morphs[ モーフ名 ];

			foreach( グループモーフオフセット groupMorphOffset in data.MorphOffsets )
			{
				string targetMorph = _モーフ名リスト[ groupMorphOffset.モーフインデックス ];

                if( モーフ名.Equals( targetMorph ) )
                    throw new InvalidOperationException( "グループモーフに自身のモーフが指定されています。" );

				_morph.進捗率を設定する( 進捗率 * groupMorphOffset.影響度, targetMorph );
			}

			return true;
		}
    }
}
