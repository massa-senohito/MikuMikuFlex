using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;
using MikuMikuFlex.エフェクトスクリプト;

namespace MikuMikuFlex
{
	/// <summary>
	///     MME形式のシェーダーのテクニックを管理するクラス
	/// </summary>
	public class テクニック : IDisposable
	{
        /// <summary>
        ///     "string Subset" アノテーションの内容。
        ///     例: "0,3,5", "6-10", "12-"
        /// </summary>
		public HashSet<int> 描画するサブセットIDの集合 { get; private set; }

        /// <summary>
        ///     "string MMDPass" アノテーションの内容。
        ///     省略時には <see cref="MMDPass種別.オブジェクト本体"/> とみなされる。
        /// </summary>
        public MMDPass種別 テクニックを適用する描画対象 { get; private set; }

        /// <summary>
        ///     "bool UseTexture" アノテーションの内容。
        ///     テクスチャ使用の有無を指定する。
        ///     そのテクニックが、テクスチャを使用するサブセットのみを対象とする場合には <see cref="三状態Boolean.有効"/> を指定する。
        ///     逆に、テクスチャを使用しないサブセットのみを対象とする場合には、<see cref="三状態Boolean.無効"/> を指定する。
        ///     アノテーション省略時には、テクスチャの有無は <see cref="三状態Boolean.無視"/> される。
        /// </summary>
        /// <remarks>
        ///     MMDPass="object","object_ss"以外のテクニックでは正しく機能しない。
        /// </remarks>
        public 三状態Boolean テクスチャを使用する { get; private set; }

        /// <summary>
        ///     "bool UseSphereMap" アノテーションの内容。
        ///     スフィアマップ使用の有無を指定する。
        ///     そのテクニックが、スフィアマップを使用するサブセットのみを対象とする場合には <see cref="三状態Boolean.有効"/> を指定する。
        ///     （PMXモデルにおいて、スフィアモードにサブテクスチャを指定した場合も含む）
        ///     逆に、スフィアマップを使用しないサブセットのみを対象とする場合には <see cref="三状態Boolean.無効"/> を指定する。
        ///     アノテーション省略時には、スフィアマップの有無は <see cref="三状態Boolean.無視"/> される。
        /// </summary>
        /// <remarks>
        ///     MMDPass="object","object_ss"以外のテクニックでは正しく機能しない。
        /// </remarks>
        public 三状態Boolean スフィアマップを使用する { get; private set; }

        /// <summary>
        ///     "bool UseToon" アノテーションの内容。
        ///     トゥーンレンダリング使用の有無を指定する。
        ///     そのテクニックが、トゥーンレンダリングを使用するオブジェクト（＝PMDモデル）を対象とする場合には <see cref="三状態Boolean.有効"/> を指定する。
        ///     逆に、トゥーンレンダリングを使用しないオブジェクト（＝アクセサリ）を対象とする場合には <see cref="三状態Boolean.無効"/> を指定する。
        ///     アノテーション省略時には、トゥーンレンダリング使用の有無は <see cref="三状態Boolean.無視"/> される。
        /// </summary>
        /// <remarks>
        ///     MMDPass="object","object_ss"以外のテクニックでは正しく機能しない。
        /// </remarks>
        public 三状態Boolean トゥーンレンダリングを使用する { get; private set; }

        /// <summary>
        ///     "bool UseSelfSadow" アノテーションの内容。（MMM仕様）
        /// </summary>
        public 三状態Boolean セルフ影を使用する { get; private set; }

        /// <summary>
        ///     "bool UseMulSphere" アノテーションの内容。（MMFオリジナル仕様？）
        /// </summary>
        public 三状態Boolean 乗算スフィアを使用する { get; private set; }

        public EffectTechnique D3DEffectTechnique { get; private set; }

        /// <summary>
        ///     このテクニックのパスの一覧
        /// </summary>
        public Dictionary<string, パス> パスリスト { get; private set; }


        /// <summary>
        ///     スクリプトランタイム。
        /// </summary>
		internal ScriptRuntime ScriptRuntime { get; private set; }


        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="D3DEffectTechnique"></param>
        /// <param name="subsetCount"></param>
        public テクニック( エフェクト effect, EffectTechnique d3dEffectTechnique, int subsetCount )
        {
            D3DEffectTechnique = d3dEffectTechnique;

            描画するサブセットIDの集合 = new HashSet<int>();

            パスリスト = new Dictionary<string, パス>();

            if( !D3DEffectTechnique.IsValid )
                throw new InvalidMMEEffectShader例外( string.Format( "テクニック「{0}」の検証に失敗しました。", D3DEffectTechnique.Description.Name ) );
            
            
            // MMDPass アノテーション

            string mmdpass = getAnnotationString( D3DEffectTechnique, "MMDPass" );
            if( String.IsNullOrWhiteSpace( mmdpass ) )
            {
                テクニックを適用する描画対象 = MMDPass種別.オブジェクト本体;
            }
            else
            {
                mmdpass = mmdpass.ToLower();
                switch( mmdpass )
                {
                    case "object":
                        テクニックを適用する描画対象 = MMDPass種別.オブジェクト本体;
                        break;

                    case "object_ss":
                        テクニックを適用する描画対象 = MMDPass種別.オブジェクト本体_セルフ影あり;
                        break;

                    case "zplot":
                        テクニックを適用する描画対象 = MMDPass種別.セルフ影用Z値プロット;
                        break;

                    case "shadow":
                        テクニックを適用する描画対象 = MMDPass種別.影;
                        break;

                    case "edge":
                        テクニックを適用する描画対象 = MMDPass種別.エッジ;
                        break;

                    case "skinning":
                        テクニックを適用する描画対象 = MMDPass種別.スキニング;
                        break;

                    case "scene":
                        テクニックを適用する描画対象 = MMDPass種別.シーン;
                        break;

                    default:
                        throw new InvalidOperationException( "予期しない識別子" );
                }
            }
            

            // その他アノテーション

            テクスチャを使用する = getAnnotationBoolean( D3DEffectTechnique, "UseTexture" );

            スフィアマップを使用する = getAnnotationBoolean( D3DEffectTechnique, "UseSphereMap" );

            トゥーンレンダリングを使用する = getAnnotationBoolean( D3DEffectTechnique, "UseToon" );

            セルフ影を使用する = getAnnotationBoolean( D3DEffectTechnique, "UseSelfShadow" );

            乗算スフィアを使用する = getAnnotationBoolean( D3DEffectTechnique, "MulSphere" );


            // Subset アノテーション

            GetSubsets( D3DEffectTechnique, subsetCount );


            // Script アノテーション

            EffectVariable rawScript = EffectParseHelper.アノテーションを取得する( D3DEffectTechnique, "Script" );

            for( int i = 0; i < D3DEffectTechnique.Description.PassCount; i++ )
            {
                EffectPass pass = D3DEffectTechnique.GetPassByIndex( i );
                パスリスト.Add( pass.Description.Name, new パス( effect, pass ) );
            }


            // Scriptランタイムの生成

            if( rawScript != null )
            {
                ScriptRuntime = new ScriptRuntime( rawScript.AsString().GetString(), effect, this );   // Script アノテーションがあれば登録する
            }
            else
            {
                ScriptRuntime = new ScriptRuntime( "", effect, this );
            }
        }

        public void Dispose()
        {
            D3DEffectTechnique?.Dispose();
            D3DEffectTechnique = null;
        }

		public void パスの適用と描画をパスの数だけ繰り返す<T>( Action<T> drawAction, T drawTarget )
		{
			if( string.IsNullOrWhiteSpace( ScriptRuntime.ScriptCode ) )
			{
                // このテクニックに Script が存在しないならパスで描画する
                foreach( パス pass in パスリスト.Values )
                    pass.適用して描画する( drawAction, drawTarget );
			}
			else
			{
                // このテクニックに Script が存在する場合は、処理をスクリプトランタイムに任せる
                ScriptRuntime.実行する( drawAction, drawTarget );
			}
		}


        /// <summary>
        ///     両方の bool 値が同じであれば true を返す。
        ///     ただし、<paramref name="teqValue"/> が <see cref="三状態Boolean.無視"/> なら常に true を返す。
        /// </summary>
        /// <param name="teqValue"></param>
        /// <param name="subsetValue"></param>
        /// <returns></returns>
		public static bool CheckExtebdedBoolean( 三状態Boolean teqValue, bool subsetValue )
		{
			if( subsetValue )
			{
				return teqValue != 三状態Boolean.無効;  // 有効 or 無視で true
			}
			return teqValue != 三状態Boolean.有効;      // 無効 or 無視で true
		}


        private void GetSubsets( EffectTechnique technique, int subsetCount )
        {
            string subset = getAnnotationString( technique, "Subset" );

            //Subsetの解析
            if( string.IsNullOrWhiteSpace( subset ) )
            {
                for( int i = 0; i <= subsetCount; i++ ) //指定しない場合は全てがレンダリング対象のサブセットとなる
                {
                    描画するサブセットIDの集合.Add( i );
                }
            }
            else
            {
                string[] chunks = subset.Split( ',' ); //,でサブセットアノテーションを分割
                foreach( string chunk in chunks )
                {
                    if( chunk.IndexOf( '-' ) == -1 ) //-がない場合はそれが単体であると認識する
                    {
                        int value = 0;
                        if( int.TryParse( chunk, out value ) )
                        {
                            描画するサブセットIDの集合.Add( value );
                        }
                        else
                        {
                            throw new InvalidMMEEffectShader例外(
                                string.Format( "テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」は認識されません。",
                                    technique.Description.Name, subset, chunk ) );
                        }
                    }
                    else
                    {
                        string[] regions = chunk.Split( '-' ); //-がある場合は範囲指定と認識する。
                        if( regions.Length > 2 )
                            throw new InvalidMMEEffectShader例外(
                                string.Format( "テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」には\"-\"が2つ以上存在します。",
                                    technique.Description.Name, subset, chunk ) );
                        if( string.IsNullOrWhiteSpace( regions[ 1 ] ) ) //この場合、X-の形だと認識される。
                        {
                            int value = 0;
                            if( int.TryParse( regions[ 0 ], out value ) )
                            {
                                for( int i = value; i <= subsetCount; i++ )
                                {
                                    描画するサブセットIDの集合.Add( i );
                                }
                            }
                            else
                            {
                                throw new InvalidMMEEffectShader例外(
                                    string.Format( "テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」の「{3}」は認識されません。",
                                        technique.Description.Name, subset, chunk, regions[ 0 ] ) );
                            }
                        }
                        else //この場合X-Yの形式だと認識される
                        {
                            int value1 = 0;
                            int value2 = 0;
                            if( int.TryParse( regions[ 0 ], out value1 ) && int.TryParse( regions[ 1 ], out value2 ) )
                            {
                                for( int i = value1; i <= value2; i++ )
                                {
                                    描画するサブセットIDの集合.Add( i );
                                }
                            }
                            else
                            {
                                throw new InvalidMMEEffectShader例外(
                                    string.Format(
                                        "テクニック「{0}」のサブセット解析中にエラーが発生しました。「{1}」中の「{2}」の「{3}」もしくは「{4}」は認識されません。",
                                        technique.Description.Name, subset, chunk, regions[ 0 ], regions[ 1 ] ) );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     指定したテクニックの指定したアノテーションを文字列で取得します
        /// </summary>
        /// <param name="technique">テクニック</param>
        /// <param name="attrName">アノテーション名</param>
        /// <returns>値</returns>
        public string getAnnotationString( EffectTechnique technique, string attrName )
        {
            EffectVariable annotationVariable = EffectParseHelper.アノテーションを取得する( technique, attrName );

            if( annotationVariable == null )
                return "";

            return annotationVariable.AsString().GetString();
        }

        /// <summary>
        ///     指定したテクニックの指定したアノテーションをBool値で取得します
        /// </summary>
        /// <param name="technique">テクニック</param>
        /// <param name="attrName">アノテーション名</param>
        /// <returns>値</returns>
        public 三状態Boolean getAnnotationBoolean( EffectTechnique technique, string attrName )
        {
            EffectVariable annotationVariable = EffectParseHelper.アノテーションを取得する( technique, attrName );

            if( annotationVariable == null )
                return 三状態Boolean.無視;

            int annotation = annotationVariable.AsScalar().GetInt();

            if( annotation == 1 )
            {
                return 三状態Boolean.有効;
            }

            return 三状態Boolean.無効;
        }
    }
}
