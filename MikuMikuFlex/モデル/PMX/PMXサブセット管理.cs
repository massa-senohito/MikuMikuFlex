using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MMDFileParser.PMXModelParser;
using MikuMikuFlex.エフェクト;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.モデル.PMX
{
	internal class PMXサブセット管理 : サブセット管理
	{
		public List<サブセット> サブセットリスト { get; private set; }

        public int サブセットリストの要素数 => サブセットリスト.Count;

        public IDrawable Drawable { get; set; }


        public PMXサブセット管理( PMXModel drawable, PMXモデル model )
		{
			_モデル = model;
			Drawable = drawable;
		}

        public void 初期化する( トゥーンテクスチャ管理 ToonManager, サブリソースローダー subresourceManager )
		{
            _サブリソースローダー = subresourceManager;
            _トゥーンテクスチャ管理 = ToonManager;

			サブセットリスト = new List<サブセット>( _モデル.材質リスト.Count );


            // すべての材質について、サブセットを生成する。

			int 現在の合計頂点数 = 0;

            Debug.Write( $"サブセット:{_モデル.材質リスト.Count} " );

			for( int i = 0; i < _モデル.材質リスト.Count; i++ )
            {
                Debug.Write( ">" );

                var 材質 = _モデル.材質リスト[ i ];


                // サブセット生成

                var subset = new PMXサブセット( Drawable, 材質, i ) {
                    カリングする = !( 材質.描画フラグ.HasFlag( 描画フラグ.両面描画 ) ),
                    頂点数 = 材質.頂点数 / 3,
                    インデックスバッファにおける開始位置 = 現在の合計頂点数,
                };


                // トゥーンテクスチャを決定する

				if( 材質.共有Toonのテクスチャ参照インデックス >= _トゥーンテクスチャ管理.このアバターのトゥーンの配列.Length )
				{
                    // (A) 材質が、まだ読み込まれていないトゥーンテクスチャを指定している場合

                    subset.エフェクト用材質情報.トゥーンを使用する = false;

                    if( 0 == _トゥーンテクスチャ管理.このアバターのトゥーンの配列.Length )
                    {
                        int index = _トゥーンテクスチャ管理.トゥーンを追加で読み込み現在の最後のトゥーンインデックスを返す( _モデル.テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ] );
						subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ index ];
					}
					else
					{
						subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ 0 ];
					}
                }
                else
				{
                    // (B) 材質が、すでに読み込み済みのトゥーンテクスチャを指定している場合

					if( 材質.共有Toonフラグ == 1 )
					{
                        // (B-a) 共有トゥーンテクスチャを指定している場合

                        subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ 材質.共有Toonのテクスチャ参照インデックス + 1 ];
						subset.エフェクト用材質情報.トゥーンを使用する = true;
                    }
                    else if( 材質.共有Toonのテクスチャ参照インデックス != -1 )
					{
                        // (B-b) 材質が、共有トゥーンテクスチャ以外のトゥーンテクスチャを指定している場合

						if( _モデル.テクスチャリスト.Count < 材質.共有Toonのテクスチャ参照インデックス + 1 )
						{
                            // (B-b-a) 存在しないトゥーンテクスチャが指定されている場合 → 先頭のものを使っておく。
							subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ 0 ];
							subset.エフェクト用材質情報.トゥーンを使用する = true;
						}
						else
						{
                            // (B-b-b) 存在するテクスチャの場合、そのテクスチャを読み込んで、それをトゥーンテクスチャとする。
                            int index = _トゥーンテクスチャ管理.トゥーンを追加で読み込み現在の最後のトゥーンインデックスを返す( _モデル.テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ] );
							subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ index ];
							subset.エフェクト用材質情報.トゥーンを使用する = true;
						}
					}
					else
					{
                        // (B-c) 材質が、トゥーンテクスチャを指定していない場合 → 先頭のものを使っておく。
						subset.エフェクト用材質情報.トゥーンテクスチャ = _トゥーンテクスチャ管理.このアバターのトゥーンの配列[ 0 ];
						subset.エフェクト用材質情報.トゥーンを使用する = true;
					}
				}


                // 頂点数を更新

				現在の合計頂点数 += 材質.頂点数;


                // テクスチャの読み込み

                if( 材質.通常テクスチャの参照インデックス != -1 )
				{
                    subset.エフェクト用材質情報.テクスチャ = _サブリソースを検索して返す( _モデル.テクスチャリスト[ 材質.通常テクスチャの参照インデックス ] );
				}
				
                // スフィアマップの読み込み

				if( 材質.スフィアテクスチャの参照インデックス != -1 )
				{
                    subset.エフェクト用材質情報.スフィアモード = 材質.スフィアモード;
					subset.エフェクト用材質情報.スフィアマップ = _サブリソースを検索して返す( _モデル.テクスチャリスト[ 材質.スフィアテクスチャの参照インデックス ] );
				}


                // 完成したサブセットをリストに追加

                サブセットリスト.Add( subset );
			}
            Debug.WriteLine( " done." );
        }

        public void Dispose()
        {
            foreach( KeyValuePair<string, ShaderResourceView> kvp in _テクスチャキャッシュ )
                kvp.Value?.Dispose();
            _テクスチャキャッシュ.Clear();

            foreach( KeyValuePair<string, Texture2D> kvp in _テクスチャリソースキャッシュ )
                kvp.Value?.Dispose();
            _テクスチャリソースキャッシュ.Clear();

            foreach( PMXサブセット subset in サブセットリスト )
                subset.Dispose();

            サブセットリスト.Clear();
        }

        public void すべてを描画する( オブジェクト用エフェクト管理 EffectManager )
        {
            for( int i = 0; i < サブセットリスト.Count; i++ )
            {
                var ipmxSubset = サブセットリスト[ i ];

                var effect = EffectManager.サブセットのエフェクトを取得する( i );

                effect.材質ごとに更新するエフェクト変数と特殊エフェクト変数を更新する( サブセットリスト[ i ].エフェクト用材質情報 );

                effect.エフェクトパスを割り当てつつサブセットを描画する( 
                    サブセットリスト[ i ],
                    MMDPass種別.オブジェクト本体,
                    ( subset ) => {
                        subset.描画する( RenderContext.Instance.DeviceManager.D3DDeviceContext );
                    } );
            }
        }

        public void エッジを描画する( オブジェクト用エフェクト管理 EffectManager )
		{
            for( int i = 0; i < サブセットリスト.Count; i++ )
            {
                var ipmxSubset = サブセットリスト[ i ];

                if( !( ipmxSubset.エフェクト用材質情報.エッジが有効である ) )
                    continue;

                var effect = EffectManager.サブセットのエフェクトを取得する( i );

                effect.材質ごとに更新するエフェクト変数と特殊エフェクト変数を更新する( サブセットリスト[ i ].エフェクト用材質情報 );

                effect.エフェクトパスを割り当てつつサブセットを描画する(
                    サブセットリスト[ i ],
                    MMDPass種別.エッジ,
                    ( subset ) => {
                        subset.描画する( RenderContext.Instance.DeviceManager.D3DDeviceContext );
                    } );
            }
        }

        public void 地面影を描画する( オブジェクト用エフェクト管理 EffectManager )
        {
			// TODO 地面陰の実装

			//foreach (PMXSubset variable in from subset in Subsets where subset.MaterialInfo.isGroundShadowEnable select subset)
			//{
			//    UpdateConstantByMaterial(variable);
			//    MMEEffect.ApplyEffectPass(variable, MMEEffectPassType.Shadow, (subset) => subset.Draw(device));
			//}
		}


        private readonly PMXモデル _モデル;

        private サブリソースローダー _サブリソースローダー;

        private トゥーンテクスチャ管理 _トゥーンテクスチャ管理;

        private Dictionary<string, ShaderResourceView> _テクスチャキャッシュ = new Dictionary<string, ShaderResourceView>();
        private Dictionary<string, Texture2D> _テクスチャリソースキャッシュ = new Dictionary<string, Texture2D>();


        private ShaderResourceView _サブリソースを検索して返す( string name )
		{
            if( _テクスチャキャッシュ.ContainsKey( name ) )
                return _テクスチャキャッシュ[ name ];

			using( Stream stream = _サブリソースローダー.リソースのストリームを返す( name ) )
			{
                var texture = (ShaderResourceView) null;

                if( stream != null )
                {
                    texture = MikuMikuFlex.Utility.MMFShaderResourceView.FromStream( RenderContext.Instance.DeviceManager.D3DDevice, stream, out Texture2D textureResource );
                    _テクスチャキャッシュ.Add( name, texture );
                    _テクスチャリソースキャッシュ.Add( name, textureResource );
                }

                return texture;
			}
		}
    }
}