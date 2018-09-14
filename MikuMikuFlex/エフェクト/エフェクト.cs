using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MikuMikuFlex.DeviceManagement;
using MikuMikuFlex.エフェクト.Includer;
using MikuMikuFlex.エフェクト.変数管理;
using MikuMikuFlex.エフェクト.変数管理.定数;
using MikuMikuFlex.エフェクト.変数管理.コントロール;
using MikuMikuFlex.エフェクト.変数管理.材質;
using MikuMikuFlex.エフェクト.変数管理.行列;
using MikuMikuFlex.エフェクト.変数管理.マウス;
using MikuMikuFlex.エフェクト.変数管理.特殊パラメータ;
using MikuMikuFlex.エフェクト.変数管理.スクリーン情報;
using MikuMikuFlex.エフェクト.変数管理.テクスチャ;
using MikuMikuFlex.エフェクト.変数管理.時間;
using MikuMikuFlex.エフェクト.変数管理.ワールド情報;
using MikuMikuFlex.モデル;
using MikuMikuFlex.Utility;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト
{
    /// <summary>
    ///     MMEのエフェクトを管理するクラス
    /// </summary>
    public class エフェクト : IDisposable
    {
        // 全インスタンスで共有（const, static）

        public const string エフェクトに定義するMMF定数 = "MMF";

        public const string 既定のシェーダのリソースパス = "MikuMikuFlex.Resource.Shader.DefaultShader.fx";

        public static string アプリケーションの定数
        {
            get => _アプリケーションの定数;
            set
            {
                if( _アプリケーションの定数 != value )
                {
                    //前と違う値がセットされた場合、マクロのリストをすり替える

                    マクロリスト.Remove( new ShaderMacro( _アプリケーションの定数, null ) );

                    _アプリケーションの定数 = value;

                    マクロリスト.Add( new ShaderMacro( _アプリケーションの定数, null ) );
                }
            }
        }

        /// <summary>
        ///     現在の MMF で利用可能なすべてのエフェクト変数管理インスタンスのリスト。
        ///     [Key: 変数管理セマンティクス名、Value: 変数管理]
        /// </summary>
        public static Dictionary<string, 変数管理.変数管理> 変数管理マスタリスト { get; private set; }

        /// <summary>
        ///     現在の MMF で利用可能なすべての特殊パラメータ変数管理インスタンスのリスト。
        ///     [Key: 変数名、Value: 変数管理]
        /// </summary>
        public static Dictionary<string, 特殊パラメータ変数> 特殊パラメータ変数管理マスタリスト { get; private set; }

        /// <summary>
        ///     全エフェクトで共有するマクロのリスト。
        /// </summary>
        public static List<ShaderMacro> マクロリスト { get; private set; }

        /// <summary>
        ///     全エフェクトで共有する Include 。
        /// </summary>
        public static Include Include { get; private set; }


        /// <summary>
        ///     エフェクトシェーダーコードを記したファイルパスまたはリソースパス。
        /// </summary>
        public string ファイル名 { get; private set; }

        /// <summary>
        ///     対応する Direct3D11 Effect インスタンス。
        /// </summary>
        public Effect D3DEffect { get; private set; }

        /// <summary>
        ///     このエフェクトに存在するテクニックのリスト。
        /// </summary>
        public List<テクニック> テクニックリスト { get; private set; }

        /// <summary>
        ///     "string ScriptOutput" アノテーションの内容。省略可能。
        ///     "color" 以外の値は指定できない。デフォルト値もこの値である。
        /// </summary>
        public string ScriptOutput { get; private set; }

        /// <summary>
        ///     "string ScriptClass" アノテーションの内容。省略可能。
        ///     このエフェクトファイルの目的（何を描画するエフェクトか）を指定する。
        ///     以下のうちいずれかを指定する。
        ///         "object"        : オブジェクトを描画する。（デフォルト）
        ///         "scene"         : スクリーンバッファを描画する。
        ///         "sceneorobject" : 上記の両方。
        /// </summary>
        /// <remarks>
        ///     基本的には、通常のオブジェクト描画用のエフェクトでは、"object"を指定し、
        ///     プリエフェクト、ポストエフェクトでは、"scene"を指定する。
        ///     "object"を指定した場合、パスのスクリプトで Draw = Buffer を実行してはならない。
        ///     また、"scene"を指定した場合、 Draw=Geometry を実行してはならない。
        ///     "sceneorobject"を指定した場合は、両方を実行できる。
        /// </remarks>
        public ScriptClass ScriptClass { get; private set; }

        /// <summary>
        ///     "string ScriptOrder" アノテーションの内容。省略可能。
        ///     このエフェクトファイルの実行タイミングを指定する。
        ///     以下のうちいずれかを指定する。
        ///         "standard"    : オブジェクトを描画する。（デフォルト）
        ///         "preprocess"  : オブジェクトの描画よりも先に描画する。プリエフェクト用。
        ///         "postprocess" : オブジェクトの描画の後で描画する。ポストエフェクト用。
        /// </summary>
        /// <remarks>
        ///     ※正確には、preprocessのさらに前に、postprocessの前処理（テクニックのスクリプトの先頭から
        ///     ScriptExternal=Colorまで）が実行される。
        /// </remarks>
        public ScriptOrder ScriptOrder { get; private set; }

        /// <summary>
        ///     "string Script" アノテーションの内容。省略可能。
        ///     使用するテクニックの検索順序を指定する。
        ///     通常は、エフェクトファイルに記述されている順に使用可能なテクニックが検索されるが、
        ///     このアノテーションを用いると、その検索順序を明示的に指定できる。
        ///     
        ///     以下の書式で指定する。
        ///         "Technique=Technique?テクニック名1:テクニック名2:～;"
        ///         
        ///     例：string Script = "Technique=Technique?SimplePS:TexturedPS:SimpleQuadraticPS:TexturedQuadraticPS;";
        ///     
        ///     なお、使用するテクニックが１つの場合には、以下のようにも指定できる。
        ///         string Script = "Technique=MainTech;";
        /// </summary>
        public string STANDARDSGLOBALScript { get; private set; }

        public サブリソースローダー テクスチャなどのパスの解決に利用するローダー { get; private set; }

        #region " ScriptClass.Object 用メンバ "
        //----------------

        public Dictionary<EffectVariable, 変数管理.変数管理> モデルごとに更新するエフェクト変数のマップ { get; private set; }

        public Dictionary<EffectVariable, 変数管理.変数管理> 材質ごとに更新するエフェクト変数のマップ { get; private set; }

        public Dictionary<EffectVariable, 特殊パラメータ変数> 材質ごとに更新する特殊エフェクト変数のマップ { get; private set; }

        //----------------
        #endregion

        #region " ScriptClass.Scene 用メンバ "
        //----------------

        public Dictionary<EffectVariable, 変数管理.変数管理> シーンごとに更新するエフェクト変数のマップ { get; private set; }

        public Dictionary<string, RenderTargetView> レンダーターゲットビューのマップ { get; private set; }

        public Dictionary<string, DepthStencilView> 深度ステンシルビューのマップ { get; private set; }

        //----------------
        #endregion


        /// <summary>
        ///     プライベートコンストラクタ
        /// </summary>
        /// <param name="ファイル名">エフェクトの作成元のファイルパスまたはリソースパス。</param>
        /// <param name="context"></param>
        /// <param name="d3dEffect">管理対象のエフェクト</param>
        /// <param name="model">使用対象のモデル</param>
        /// <param name="loader"></param>
        private エフェクト( string ファイル名, Effect d3dEffect, IDrawable model, サブリソースローダー loader )
        {
            this.ファイル名 = ファイル名;
            テクスチャなどのパスの解決に利用するローダー = loader;

            D3DEffect = d3dEffect;

            ScriptOutput = "color";
            ScriptClass = ScriptClass.Object;
            ScriptOrder = ScriptOrder.Standard;
            STANDARDSGLOBALScript = "";

            var D3Dテクニックリスト = new List<EffectTechnique>();
            for( int i = 0; i < d3dEffect.Description.GlobalVariableCount; i++ )
            {
                EffectVariable variable = d3dEffect.GetVariableByIndex( i );

                if( variable.Description.Semantic != null && variable.Description.Semantic.ToUpper().Equals( "STANDARDSGLOBAL" ) )
                {
                    _STANDARDSGLOBALセマンティクスを解析する( d3dEffect, variable, out D3Dテクニックリスト );
                    break;
                }
            }

            材質ごとに更新するエフェクト変数のマップ = new Dictionary<EffectVariable, 変数管理.変数管理>();
            モデルごとに更新するエフェクト変数のマップ = new Dictionary<EffectVariable, 変数管理.変数管理>();
            材質ごとに更新する特殊エフェクト変数のマップ = new Dictionary<EffectVariable, 特殊パラメータ変数>();
            シーンごとに更新するエフェクト変数のマップ = new Dictionary<EffectVariable, 変数管理.変数管理>();
            テクニックリスト = new List<テクニック>();
            レンダーターゲットビューのマップ = new Dictionary<string, RenderTargetView>();
            深度ステンシルビューのマップ = new Dictionary<string, DepthStencilView>();
            _利用対象のモデル = model;

            // グローバル変数の登録
            int valCount = d3dEffect.Description.GlobalVariableCount;
            for( int i = 0; i < valCount; i++ )
            {
                EffectVariable variable = d3dEffect.GetVariableByIndex( i );

                string セマンティクス = Regex.Replace( variable.Description.Semantic ?? "", "[0-9]", "" ).ToUpper();
                string semanticIndexStr = Regex.Replace( variable.Description.Semantic ?? "", "[^0-9]", "" );
                int セマンティクス番号 = string.IsNullOrEmpty( semanticIndexStr ) ? 0 : int.Parse( semanticIndexStr );

                string 型名 = variable.TypeInfo.Description.TypeName.ToLower();

                if( 変数管理マスタリスト.ContainsKey( セマンティクス ) )
                {
                    // (A) 通常はセマンティクスに応じて登録する。

                    変数管理.変数管理 subs = 変数管理マスタリスト[ セマンティクス ];
                    subs.指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( variable );
                    if( subs.更新タイミング == 更新タイミング.材質ごと )
                    {
                        材質ごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, セマンティクス番号 ) );
                    }
                    else
                    {
                        モデルごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, セマンティクス番号 ) );
                    }
                }
                else if( 型名.Equals( "texture" ) || 型名.Equals( "texture2d" ) || 型名.Equals( "texture3d" ) || 型名.Equals( "texturecube" ) )
                {
                    // (B) テクスチャのみ例外で、変数型に応じて登録する。

                    変数管理.変数管理 subs = new テクスチャ変数();
                    subs.指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( variable );

                    switch( セマンティクス )
                    {
                        case "CURRENTSCENECOLOR":
                            subs = new CURRENTSCENECOLOR変数();
                            subs.指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( variable );
                            シーンごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, セマンティクス番号 ) );
                            break;

                        case "CURRENTSCENEDEPTHSTENCIL":
                            subs = new CURRENTSCENEDEPTHSTENCIL変数();
                            subs.指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( variable );
                            シーンごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, セマンティクス番号 ) );
                            break;

                        default:
                            モデルごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, セマンティクス番号 ) );
                            break;
                    }
                }
                else
                {
                    // (C) 特殊変数は変数名に応じて登録する。

                    string name = variable.Description.Name.ToLower();

                    if( 特殊パラメータ変数管理マスタリスト.ContainsKey( name ) )
                    {
                        材質ごとに更新する特殊エフェクト変数のマップ.Add( variable, 特殊パラメータ変数管理マスタリスト[ name ] );
                    }
                }
            }

            // 定数バッファの登録

            valCount = d3dEffect.Description.ConstantBufferCount;
            for( int i = 0; i < valCount; i++ )
            {
                EffectConstantBuffer variable = d3dEffect.GetConstantBufferByIndex( i );
                string name = variable.Description.Name.ToUpper();

                if( 変数管理マスタリスト.ContainsKey( name ) )
                {
                    変数管理.変数管理 subs = 変数管理マスタリスト[ name ]; //定数バッファにはセマンティクスが付けられないため、変数名で代用
                    subs.指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( variable );
                    if( subs.更新タイミング == 更新タイミング.材質ごと )
                    {
                        材質ごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, 0 ) );
                    }
                    else
                    {
                        モデルごとに更新するエフェクト変数のマップ.Add( variable, subs.変数登録インスタンスを生成して返す( variable, this, 0 ) );
                    }
                }
            }

            // テクニックの登録

            for( int i = 0; i < d3dEffect.Description.TechniqueCount; i++ )
            {
                var tech = d3dEffect.GetTechniqueByIndex( i );

                if( D3Dテクニックリスト.FindIndex( ( t ) => ( t.Description.Name == tech.Description.Name ) ) == -1 )
                {
                    D3Dテクニックリスト.Add( tech );
                }
            }

            int subsetCount = model is ISubsetDivided ? ( (ISubsetDivided) model ).サブセット数 : 1;
            foreach( EffectTechnique d3dTech in D3Dテクニックリスト )
            {
                テクニックリスト.Add( new テクニック( this, d3dTech, subsetCount ) );
            }
            D3Dテクニックリスト.Clear();
        }

        public void Dispose()
        {
            foreach( var tech in テクニックリスト )
                tech.Dispose();
            テクニックリスト.Clear();

            D3DEffect?.Dispose();
            D3DEffect = null;

            foreach( KeyValuePair<EffectVariable, 特殊パラメータ変数> kvp in 材質ごとに更新する特殊エフェクト変数のマップ )
            {
                if( kvp.Value is IDisposable )
                {
                    var disposeTarget = (IDisposable) kvp.Value;
                    disposeTarget?.Dispose();
                }
                kvp.Key.Dispose();
            }
            foreach( KeyValuePair<EffectVariable, 変数管理.変数管理> kvp in 材質ごとに更新するエフェクト変数のマップ )
            {
                if( kvp.Value is IDisposable )
                {
                    var disposeTarget = (IDisposable) kvp.Value;
                    disposeTarget?.Dispose();
                }
                kvp.Key.Dispose();
            }
            foreach( KeyValuePair<EffectVariable, 変数管理.変数管理> kvp in this.モデルごとに更新するエフェクト変数のマップ )
            {
                if( kvp.Value is IDisposable )
                {
                    var disposeTarget = (IDisposable) kvp.Value;
                    disposeTarget?.Dispose();
                }
                kvp.Key.Dispose();
            }

            レンダーターゲットビューのマップ.Clear(); // Dispose は変数管理側で行われる
            深度ステンシルビューのマップ.Clear();     //
        }

        public static void 初期化する( DeviceManager deviceManager )
        {
            変数管理マスタリスト = new Dictionary<string, 変数管理.変数管理>();
            foreach( var variable in new List<変数管理.変数管理> {
                new WORLD変数(),
                new PROJECTION変数(),
                new VIEW変数(),
                new WORLDINVERSE変数(),
                new WORLDTRANSPOSE変数(),
                new WORLDINVERSETRANSPOSE変数(),
                new VIEWINVERSE変数(),
                new VIEWTRANSPOSE変数(),
                new VIEWINVERSETRANSPOSE変数(),
                new PROJECTIONINVERSE変数(),
                new PROJECTIONTRANSPOSE変数(),
                new PROJECTIONINVERSETRANSPOSE変数(),
                new WORLDVIEW変数(),
                new WORLDVIEWINVERSE変数(),
                new WORLDVIEWTRANSPOSE変数(),
                new VIEWPROJECTION変数(),
                new VIEWPROJECTIONINVERSE変数(),
                new VIEWPROJECTIONTRANSPOSE変数(),
                new VIEWPROJECTIONINVERSETRANSPOSE変数(),
                new WORLDVIEWPROJECTION変数(),
                new WORLDVIEWPROJECTIONINVERSE変数(),
                new WORLDVIEWPROJECTIONTRANSPOSE変数(),
                new WORLDVIEWPROJECTIONINVERSETRANSPOSE変数(),
                //マテリアル
                new DIFFUSE変数(),
                new AMBIENT変数(),
                new SPECULAR変数(),
                new SPECULARPOWER変数(),
                new TOONCOLOR変数(),
                new EDGECOLOR変数(),
                new GROUNDSHADOWCOLOR変数(),
                new MATERIALTEXTURE変数(),
                new MATERIALSPHEREMAP変数(),
                new MATERIALTOONTEXTURE変数(),
                new ADDINGTEXTURE変数(),
                new MULTIPLYINGTEXTURE変数(),
                new ADDINGSPHERETEXTURE変数(),
                new MULTIPLYINGSPHERETEXTURE変数(),
                new EDGEWIDTH変数(),
                //位置/方向
                new POSITION変数(),
                new DIRECTION変数(),
                //時間
                new TIME変数(),
                new ELAPSEDTIME変数(),
                //マウス
                new MOUSEPOSITION変数(),
                new LEFTMOUSEDOWN変数(),
                new MIDDLEMOUSEDOWN変数(),
                new RIGHTMOUSEDOWN変数(),
                //スクリーン情報
                new VIEWPORTPIXELSIZE変数(),
                //定数バッファ
                new BASICMATERIALCONSTANT変数(),
                new FULLMATERIALCONSTANT変数(),
                //コントロールオブジェクト
                new CONTROLOBJECT変数(),
                // テクスチャ …… は型で判別するので、このリストに加えないこと。
                //new CURRENTSCENECOLOR変数(),
                //new RENDERCOLORTARGET変数(),
                //new RENDERDEPTHSTENCILTARGET変数(),
                // テッセレーション
                new TESSFACTOR変数(),
            } )
            {
                変数管理マスタリスト.Add( variable.セマンティクス, variable );
            }

            特殊パラメータ変数管理マスタリスト = new Dictionary<string, 特殊パラメータ変数>();
            foreach( var variable in new List<特殊パラメータ変数> {
                new opadd変数(),
                new parthf変数(),
                new spadd変数(),
                new SubsetCount変数(),
                new transp変数(),
                new use_spheremap変数(),
                new use_texture変数(),
                new use_toontexturemap変数(),
                new VertexCount変数()
            } )
            {
                特殊パラメータ変数管理マスタリスト.Add( variable.変数名, variable );
            }

            マクロリスト = new List<ShaderMacro>() {
                new ShaderMacro( エフェクトに定義するMMF定数, "" ),  // 定数 'MMF' を定義
                new ShaderMacro( アプリケーションの定数, "" ),       // アプリケーションの定数を定義
                new ShaderMacro( "MMM_LightCount", "3" ),           // 最大ライト数（MMM準拠で 3）
                ( deviceManager.DeviceFeatureLevel == FeatureLevel.Level_11_0 ) ?
                    new ShaderMacro( "DX_LEVEL_11_0", "" ) :
                    new ShaderMacro( "DX_LEVEL_10_1", "" ),
            };

            Include = new BasicEffectIncluder();
        }

        public static エフェクト ファイルをエフェクトとして読み込む( string ファイルパス, IDrawable 使用対象モデル, サブリソースローダー loader = null )
        {
            if( null == loader )
                loader = new サブリソースローダー( Path.GetDirectoryName( ファイルパス ) );

            var d3dEffect = CGHelper.EffectFx5を作成する( ファイルパス, (Device) RenderContext.Instance.DeviceManager.D3DDevice );

            return new エフェクト( ファイルパス, d3dEffect, 使用対象モデル, loader );
        }

        public static エフェクト リソースをエフェクトとして読み込む( string リソースパス, IDrawable 使用対象モデル, サブリソースローダー loader )
        {
            var d3dEffect = CGHelper.EffectFx5を作成するFromResource( リソースパス, (Device) RenderContext.Instance.DeviceManager.D3DDevice );

            return new エフェクト( リソースパス, d3dEffect, 使用対象モデル, loader );
        }


        #region " ScriptClass.Object 用メンバ "
        //----------------

        public void モデルごとに更新するエフェクト変数を更新する()
        {
            if( !( this.ScriptClass.HasFlag( ScriptClass.Object ) ) )
                return;

            var argument = new 変数更新時引数( _利用対象のモデル );

            foreach( KeyValuePair<EffectVariable, 変数管理.変数管理> subscriberBase in this.モデルごとに更新するエフェクト変数のマップ )
            {
                subscriberBase.Value.変数を更新する( subscriberBase.Key, argument );
            }
        }

        public void 材質ごとに更新するエフェクト変数と特殊エフェクト変数を更新する( エフェクト用材質情報 info )
        {
            if( !( this.ScriptClass.HasFlag( ScriptClass.Object ) ) )
                return;

            var argument = new 変数更新時引数( info, _利用対象のモデル );

            foreach( KeyValuePair<EffectVariable, 変数管理.変数管理> item in this.材質ごとに更新するエフェクト変数のマップ )
            {
                item.Value.変数を更新する( item.Key, argument );
            }

            foreach( KeyValuePair<EffectVariable, 特殊パラメータ変数> peculiarValueSubscriberBase in 材質ごとに更新する特殊エフェクト変数のマップ )
            {
                peculiarValueSubscriberBase.Value.変数を更新する( peculiarValueSubscriberBase.Key, argument );
            }
        }

        public void エフェクトを適用しつつサブセットを描画する( サブセット ipmxSubset, MMDPass種別 passType, Action<サブセット> drawAction )
        {
            if( ipmxSubset.エフェクト用材質情報.拡散色.W == 0 ||
                !( this.ScriptClass.HasFlag( ScriptClass.Object ) ) )
                return;

            // 両面描画かどうかに応じてカリングの値を切り替える

            if( passType == MMDPass種別.エッジ )
            {
                RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.裏側片面描画の際のラスタライザステート;
            }
            else if( ipmxSubset.カリングする )
            {
                if( ipmxSubset.エフェクト用材質情報.Line描画を使用する )
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.片面描画の際のラスタライザステートLine;
                else
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.片面描画の際のラスタライザステート;
            }
            else
            {
                if( ipmxSubset.エフェクト用材質情報.Line描画を使用する )
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.両面描画の際のラスタライザステートLine;
                else
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.両面描画の際のラスタライザステート;
            }


            // 使用するtechniqueを検索する

            テクニック technique =
                ( from teq in テクニックリスト
                  where
                    teq.描画するサブセットIDの集合.Contains( ipmxSubset.サブセットID ) &&
                    teq.テクニックを適用する描画対象 == passType
                  select teq ).FirstOrDefault();

            if( null != technique )
            {
                // 最初の１つだけ有効（複数はないはずだが）
                technique.パスの適用と描画をパスの数だけ繰り返す( drawAction, ipmxSubset );
            }
        }

        //----------------
        #endregion

        #region " ScriptClass.Scene 用メンバ "
        //----------------

        public void シーンごとに更新するエフェクト変数を更新する( SharpDX.DXGI.SwapChain swapChain )
        {
            if( !( this.ScriptClass.HasFlag( ScriptClass.Scene ) ) )
                return;

            var argument = new 変数更新時引数( swapChain );

            foreach( KeyValuePair<EffectVariable, 変数管理.変数管理> kvp in this.シーンごとに更新するエフェクト変数のマップ )
                kvp.Value.変数を更新する( kvp.Key, argument );
        }

        public void エフェクトを適用しつつシーンを描画する()
        {
            if( !( this.ScriptClass.HasFlag( ScriptClass.Scene ) ) )
                return;

            // カリング設定

            RenderContext.Instance.DeviceManager.D3DDeviceContext.Rasterizer.State = RenderContext.Instance.片面描画の際のラスタライザステート;

            // 使用するtechniqueを検索する。

            テクニック technique =
                ( from teq in テクニックリスト
                  where
                    teq.テクニックを適用する描画対象 == MMDPass種別.シーン
                  select teq ).FirstOrDefault();

            if( null != technique )
            {
                // 最初の１つだけ有効（複数はないはずだが）
                technique.パスの適用と描画をパスの数だけ繰り返す<int>( ( dummy ) => {

                    // 頂点バッファとインデックスバッファを使わずに 4 つの頂点の描画を指示する。
                    // 頂点シェーダーでは、頂点番号（0～3）を SV_VertexID で受け取ることができる。
                    RenderContext.Instance.DeviceManager.D3DDeviceContext.Draw( 4, 0 );

                }, 0 );
            }
        }

        //----------------
        #endregion


        private readonly IDrawable _利用対象のモデル;

        private static string _アプリケーションの定数 = "MMFApp";

        private void _STANDARDSGLOBALセマンティクスを解析する( Effect d3dEffect, EffectVariable variable, out List<EffectTechnique> techniqueList )
        {
            techniqueList = new List<EffectTechnique>();

            if( !variable.Description.Name.ToLower().Equals( "script" ) )
                throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数名は\"Script\"である必要があります、指定された変数名は\"{variable.Description.Name}\"でした。" );

            if( !variable.TypeInfo.Description.TypeName.ToLower().Equals( "float" ) )
                throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数型は\"float\"である必要があります、指定された変数名は\"{variable.TypeInfo.Description.TypeName.ToLower()}\"でした。" );

            if( variable.AsScalar().GetFloat() != 0.8f )
                throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される値は\"0.8\"である必要があります、指定された値は\"{variable.AsScalar().GetFloat()}\"でした。" );


            // ScriptOutput

            EffectVariable scriptOutput値 = EffectParseHelper.アノテーションを取得する( variable, "ScriptOutput", "string" );

            if( scriptOutput値 != null )
            {
                if( !scriptOutput値.AsString().GetString().Equals( "color" ) )
                    throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のアノテーション「string ScriptOutput」は、\"color\"でなくてはなりません。指定された値は\"{scriptOutput値.AsString().GetString().ToLower()}\"でした。" );
            }


            // ScriptClass

            EffectVariable scriptClass値 = EffectParseHelper.アノテーションを取得する( variable, "ScriptClass", "string" );

            if( scriptClass値 != null )
            {
                string sc = scriptClass値.AsString().GetString();

                switch( sc.ToLower() )
                {
                    case "object":
                        ScriptClass = ScriptClass.Object;
                        break;

                    case "scene":
                        ScriptClass = ScriptClass.Scene;
                        break;

                    case "sceneorobject":
                        ScriptClass = ScriptClass.Object | ScriptClass.Scene;
                        break;

                    default:
                        throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のアノテーション「string ScriptClass」は、\"object\",\"scene\",\"sceneorobject\"でなくてはなりません。指定された値は\"{sc.ToLower()}\"でした。(スペルミス?)" );
                }
            }


            // ScriptOrder

            EffectVariable scriptOrder値 = EffectParseHelper.アノテーションを取得する( variable, "ScriptOrder", "string" );

            if( scriptOrder値 != null )
            {
                string sor = scriptOrder値.AsString().GetString();

                switch( sor.ToLower() )
                {
                    case "standard":
                        ScriptOrder = ScriptOrder.Standard;
                        break;

                    case "preprocess":
                        ScriptOrder = ScriptOrder.Preprocess;
                        break;

                    case "postprocess":
                        ScriptOrder = ScriptOrder.Postprocess;
                        break;

                    default:
                        throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のアノテーション「string ScriptOrder」は、\"standard\",\"preprocess\",\"postprocess\"でなくてはなりません。指定された値は\"{sor.ToLower()}\"でした。(スペルミス?)" );
                }
            }


            // Script

            var script値 = EffectParseHelper.アノテーションを取得する( variable, "Script", "string" );

            if( script値 != null )
            {
                STANDARDSGLOBALScript = script値.AsString().GetString();

                if( string.IsNullOrEmpty( STANDARDSGLOBALScript ) )
                {
                    // (A) Script アノテーションがない　＝　検索順序に指定がない

                    for( int i = 0; i < d3dEffect.Description.TechniqueCount; i++ )
                    {
                        techniqueList.Add( d3dEffect.GetTechniqueByIndex( i ) );
                    }
                }
                else
                {
                    // (B) 検索順序の指定がある

                    string[] scriptChunks = STANDARDSGLOBALScript.Split( ';' );

                    if( scriptChunks.Length == 1 )
                        throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{STANDARDSGLOBALScript}」は読み込めませんでした。\";\"が足りません。" );

                    string targetScript = scriptChunks[ scriptChunks.Length - 2 ]; // 最後のセミコロンが付いているスクリプト以外は無視

                    if( STANDARDSGLOBALScript.IndexOf( "?" ) == -1 )
                    {
                        // (B-a) '?' が含まれない時

                        string[] args = targetScript.Split( '=' );

                        if( args.Length > 2 )
                            throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{targetScript}」は読み込めませんでした。\"=\"の数が多すぎます。" );

                        if( !args[ 0 ].ToLower().Equals( "technique" ) )
                            throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{targetScript}」は読み込めませんでした。\"{args[ 0 ]}\"は\"Technique\"であるべきです。(スペルミス?)" );

                        EffectTechnique technique = d3dEffect.GetTechniqueByName( args[ 1 ] );

                        if( technique != null )
                        {
                            techniqueList.Add( technique );
                        }
                        else
                        {
                            throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{targetScript}」は読み込めませんでした。テクニック\"{args[ 1 ]}\"は存在しません。(スペルミス?)" );
                        }
                    }
                    else
                    {
                        // (B-b) '?' が含まれている時

                        string[] args = targetScript.Split( '?' );

                        if( args.Length == 2 )
                        {
                            string[] techniques = args[ 1 ].Split( ':' );

                            foreach( string technique in techniques )
                            {
                                EffectTechnique effectTechnique = d3dEffect.GetTechniqueByName( technique );

                                if( effectTechnique != null )
                                {
                                    techniqueList.Add( effectTechnique );
                                }
                                else
                                {
                                    throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{targetScript}」は読み込めませんでした。テクニック\"{technique}\"は見つかりません。(スペルミス?)" );
                                }
                            }
                        }
                        else if( args.Length > 2 )
                        {
                            throw new InvalidMMEEffectShader例外( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{targetScript}」は読み込めませんでした。\"?\"の数が多すぎます。" );
                        }
                    }

                    if( scriptChunks.Length > 2 )
                        System.Diagnostics.Debug.WriteLine( $"STANDARDSGLOBALセマンティクスの指定される変数のスクリプト「{STANDARDSGLOBALScript}」では、複数回Techniqueの代入が行われていますが、最後の代入以外は無視されます。" );
                }
            }
        }
    }
}
