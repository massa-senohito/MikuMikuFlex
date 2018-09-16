using System.Collections.Generic;
using MMDFileParser.PMXModelParser;

namespace MikuMikuFlex
{
	/// <summary>
	/// PMX標準のモーフを管理するクラス
	/// </summary>
	public class PMXモーフ管理 : モーフ管理
	{
        public List<モーフ> モーフリスト = new List<モーフ>();


        public PMXモーフ管理( PMXModel model )
		{
            モーフリスト.Add( new 頂点モーフ( model ) );
			モーフリスト.Add( new ボーンモーフ( model ) );
			モーフリスト.Add( new 材質モーフ( model ) );
			モーフリスト.Add( new グループモーフ( model, this ) );
			モーフリスト.Add( new UVモーフ( model, モーフ種類.UV ) );
			モーフリスト.Add( new UVモーフ( model, モーフ種類.追加UV1 ) );
			モーフリスト.Add( new UVモーフ( model, モーフ種類.追加UV2 ) );
			モーフリスト.Add( new UVモーフ( model, モーフ種類.追加UV3 ) );
			モーフリスト.Add( new UVモーフ( model, モーフ種類.追加UV4 ) );
		}

		public float モーフの進捗率を返す( string モーフ名 )
			=> _名前toモーフの進捗率マップ[ モーフ名 ];

		public void フレームを設定する( float フレーム, IEnumerable<モーフモーション> モーフモーションリスト )
		{
			foreach( var morphMotion in モーフモーションリスト )
			{
				if( _名前toモーフの進捗率マップ.ContainsKey( morphMotion.モーフ名 ) )
				{
                    // (A) すでに追加済み
					_名前toモーフの進捗率マップ[ morphMotion.モーフ名 ] = morphMotion.指定したフレームにおけるモーフ値を取得する( フレーム );
				}
				else
				{
                    // (B) 未追加 → 追加する
					_名前toモーフの進捗率マップ.Add( morphMotion.モーフ名, morphMotion.指定したフレームにおけるモーフ値を取得する( フレーム ) );
				}
			}

			foreach( モーフ mmdMorphManager in モーフリスト )
			{
				mmdMorphManager.フレームを設定する( フレーム, モーフモーションリスト );
			}
		}

		public bool 進捗率を設定する( float 進捗率, string モーフ名 )
		{
			if( _名前toモーフの進捗率マップ.ContainsKey( モーフ名 ) )
			{
                // (A) すでに追加済み
                _名前toモーフの進捗率マップ[ モーフ名 ] = 進捗率;
			}
			else
			{
                // (B) 未追加 → 追加する
                _名前toモーフの進捗率マップ.Add( モーフ名, 進捗率 );
			}

			foreach( var モーフ in モーフリスト )
			{
                モーフ.進捗率を設定する( 進捗率, モーフ名 );
			}

            return true;
		}

		public void 更新する()
		{
            foreach( var モーフ in モーフリスト )
            {
                モーフ.更新する();
            }
		}


        private Dictionary<string, float> _名前toモーフの進捗率マップ = new Dictionary<string, float>();
    }
}
