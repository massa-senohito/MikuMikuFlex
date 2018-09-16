using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.MorphOffset;
using MikuMikuFlex.モデル;
using MikuMikuFlex.モデル.PMX;
using SharpDX;

namespace MikuMikuFlex
{
    /// <summary>
    ///     外部には非公開。
    /// </summary>
	internal class ボーンモーフ : モーフ
	{
        internal class ボーンモーフデータ
        {
            public List<ボーンモーフオフセット> BoneMorphs = new List<ボーンモーフオフセット>();

            public ボーンモーフデータ( MMDFileParser.PMXModelParser.モーフ morphData )
            {
                foreach( モーフオフセット morphOffsetBase in morphData.モーフオフセットリスト )
                {
                    BoneMorphs.Add( (ボーンモーフオフセット) morphOffsetBase );
                }
            }
        }

        public Dictionary<string, ボーンモーフデータ> MorphList = new Dictionary<string, ボーンモーフデータ>();


		public ボーンモーフ( PMXModel model )
		{
			_skinningProvider = model.スキニング;

			foreach( MMDFileParser.PMXModelParser.モーフ morphData in model.モデル.モーフリスト )
			{
				if( morphData.モーフ種類 == モーフ種類.ボーン )
				{
					this.MorphList.Add( morphData.モーフ名, new ボーンモーフデータ( morphData ) );
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

		}


        private スキニング _skinningProvider;
        
        private bool _進捗率を設定する( float 進捗率, string モーフ名 )
		{
			if( !MorphList.ContainsKey( モーフ名 ) )
                return false;

			ボーンモーフデータ data = MorphList[ モーフ名 ];

			foreach( ボーンモーフオフセット boneMorphOffset in data.BoneMorphs )
			{
				var rot = new Quaternion( boneMorphOffset.回転量.X, boneMorphOffset.回転量.Y, boneMorphOffset.回転量.Z, boneMorphOffset.回転量.W );
				_skinningProvider.ボーン配列[ boneMorphOffset.ボーンインデックス ].回転 *= rot;
				_skinningProvider.ボーン配列[ boneMorphOffset.ボーンインデックス ].移動 += boneMorphOffset.移動量;
			}

			return true;
		}
    }
}