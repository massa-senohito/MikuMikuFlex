using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

#pragma warning disable 0649

namespace MikuMikuFlex3
{
    public class PMXモデル : IDisposable
    {

        // 制御


        public const int 最大ボーン数 = 768;

        internal PMX頂点制御 PMX頂点制御 { get; private protected set; }

        internal PMXボーン制御[] PMXボーン制御リスト { get; private protected set; }

        internal List<PMXボーン制御> IKボーンリスト { get; private protected set; }

        internal PMX材質制御[] PMX材質制御リスト { get; private protected set; }

        internal PMXモーフ制御[] PMXモーフ制御リスト { get; private protected set; }



        // 生成と終了


        /// <summary>
        ///     ファイルからPMXモデルを読み込む。
        /// </summary>
        /// <remarks>
        ///     PMXファイルで使用されるテクスチャ等のリソースファイルは、
        ///     PMXファイルと同じフォルダを基準として検索される。
        /// </remarks>
        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, string PMXファイルパス )
        {
            var stream = new FileStream( PMXファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read );

            this._読み込んで初期化する( d3dDevice, stream, リソースを開く: ( file ) => {

                var baseFolder = Path.GetDirectoryName( PMXファイルパス );
                var path = Path.Combine( baseFolder, file );
                return new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read );

            } );

            // stream は、上記初期化作業の中で閉じられる。
        }

        /// <summary>
        ///     埋め込みリソースからPMXモデルを読み込む。
        /// </summary>
        /// <remarks>
        ///     PMXリソースで使用されるテクスチャ等のリソースは、
        ///     PMXリソースと同じ名前空間を基準として検索される。
        /// </remarks>
        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, Type 名前空間を示す型, string リソース名 )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = $"{this.GetType().Namespace}.{リソース名}";

            var stream = assembly.GetManifestResourceStream( path );

            this._読み込んで初期化する( d3dDevice, stream, リソースを開く: ( resource ) => {

                // PMXではテクスチャ名などにパス区切り文字を使用できるが、その区切りがなんであるかはOSに依存して
                // PMXでは感知しないとのことなので、とりあえず '/' と '\' を想定する。
                var rpath = resource.Replace( Path.DirectorySeparatorChar, '.' ).Replace( Path.AltDirectorySeparatorChar, '.' );    // '.' 区切りに変換

                return assembly.GetManifestResourceStream( 名前空間を示す型, rpath );

            } );

            // stream は、上記初期化作業の中で閉じられる。
        }

        private void _読み込んで初期化する( SharpDX.Direct3D11.Device d3dDevice, Stream PMXデータ, Func<string, Stream> リソースを開く )
        {
            //Task.Run( () => {
            {
                #region " モデルを読み込む。"
                //----------------
                this._PMXFモデル = new PMXFormat.モデル( PMXデータ );

                if( this._PMXFモデル.ボーンリスト.Count > 最大ボーン数 )
                    throw new Exception( "ボーン数が多すぎます。" );

                PMXデータ.Dispose();
                //----------------
                #endregion

                #region " PMXボーン制御リストを作成する。"
                //----------------
                {
                    int ボーン数 = this._PMXFモデル.ボーンリスト.Count;

                    this.PMXボーン制御リスト = new PMXボーン制御[ ボーン数 ];

                    for( int i = 0; i < ボーン数; i++ )
                        this.PMXボーン制御リスト[ i ] = new PMXボーン制御( this._PMXFモデル.ボーンリスト[ i ], i );

                    for( int i = 0; i < ボーン数; i++ )
                        this.PMXボーン制御リスト[ i ].読み込み後の処理を行う( this.PMXボーン制御リスト );
                }
                //----------------
                #endregion
                #region " IKボーンリストを作成する。"
                //----------------
                {
                    var ikBones = this.PMXボーン制御リスト.Where( ( bone ) => bone.PMXFボーン.IKボーンである );

                    this.IKボーンリスト = new List<PMXボーン制御>( ikBones.Count() );
                    for( int i = 0; i < ikBones.Count(); i++ )
                        this.IKボーンリスト.Add( ikBones.ElementAt( i ) );
                }
                //----------------
                #endregion
                #region " ボーンのルートリストを作成する。"
                //----------------
                {
                    this._ルートボーンリスト = new List<PMXボーン制御>();

                    for( int i = 0; i < this.PMXボーン制御リスト.Length; i++ )
                    {
                        // 親ボーンを持たないのがルートボーン。
                        if( this.PMXボーン制御リスト[ i ].PMXFボーン.親ボーンのインデックス == -1 )
                        {
                            this._ルートボーンリスト.Add( this.PMXボーン制御リスト[ i ] );
                        }
                    }
                }
                //----------------
                #endregion
                #region " PMXボーンの変形階層を設定する。"
                //----------------
                {
                    foreach( var root in this._ルートボーンリスト )
                        設定( root, 0 );

                    void 設定( PMXボーン制御 bone, int layer )
                    {
                        bone.変形階層 = layer;
                        foreach( var child in bone.子ボーンリスト )
                            設定( child, layer + 1 );
                    }
                }
                //----------------
                #endregion
                #region " ボーンをソートする。"
                //----------------
                {
                    var comparison = new Comparison<PMXボーン制御>( ( x, y ) => {

                        // 後であればあるほどスコアが大きくなるように計算する

                        int xScore = 0;
                        int yScore = 0;
                        int BoneCount = this.PMXボーン制御リスト.Length;

                        if( x.PMXFボーン.物理後変形である )
                        {
                            xScore += BoneCount * BoneCount;
                        }
                        if( y.PMXFボーン.物理後変形である )
                        {
                            yScore += BoneCount * BoneCount;
                        }
                        xScore += BoneCount * x.変形階層;
                        yScore += BoneCount * y.変形階層;
                        xScore += x.ボーンインデックス;
                        yScore += y.ボーンインデックス;
                        return xScore - yScore;

                    } );

                    this.IKボーンリスト.Sort( comparison );
                    this._ルートボーンリスト.Sort( comparison );
                }
                //----------------
                #endregion
                #region " 親付与によるFKを初期化する。"
                //----------------
                {
                    this._親付与によるFK変形更新 = new 親付与によるFK変形更新( this.PMXボーン制御リスト );
                }
                //----------------
                #endregion
                #region " 物理変形を初期化する。"
                //----------------
                {
                    this._物理変形更新 = new PMX物理変形更新( this.PMXボーン制御リスト, this._PMXFモデル.剛体リスト, this._PMXFモデル.ジョイントリスト );
                }
                //----------------
                #endregion
                #region " PMX材質制御リストを作成する。"
                //----------------
                {
                    int 材質数 = this._PMXFモデル.材質リスト.Count;

                    this.PMX材質制御リスト = new PMX材質制御[ 材質数 ];

                    for( int i = 0; i < 材質数; i++ )
                        this.PMX材質制御リスト[ i ] = new PMX材質制御( this._PMXFモデル.材質リスト[ i ] );
                }
                //----------------
                #endregion
                #region " PMXモーフ制御リストを作成する。"
                //----------------
                {
                    int モーフ数 = this._PMXFモデル.モーフリスト.Count;

                    this.PMXモーフ制御リスト = new PMXモーフ制御[ モーフ数 ];

                    for( int i = 0; i < モーフ数; i++ )
                        this.PMXモーフ制御リスト[ i ] = new PMXモーフ制御( this._PMXFモデル.モーフリスト[ i ] );
                }
                //----------------
                #endregion

                #region " 既定のエフェクトを生成する。"
                //----------------
                {
                    var assembly = Assembly.GetExecutingAssembly();

                    using( var st = assembly.GetManifestResourceStream( this.GetType(), "Resources.Shaders.DefaultShader.cso" ) )
                    {
                        var effectByteCode = new byte[ st.Length ];
                        st.Read( effectByteCode, 0, (int) st.Length );

                        this._既定のEffect = new Effect( d3dDevice, effectByteCode );
                    }

                    this._エフェクト変数 = new エフェクト変数( this._既定のEffect );
                }
                //----------------
                #endregion
                #region " テクニックリストを生成する。"
                //----------------
                {
                    var techList = new List<EffectTechnique>();

                    for( int i = 0; i < this._既定のEffect.Description.TechniqueCount; i++ )
                    {
                        var tech = this._既定のEffect.GetTechniqueByIndex( i );

                        // 名前が重複しないテクニックのみ採択
                        if( techList.FindIndex( ( t ) => ( t.Description.Name == tech.Description.Name ) ) == -1 )
                            techList.Add( tech );
                    }

                    this._D3Dテクニックリスト = new List<テクニック>();

                    int subsetCount = this._PMXFモデル.材質リスト.Count;

                    foreach( var d3dTech in techList )
                    {
                        this._D3Dテクニックリスト.Add( new テクニック( this._既定のEffect, d3dTech, subsetCount ) );
                    }

                    techList.Clear();
                }
                //----------------
                #endregion

                #region " 入力頂点リストを生成する。"
                //----------------
                {
                    var 頂点リスト = new List<CS_INPUT>( this._PMXFモデル.頂点リスト.Count );

                    for( int i = 0; i < this._PMXFモデル.頂点リスト.Count; i++ )
                    {
                        this._頂点データを頂点レイアウトリストに追加する( this._PMXFモデル.頂点リスト[ i ], 頂点リスト );
                    }

                    this.PMX頂点制御 = new PMX頂点制御( 頂点リスト.ToArray() );
                }
                //----------------
                #endregion

                #region " スキニングバッファを作成する。"
                //----------------
                {
                    this._D3Dスキニングバッファ = new SharpDX.Direct3D11.Buffer(
                        d3dDevice,
                        new BufferDescription {
                            SizeInBytes = CS_INPUT.SizeInBytes * this.PMX頂点制御.入力頂点配列.Length,
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.ShaderResource,// | BindFlags.UnorderedAccess,
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.BufferStructured,   // 構造化バッファ
                            StructureByteStride = CS_INPUT.SizeInBytes,
                        } );

                    this._D3DスキニングバッファSRView = new ShaderResourceView(
                        d3dDevice,
                        this._D3Dスキニングバッファ,  // 構造化バッファ
                        new ShaderResourceViewDescription {
                            Format = Format.Unknown,
                            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                            BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource {
                                FirstElement = 0,
                                ElementCount = this.PMX頂点制御.入力頂点配列.Length,
                            },
                        } );
                }
                //----------------
                #endregion
                #region " 頂点バッファを作成する。"
                //----------------
                {
                    this._D3D頂点バッファ = new SharpDX.Direct3D11.Buffer(
                        d3dDevice,
                        new BufferDescription {
                            SizeInBytes = VS_INPUT.SizeInBytes * this.PMX頂点制御.入力頂点配列.Length,
                            Usage = ResourceUsage.Default,
                            BindFlags = BindFlags.VertexBuffer | BindFlags.ShaderResource | BindFlags.UnorderedAccess,  // 非順序アクセス
                            CpuAccessFlags = CpuAccessFlags.None,
                            OptionFlags = ResourceOptionFlags.BufferAllowRawViews,   // 生ビューバッファ
                        } );

                    this._D3D頂点バッファビューUAView = new UnorderedAccessView(
                        d3dDevice,
                        this._D3D頂点バッファ,
                        new UnorderedAccessViewDescription {
                            Format = Format.R32_Typeless,
                            Dimension = UnorderedAccessViewDimension.Buffer,
                            Buffer = new UnorderedAccessViewDescription.BufferResource {
                                FirstElement = 0,
                                ElementCount = VS_INPUT.SizeInBytes * this.PMX頂点制御.入力頂点配列.Length / 4,
                                Flags = UnorderedAccessViewBufferFlags.Raw,
                            },
                        } );
                }
                //----------------
                #endregion
                #region " インデックスバッファを作成する。"
                //----------------
                {
                    var インデックスリスト = new List<uint>();

                    foreach( PMXFormat.面 surface in this._PMXFモデル.面リスト )
                    {
                        インデックスリスト.Add( surface.頂点1 );
                        インデックスリスト.Add( surface.頂点2 );
                        インデックスリスト.Add( surface.頂点3 );
                    }

                    using( var dataStream = DataStream.Create( インデックスリスト.ToArray(), true, true ) )
                    {
                        this._D3Dインデックスバッファ = new SharpDX.Direct3D11.Buffer(
                            d3dDevice,
                            dataStream,
                            new BufferDescription {
                                BindFlags = BindFlags.IndexBuffer,
                                SizeInBytes = (int) dataStream.Length
                            } );
                    }
                }
                //----------------
                #endregion
                #region " 頂点レイアウトを作成する。"
                //----------------
                {
                    this._D3D頂点レイアウト = new InputLayout(
                        d3dDevice,
                        this._既定のEffect.GetTechniqueByName( "DefaultObject" ).GetPassByIndex( 0 ).Description.Signature,
                        VS_INPUT.VertexElements );
                }
                //----------------
                #endregion
                #region " ラスタライザステートを作成する。"
                //----------------
                {
                    this._片面描画の際のラスタライザステート = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Solid,
                    } );

                    this._両面描画の際のラスタライザステート = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Solid,
                    } );

                    this._片面描画の際のラスタライザステートLine = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                        CullMode = CullMode.Back,
                        FillMode = FillMode.Wireframe,
                    } );

                    this._両面描画の際のラスタライザステートLine = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                        CullMode = CullMode.None,
                        FillMode = FillMode.Wireframe,
                    } );

                    this._裏側片面描画の際のラスタライザステート = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                        CullMode = CullMode.Front,
                        FillMode = FillMode.Solid,
                    } );
                }
                //----------------
                #endregion

                #region " 共有テクスチャを読み込む。"
                //----------------
                {
                    this._共有テクスチャリスト = new (Texture2D tex2d, ShaderResourceView srv)[ 11 ];

                    var 共有テクスチャパス = new string[] {
                    @"Resources.Toon.toon0.bmp",
                    @"Resources.Toon.toon1.bmp",
                    @"Resources.Toon.toon2.bmp",
                    @"Resources.Toon.toon3.bmp",
                    @"Resources.Toon.toon4.bmp",
                    @"Resources.Toon.toon5.bmp",
                    @"Resources.Toon.toon6.bmp",
                    @"Resources.Toon.toon7.bmp",
                    @"Resources.Toon.toon8.bmp",
                    @"Resources.Toon.toon9.bmp",
                    @"Resources.Toon.toon10.bmp",
                };

                    var assembly = Assembly.GetExecutingAssembly();

                    for( int i = 0; i < 11; i++ )
                    {
                        this._共有テクスチャリスト[ i ] = (null, null);

                        var path = $"{this.GetType().Namespace}.{共有テクスチャパス[ i ]}";

                        try
                        {
                            if( null != assembly.GetManifestResourceInfo( path ) )
                            {
                                var stream = assembly.GetManifestResourceStream( path );
                                var srv = MMFShaderResourceView.FromStream( d3dDevice, stream, out var tex2d );

                                this._共有テクスチャリスト[ i ] = (tex2d, srv);
                            }
                        }
                        catch( Exception e )
                        {
                            Trace.TraceError( $"共有テクスチャの読み込みに失敗しました。[{path}][{e.Message}]" );
                        }
                    }
                }
                //----------------
                #endregion
                #region " 個別テクスチャを読み込む。"
                //----------------
                {
                    this._個別テクスチャリスト = new (Texture2D tex2d, ShaderResourceView srv)[ this._PMXFモデル.テクスチャリスト.Count ];

                    for( int i = 0; i < this._PMXFモデル.テクスチャリスト.Count; i++ )
                    {
                        this._個別テクスチャリスト[ i ] = (null, null);

                        var texturePath = this._PMXFモデル.テクスチャリスト[ i ];
                        var 拡張子 = Path.GetExtension( texturePath ).ToLower();

                        Debug.Write( $"Loading {texturePath} ... " );

                        try
                        {
                            var stream = リソースを開く( texturePath );    // 開く方法は呼び出し元に任せる

                            var srv = MMFShaderResourceView.FromStream(
                                d3dDevice,
                                ( 拡張子 == ".tga" ) ? TargaSolver.LoadTargaImage( stream ) : stream,
                                out var tex2d );

                            this._個別テクスチャリスト[ i ] = (tex2d, srv);

                            Debug.WriteLine( "OK" );
                        }
                        catch( Exception e )
                        {
                            Debug.WriteLine( "error!" );
                            Trace.TraceError( $"個別テクスチャファイルの読み込みに失敗しました。[{texturePath}][{e.Message}]" );
                        }
                    }
                }
                //----------------
                #endregion

                #region " ボーン用バッファを作成する。"
                //----------------
                this._ボーンのモデルポーズ配列 = new Matrix[ this._PMXFモデル.ボーンリスト.Count ];
                this._ボーンのローカル位置配列 = new Vector3[ this._PMXFモデル.ボーンリスト.Count ];
                this._ボーンの回転配列 = new Vector4[ this._PMXFモデル.ボーンリスト.Count ];
                //----------------
                #endregion
                #region " ボーン用定数バッファを作成する。"
                //----------------
                {
                    this._D3DBoneTrans定数バッファ = new SharpDX.Direct3D11.Buffer(
                        d3dDevice,
                        new BufferDescription {
                            SizeInBytes = this.PMXボーン制御リスト.Length * _D3DBoneTrans.SizeInBytes,
                            BindFlags = BindFlags.ConstantBuffer,
                        } );

                    this._D3DBoneLocalPosition定数バッファ = new SharpDX.Direct3D11.Buffer(
                        d3dDevice,
                        new BufferDescription {
                            SizeInBytes = this.PMXボーン制御リスト.Length * _D3DBoneLocalPosition.SizeInBytes,
                            BindFlags = BindFlags.ConstantBuffer,
                        } );

                    this._D3DBoneQuaternion定数バッファ = new SharpDX.Direct3D11.Buffer(
                        d3dDevice,
                        new BufferDescription {
                            SizeInBytes = this.PMXボーン制御リスト.Length * _D3DBoneQuaternion.SizeInBytes,
                            BindFlags = BindFlags.ConstantBuffer,
                        } );
                }
                //----------------
                #endregion

                #region " 既定のスキニングを作成する。"
                //----------------
                this._既定のスキニング = new 既定のスキニング( d3dDevice );
                //----------------
                #endregion

                this._初期化完了.Set();
            }
            //} );
        }

        public virtual void Dispose()
        {
            this._初期化完了.Reset();

            this._物理変形更新?.Dispose();
            this._既定のスキニング?.Dispose();

            this._D3DBoneTrans定数バッファ?.Dispose();
            this._D3DBoneLocalPosition定数バッファ?.Dispose();
            this._D3DBoneQuaternion定数バッファ?.Dispose();
            this._ボーンのモデルポーズ配列 = null;
            this._ボーンのローカル位置配列 = null;
            this._ボーンの回転配列 = null;

            foreach( var pair in this._個別テクスチャリスト )
            {
                pair.srv?.Dispose();
                pair.tex2d?.Dispose();
            }
            foreach( var pair in this._共有テクスチャリスト )
            {
                pair.srv?.Dispose();
                pair.tex2d?.Dispose();
            }

            this._裏側片面描画の際のラスタライザステート?.Dispose();
            this._片面描画の際のラスタライザステート?.Dispose();
            this._片面描画の際のラスタライザステートLine?.Dispose();
            this._両面描画の際のラスタライザステート?.Dispose();
            this._両面描画の際のラスタライザステートLine?.Dispose();
            this._D3D頂点レイアウト?.Dispose();
            this._D3Dインデックスバッファ?.Dispose();
            this._D3D頂点バッファビューUAView?.Dispose();
            this._D3D頂点バッファ?.Dispose();
            this._D3DスキニングバッファSRView?.Dispose();
            this._D3Dスキニングバッファ?.Dispose();

            this.PMX頂点制御 = null;

            this._エフェクト変数?.Dispose();

            foreach( var tech in this._D3Dテクニックリスト )
                tech?.Dispose();

            this._既定のEffect?.Dispose();

            foreach( var morph in this.PMXモーフ制御リスト )
                morph.Dispose();
            this.PMXモーフ制御リスト = null;

            foreach( var material in this.PMX材質制御リスト )
                material.Dispose();
            this.PMX材質制御リスト = null;

            this._ルートボーンリスト = null;
            this.IKボーンリスト = null;
            this._親付与によるFK変形更新 = null;
            foreach( var bone in this.PMXボーン制御リスト )
                bone.Dispose();
            this.PMXボーン制御リスト = null;

            this._PMXFモデル = null;
        }

        private ManualResetEventSlim _初期化完了 = new ManualResetEventSlim( false );



        // 進行と描画


        /// <summary>
        ///     現在時刻におけるモデルの状態を更新し、描画する。
        /// </summary>
        /// <param name="d3ddc">描画先のデバイスコンテキスト。</param>
        /// <param name="viewport">描画先ビューポートのサイズ。</param>
        public void 描画する( double 現在時刻sec, DeviceContext d3ddc, Matrix ワールド変換行列, カメラ camera, 照明 light, ViewportF viewport )
        {
            if( !this._初期化完了.IsSet )
                this._初期化完了.Wait();


            // 進行


            #region " 材質状態をリセットする。"
            //----------------
            foreach( var mat in this.PMX材質制御リスト )
                mat.状態をリセットする();
            //----------------
            #endregion

            #region " 頂点状態をリセットする。"
            //----------------
            this.PMX頂点制御.状態をリセットする( this._PMXFモデル.ヘッダ.追加UV数, this._PMXFモデル.頂点リスト );
            //----------------
            #endregion

            #region " ボーン状態をリセットする。"
            //----------------
            foreach( var bone in this.PMXボーン制御リスト )
            {
                bone.ローカル位置 = bone.PMXFボーン.位置;
                bone.移動 = Vector3.Zero;
                bone.回転 = Quaternion.Identity;
            }
            //----------------
            #endregion

            this._モデルポーズを再計算する();

            #region " モーフを適用する。"
            //----------------
            foreach( var morph in this.PMXモーフ制御リスト )
                morph.モーフを適用する( 現在時刻sec, this );

            this._モデルポーズを再計算する();
            //----------------
            #endregion

            #region " ボーンモーションを適用する。"
            //----------------
            foreach( var bone in this.PMXボーン制御リスト )
                bone.ボーンモーションを適用する( 現在時刻sec );

            this._モデルポーズを再計算する();
            //----------------
            #endregion

            #region " IKを適用する。"
            //----------------
            CCDによるIK変形更新.変形を更新する( this.IKボーンリスト );
            //----------------
            #endregion

            #region " 親付与によるFKを適用する。"
            //----------------
            this._親付与によるFK変形更新.変形を更新する();
            this._モデルポーズを再計算する();
            //----------------
            #endregion
            
            #region " 物理演算による変形を適用する。"
            //----------------
            this._物理変形更新.変形を更新する();
            //----------------
            #endregion

            #region " モデルポーズを再計算しつつ、ボーン状態を確定する。"
            //----------------
            foreach( var root in this._ルートボーンリスト )
            {
                root.モデルポーズを計算する();
                root.状態を確定する( this._ボーンのモデルポーズ配列, this._ボーンのローカル位置配列, this._ボーンの回転配列 );
            }
            //----------------
            #endregion


            // 描画

            #region " スキニングを行う。"
            //----------------
            {
                bool コンピュートシェーダーを使う = true;

                if( コンピュートシェーダーを使う )
                {
                    #region " コンピュートシェーダーでスキニングする。"
                    //----------------

                    // ボーン用定数バッファを更新する。

                    d3ddc.UpdateSubresource( this._ボーンのモデルポーズ配列, this._D3DBoneTrans定数バッファ );
                    d3ddc.UpdateSubresource( this._ボーンのローカル位置配列, this._D3DBoneLocalPosition定数バッファ );
                    d3ddc.UpdateSubresource( this._ボーンの回転配列, this._D3DBoneQuaternion定数バッファ );

                    d3ddc.ComputeShader.SetConstantBuffer( 1, this._D3DBoneTrans定数バッファ );
                    d3ddc.ComputeShader.SetConstantBuffer( 2, this._D3DBoneTrans定数バッファ );
                    d3ddc.ComputeShader.SetConstantBuffer( 3, this._D3DBoneTrans定数バッファ );

                    // 入力頂点リスト[] を D3Dスキニングバッファへ転送する。

                    if( _初めての描画 )
                    {
                        // 初回は全部転送。
                        d3ddc.UpdateSubresource( this.PMX頂点制御.入力頂点配列, this._D3Dスキニングバッファ );
                    }
                    else
                    {
                        // ２回目以降は差分のみ転送。
                        var dstRegion = new ResourceRegion( 0, 0, 0, 1, 1, 1 );

                        for( int i = 0; i < this.PMX頂点制御.単位更新フラグ.Length; i++ )
                        {
                            if( this.PMX頂点制御.単位更新フラグ[ i ] )
                            {
                                dstRegion.Left = i * PMX頂点制御.単位更新の頂点数 * CS_INPUT.SizeInBytes;
                                dstRegion.Right = Math.Min( ( i + 1 ) * PMX頂点制御.単位更新の頂点数, this.PMX頂点制御.入力頂点配列.Length ) * CS_INPUT.SizeInBytes;
                                d3ddc.UpdateSubresource( ref this.PMX頂点制御.入力頂点配列[ i * PMX頂点制御.単位更新の頂点数 ], this._D3Dスキニングバッファ, region: dstRegion );
                            }
                        }
                    }

                    // コンピュートシェーダーでスキニングを実行し、結果を頂点バッファに格納する。

                    d3ddc.ComputeShader.SetShaderResource( 0, this._D3DスキニングバッファSRView );
                    d3ddc.ComputeShader.SetUnorderedAccessView( 0, this._D3D頂点バッファビューUAView );
                    this._既定のスキニング.Run( d3ddc, this.PMX頂点制御.入力頂点配列.Length );

                    // UAVを外す（このあと頂点シェーダーが使えるように）

                    d3ddc.ComputeShader.SetUnorderedAccessView( 0, null );
                    //----------------
                    #endregion
                }
                else
                {
                    #region " CPUで行う場合 "
                    //----------------
                    var boneTrans = this._ボーンのモデルポーズ配列; // コンピュートシェーダー（HLSL）用に転置されているので注意。

                    var スキニング後の入力頂点リスト = new VS_INPUT[ this.PMX頂点制御.入力頂点配列.Length ];

                    for( int i = 0; i < this.PMX頂点制御.入力頂点配列.Length; i++ )
                    {
                        switch( this.PMX頂点制御.入力頂点配列[ i ].変形方式 )
                        {
                            case (uint) PMXFormat.ボーンウェイト種別.BDEF1:
                                #region " *** "
                                //----------------
                                {
                                    var 頂点 = this.PMX頂点制御.入力頂点配列[ i ];

                                    var bt1 = boneTrans[ 頂点.BoneIndex1 ];
                                    bt1.Transpose();

                                    Matrix bt =
                                        bt1;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.ボーンウェイト種別.BDEF2:
                                #region " *** "
                                //----------------
                                {
                                    var 頂点 = this.PMX頂点制御.入力頂点配列[ i ];

                                    var bt1 = boneTrans[ 頂点.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ 頂点.BoneIndex2 ];
                                    bt2.Transpose();

                                    Matrix bt =
                                        bt1 * 頂点.BoneWeight1 +
                                        bt2 * 頂点.BoneWeight2;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.ボーンウェイト種別.BDEF4:
                                #region " *** "
                                //----------------
                                {
                                    var 頂点 = this.PMX頂点制御.入力頂点配列[ i ];

                                    var bt1 = boneTrans[ 頂点.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ 頂点.BoneIndex2 ];
                                    bt2.Transpose();
                                    var bt3 = boneTrans[ 頂点.BoneIndex3 ];
                                    bt3.Transpose();
                                    var bt4 = boneTrans[ 頂点.BoneIndex4 ];
                                    bt4.Transpose();

                                    Matrix bt =
                                        bt1 * 頂点.BoneWeight1 +
                                        bt2 * 頂点.BoneWeight2 +
                                        bt3 * 頂点.BoneWeight3 +
                                        bt4 * 頂点.BoneWeight4;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                                    スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.ボーンウェイト種別.SDEF:
                                #region " *** "
                                //----------------
                                {
                                    // 参考: 
                                    // 自分用メモ「PMXのスフィリカルデフォームのコードっぽいもの」（sma42氏）
                                    // https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60755964

                                    var 頂点 = this.PMX頂点制御.入力頂点配列[ i ];

                                    var bt1 = boneTrans[ 頂点.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ 頂点.BoneIndex2 ];
                                    bt2.Transpose();

                                    #region " 影響度0,1 の算出 "
                                    //----------------
                                    float 影響度0 = 0f;  // 固定値であるSDEFパラメータにのみ依存するので、これらの値も固定値。
                                    float 影響度1 = 0f;  //
                                    {
                                        float L0 = ( 頂点.SdefR0 - (Vector3) this._ボーンのローカル位置配列[ 頂点.BoneIndex2 ] ).Length();   // 子ボーンからR0までの距離
                                        float L1 = ( 頂点.SdefR1 - (Vector3) this._ボーンのローカル位置配列[ 頂点.BoneIndex2 ] ).Length();   // 子ボーンからR1までの距離

                                        影響度0 = ( Math.Abs( L0 - L1 ) < 0.0001f ) ? 0.5f : MathUtil.Clamp( L0 / ( L0 + L1 ), 0.0f, 1.0f );
                                        影響度1 = 1.0f - 影響度0;
                                    }
                                    //----------------
                                    #endregion

                                    Matrix モデルポーズ行列L = bt1 * 頂点.BoneWeight1;
                                    Matrix モデルポーズ行列R = bt2 * 頂点.BoneWeight2;
                                    Matrix モデルポーズ行列C = モデルポーズ行列L + モデルポーズ行列R;

                                    Vector4 点C = Vector4.Transform( 頂点.Sdef_C, モデルポーズ行列C );    // BDEF2で計算された点Cの位置
                                    Vector4 点P = Vector4.Transform( 頂点.Position, モデルポーズ行列C );  // BDEF2で計算された頂点の位置

                                    Matrix 重み付き回転行列 = Matrix.RotationQuaternion(
                                        Quaternion.Slerp(   // 球体線形補間
                                            new Quaternion( this._ボーンの回転配列[ 頂点.BoneIndex1 ].ToArray() ) * 頂点.BoneWeight1,
                                            new Quaternion( this._ボーンの回転配列[ 頂点.BoneIndex2 ].ToArray() ) * 頂点.BoneWeight2,
                                            頂点.BoneWeight1 ) );

                                    Vector4 点R0 = Vector4.Transform( new Vector4( 頂点.SdefR0, 1f ), ( モデルポーズ行列L + ( モデルポーズ行列C * -頂点.BoneWeight1 ) ) );
                                    Vector4 点R1 = Vector4.Transform( new Vector4( 頂点.SdefR1, 1f ), ( モデルポーズ行列R + ( モデルポーズ行列C * -頂点.BoneWeight2 ) ) );
                                    点C += ( 点R0 * 影響度0 ) + ( 点R1 * 影響度1 );   // 膨らみすぎ防止

                                    点P -= 点C;     // 頂点を点Cが中心になるよう移動して
                                    点P = Vector4.Transform( 点P, 重み付き回転行列 );   // 回転して
                                    点P += 点C;     // 元の位置へ

                                    スキニング後の入力頂点リスト[ i ].Position = 点P;
                                    スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, 重み付き回転行列 );
                                    スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.ボーンウェイト種別.QDEF:
                                #region " *** "
                                //----------------
                                {
                                    // ※ QDEFを使ったモデルが見つからないのでテストしてません。あれば教えてください！

                                    var 頂点 = this.PMX頂点制御.入力頂点配列[ i ];

                                    var dualQuaternion = new DualQuaternion[ 4 ];   // 最大４ボーンまで対応

                                    var boneIndexes = new[] { 頂点.BoneIndex1, 頂点.BoneIndex2, 頂点.BoneIndex3, 頂点.BoneIndex4 };
                                    var boneWeights = new[] { 頂点.BoneWeight1, 頂点.BoneWeight2, 頂点.BoneWeight3, 頂点.BoneWeight4 };

                                    var bt = new[] { boneTrans[ boneIndexes[ 0 ] ], boneTrans[ boneIndexes[ 1 ] ], boneTrans[ boneIndexes[ 2 ] ], boneTrans[ boneIndexes[ 3 ] ] };
                                    for( int b = 0; b < 4; b++ )
                                        bt[ b ].Transpose();

                                    for( int b = 0; b < 4; b++ )
                                    {
                                        if( boneWeights[ b ] == 0f )
                                        {
                                            dualQuaternion[ b ] = DualQuaternion.Zero;  // 未使用
                                        }
                                        else
                                        {
                                            dualQuaternion[ b ] = new DualQuaternion( bt[ boneIndexes[ b ] ] );
                                        }
                                    }

                                    Matrix btm = (
                                        dualQuaternion[ 0 ] * boneWeights[ 0 ] +
                                        dualQuaternion[ 1 ] * boneWeights[ 1 ] +
                                        dualQuaternion[ 2 ] * boneWeights[ 2 ] +
                                        dualQuaternion[ 3 ] * boneWeights[ 3 ] ).ToMatrix();

                                    if( Matrix.Zero == btm )
                                        btm = Matrix.Identity;

                                    スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, btm );
                                    スキニング後の入力頂点リスト[ i ].Normal = 頂点.Normal;
                                }
                                //----------------
                                #endregion
                                break;
                        }

                        スキニング後の入力頂点リスト[ i ].UV = this.PMX頂点制御.入力頂点配列[ i ].UV;
                        スキニング後の入力頂点リスト[ i ].AddUV1 = this.PMX頂点制御.入力頂点配列[ i ].AddUV1;
                        スキニング後の入力頂点リスト[ i ].AddUV2 = this.PMX頂点制御.入力頂点配列[ i ].AddUV2;
                        スキニング後の入力頂点リスト[ i ].AddUV3 = this.PMX頂点制御.入力頂点配列[ i ].AddUV3;
                        スキニング後の入力頂点リスト[ i ].AddUV4 = this.PMX頂点制御.入力頂点配列[ i ].AddUV4;
                        スキニング後の入力頂点リスト[ i ].EdgeWeight = this.PMX頂点制御.入力頂点配列[ i ].EdgeWeight;
                        スキニング後の入力頂点リスト[ i ].Index = this.PMX頂点制御.入力頂点配列[ i ].Index;
                    }

                    d3ddc.UpdateSubresource( スキニング後の入力頂点リスト, this._D3D頂点バッファ );
                    //----------------
                    #endregion
                }
            }
            //----------------
            #endregion

            #region " モデル単位のD3Dパイプラインを構築する。"
            //----------------
            {
                d3ddc.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( this._D3D頂点バッファ, VS_INPUT.SizeInBytes, 0 ) );
                d3ddc.InputAssembler.SetIndexBuffer( this._D3Dインデックスバッファ, Format.R32_UInt, 0 );
                d3ddc.InputAssembler.InputLayout = this._D3D頂点レイアウト;
                d3ddc.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;

                d3ddc.Rasterizer.SetViewport( viewport );
            }
            //----------------
            #endregion

            #region " エフェクト変数（モデル単位）を設定する。"
            //----------------
            this._エフェクト変数.WORLDVIEWPROJECTION.SetMatrix( ワールド変換行列 * camera.ビュー行列を取得する() * camera.射影行列を取得する() );
            this._エフェクト変数.WORLDVIEW.SetMatrix( ワールド変換行列 * camera.ビュー行列を取得する() );
            this._エフェクト変数.WORLD.SetMatrix( ワールド変換行列 );
            this._エフェクト変数.VIEW.SetMatrix( camera.ビュー行列を取得する() );
            this._エフェクト変数.VIEWPROJECTION.SetMatrix( camera.ビュー行列を取得する() * camera.射影行列を取得する() );
            this._エフェクト変数.VIEWPORTPIXELSIZE.Set( viewport );
            this._エフェクト変数.POSITION_camera.Set( new Vector4( camera.位置, 0f ) );
            this._エフェクト変数.POSITION_light.Set( new Vector4( -light.照射方向, 0f ) );
            //----------------
            #endregion

            #region " すべての材質を描画する。"
            //----------------
            for( int i = 0; i < this.PMX材質制御リスト.Length; i++ )
            {
                var 材質 = this.PMX材質制御リスト[ i ];


                #region " エフェクト変数（材質単位）を設定する。"
                //----------------
                this._エフェクト変数.EDGECOLOR.Set( 材質.エッジ色 );
                this._エフェクト変数.EDGEWIDTH.Set( 材質.エッジサイズ );

                if( -1 != 材質.通常テクスチャの参照インデックス )
                    this._エフェクト変数.MATERIALTEXTURE.SetResource( this._個別テクスチャリスト[ 材質.通常テクスチャの参照インデックス ].srv );

                if( -1 != 材質.スフィアテクスチャの参照インデックス )
                    this._エフェクト変数.MATERIALSPHEREMAP.SetResource( this._個別テクスチャリスト[ 材質.スフィアテクスチャの参照インデックス ].srv );

                if( 1 == 材質.共有Toonフラグ )
                    this._エフェクト変数.MATERIALTOONTEXTURE.SetResource( this._共有テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                else if( -1 != 材質.共有Toonのテクスチャ参照インデックス )
                    this._エフェクト変数.MATERIALTOONTEXTURE.SetResource( this._個別テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                else
                    this._エフェクト変数.MATERIALTOONTEXTURE.SetResource( this._共有テクスチャリスト[ 0 ].srv );

                this._エフェクト変数.TESSFACTOR.Set( 材質.テッセレーション係数 );
                this._エフェクト変数.use_spheremap.Set( 材質.スフィアテクスチャの参照インデックス != -1 );
                this._エフェクト変数.spadd.Set( 材質.スフィアモード == PMXFormat.スフィアモード.加算 );
                this._エフェクト変数.use_texture.Set( 材質.通常テクスチャの参照インデックス != -1 );
                this._エフェクト変数.use_toontexturemap.Set( 材質.共有Toonのテクスチャ参照インデックス != -1 );
                this._エフェクト変数.use_selfshadow.Set( 材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.セルフ影 ) );
                this._エフェクト変数.ambientcolor.Set( new Vector4( 材質.環境色, 1f ) );
                this._エフェクト変数.diffusecolor.Set( 材質.拡散色 );
                this._エフェクト変数.specularcolor.Set( new Vector4( 材質.反射色, 1f ) );
                this._エフェクト変数.specularpower.Set( 材質.反射強度 );
                //----------------
                #endregion


                // オブジェクト描画

                var Pass種別 = MMDPass種別.オブジェクト本体;

                #region " Rasterizer.State "
                //----------------
                if( Pass種別 == MMDPass種別.エッジ )
                {
                    d3ddc.Rasterizer.State = this._裏側片面描画の際のラスタライザステート;
                }
                else if( !材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.両面描画 ) )
                {
                    if( 材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.Line描画 ) )
                        d3ddc.Rasterizer.State = this._片面描画の際のラスタライザステートLine;
                    else
                        d3ddc.Rasterizer.State = this._片面描画の際のラスタライザステート;
                }
                else
                {
                    if( 材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.Line描画 ) )
                        d3ddc.Rasterizer.State = this._両面描画の際のラスタライザステートLine;
                    else
                        d3ddc.Rasterizer.State = this._両面描画の際のラスタライザステート;
                }
                //----------------
                #endregion

                this._エフェクトを適用しつつ材質を描画する( d3ddc, 材質, Pass種別, ( mat ) => {
                    d3ddc.DrawIndexed( mat.頂点数, mat.開始インデックス, 0 );
                } );


                // エッジ描画

                Pass種別 = MMDPass種別.エッジ;

                d3ddc.Rasterizer.State = this._裏側片面描画の際のラスタライザステート;

                this._エフェクトを適用しつつ材質を描画する( d3ddc, 材質, Pass種別, ( mat ) => {
                    d3ddc.DrawIndexed( mat.頂点数, mat.開始インデックス, 0 );
                } );
            }
            //----------------
            #endregion

            this._初めての描画 = false;
        }

        private void _モデルポーズを再計算する()
        {
            foreach( var root in this._ルートボーンリスト )
                root.モデルポーズを計算する();
        }

        private void _エフェクトを適用しつつ材質を描画する( DeviceContext d3ddc, PMX材質制御 ipmxSubset, MMDPass種別 passType, Action<PMX材質制御> drawAction )
        {
            if( ipmxSubset.拡散色.W == 0 )
                return;

            // 使用するtechniqueを検索する
            テクニック technique = this._D3Dテクニックリスト.Where( ( t ) => t.テクニックを適用する描画対象 == passType ).FirstOrDefault();

            if( null != technique ) // 最初の１つだけ有効（複数はないはずだが）
                technique.パスの適用と描画をパスの数だけ繰り返す( d3ddc, drawAction, ipmxSubset );
        }

        private bool _初めての描画 = true;



        // アニメ指示


        // ボーン名が null ならすべてのボーンが対象。
        public void ボーンアニメーションをクリアする( string ボーン名 = null )
        {
            foreach( var pmxBone in this.PMXボーン制御リスト )
            {
                if( null == ボーン名 || pmxBone.PMXFボーン.ボーン名 == ボーン名 )
                {
                    pmxBone.アニメ変数_移動.遷移をクリアする();
                    pmxBone.アニメ変数_回転.遷移をクリアする();
                }
            }
        }

        // モーフ名が null ならすべてのモーフが対象。
        public void モーフアニメーションをクリアする( string モーフ名 = null )
        {
            foreach( var pmxMorph in this.PMXモーフ制御リスト )
            {
                if( null == モーフ名 || pmxMorph.PMXFモーフ.モーフ名 == モーフ名 )
                {
                    pmxMorph.アニメ変数_モーフ.遷移をクリアする();
                }
            }
        }

        public void ボーンアニメーションを追加する( string ボーン名, アニメ遷移<Vector3> 移動遷移, アニメ遷移<Quaternion> 回転遷移 )
        {
            var pmxBone = this.PMXボーン制御リスト.Where( ( bone ) => bone.PMXFボーン.ボーン名 == ボーン名 ).FirstOrDefault();

            if( null == pmxBone )
                throw new Exception( $"指定された名前「{ボーン名}」のボーンが存在しません。" );

            pmxBone.アニメ変数_移動.遷移を追加する( 移動遷移 );
            pmxBone.アニメ変数_回転.遷移を追加する( 回転遷移 );
        }

        public void モーフアニメーションを追加する( string モーフ名, アニメ遷移<float> モーフ遷移 )
        {
            var pmxMorph = this.PMXモーフ制御リスト.Where( ( morph ) => morph.PMXFモーフ.モーフ名 == モーフ名 ).FirstOrDefault();

            if( null == pmxMorph )
                throw new Exception( $"指定された名前「{モーフ名}」のモーフが存在しません。" );

            pmxMorph.アニメ変数_モーフ.遷移を追加する( モーフ遷移 );
        }



        // private


        private PMXFormat.モデル _PMXFモデル;

        private (Texture2D tex2d, ShaderResourceView srv)[] _共有テクスチャリスト;

        private (Texture2D tex2d, ShaderResourceView srv)[] _個別テクスチャリスト;

        private Effect _既定のEffect;

        private List<テクニック> _D3Dテクニックリスト;

        private エフェクト変数 _エフェクト変数;

        private Matrix[] _ボーンのモデルポーズ配列;

        private Vector3[] _ボーンのローカル位置配列;

        private Vector4[] _ボーンの回転配列;

        private struct _D3DBoneTrans    // サイズ計測用構造体
        {
            public Matrix boneTrans;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( Marshal.SizeOf( typeof( _D3DBoneTrans ) ) ) / 16 + 1 ) * 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneTrans定数バッファ;

        private struct _D3DBoneLocalPosition    // サイズ計測用構造体
        {
            public Vector3 boneLocalPosition;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( Marshal.SizeOf( typeof( _D3DBoneLocalPosition ) ) ) / 16 + 1 ) * 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneLocalPosition定数バッファ;

        private struct _D3DBoneQuaternion    // サイズ計測用構造体
        {
            public Vector4 boneQuaternion;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( Marshal.SizeOf( typeof( _D3DBoneQuaternion ) ) ) / 16 + 1 ) * 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneQuaternion定数バッファ;

        private SharpDX.Direct3D11.Buffer _D3Dスキニングバッファ;

        private ShaderResourceView _D3DスキニングバッファSRView;

        private SharpDX.Direct3D11.Buffer _D3D頂点バッファ;

        private InputLayout _D3D頂点レイアウト;

        private UnorderedAccessView _D3D頂点バッファビューUAView;

        private SharpDX.Direct3D11.Buffer _D3Dインデックスバッファ;

        private RasterizerState _裏側片面描画の際のラスタライザステート;

        private RasterizerState _片面描画の際のラスタライザステート;

        private RasterizerState _片面描画の際のラスタライザステートLine;

        private RasterizerState _両面描画の際のラスタライザステート;

        private RasterizerState _両面描画の際のラスタライザステートLine;

        private List<PMXボーン制御> _ルートボーンリスト;

        private 親付与によるFK変形更新 _親付与によるFK変形更新;

        private PMX物理変形更新 _物理変形更新;

        private ISkinning _既定のスキニング;


        /// <summary>
        ///     指定されたリソースパスを、埋め込みリソースまたはファイルとして開き、
        ///     Stream として返す。
        /// </summary>
        /// <param name="リソースパス">ファイルパスまたはリソースパス。</param>
        /// <returns></returns>
        private void _頂点データを頂点レイアウトリストに追加する( PMXFormat.頂点 頂点データ, List<CS_INPUT> 頂点レイアウトリスト )
        {
            var layout = new CS_INPUT() {
                Position = new Vector4( 頂点データ.位置, 1f ),
                Normal = 頂点データ.法線,
                UV = 頂点データ.UV,
                Index = (uint) 頂点レイアウトリスト.Count,    // 現在の要素数 ＝ List<>内でのこの要素のインデックス番号
                EdgeWeight = 頂点データ.エッジ倍率,
                変形方式 = (uint) 頂点データ.ウェイト変形方式,
            };

            switch( 頂点データ.ウェイト変形方式 )
            {
                case PMXFormat.ボーンウェイト種別.BDEF1:
                    {
                        var v = (PMXFormat.BDEF1) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.boneReferenceIndex < 0 ) ? 0 : v.boneReferenceIndex );
                        layout.BoneIndex2 = 0;
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.boneReferenceIndex < 0 ) ? 0.0f : 1.0f;
                        layout.BoneWeight2 = 0.0f;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                    }
                    break;

                case PMXFormat.ボーンウェイト種別.BDEF2:
                    {
                        var v = (PMXFormat.BDEF2) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Bone1Weight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Bone2Weight;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                    }
                    break;

                case PMXFormat.ボーンウェイト種別.SDEF:
                    {
                        var v = (PMXFormat.SDEF) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Bone1Weight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Bone2Weight;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                        layout.Sdef_C = new Vector4( v.SDEF_C, 1f );
                        layout.SdefR0 = v.SDEF_R0;
                        layout.SdefR1 = v.SDEF_R1;
                    }
                    break;

                case PMXFormat.ボーンウェイト種別.BDEF4:
                case PMXFormat.ボーンウェイト種別.QDEF:
                    {
                        var v = (PMXFormat.BDEF4) 頂点データ.ボーンウェイト;
                        float sumWeight = v.Weights.X + v.Weights.Y + v.Weights.Z + v.Weights.W;
                        if( !( 0.99999f < sumWeight && sumWeight < 1.00001f ) && v.Weights.W == 0f )
                        {
                            // sumWeight ≒ 1.0 かつ W ＝ 0 なら、W にあまり全部を足す。
                            v.Weights.W = 1.0f - v.Weights.X - v.Weights.Y - v.Weights.Z;
                            sumWeight = 1.0f;
                        }
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = (uint) ( ( v.Bone3ReferenceIndex < 0 ) ? 0 : v.Bone3ReferenceIndex );
                        layout.BoneIndex4 = (uint) ( ( v.Bone4ReferenceIndex < 0 ) ? 0 : v.Bone4ReferenceIndex );
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Weights.X / sumWeight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Weights.Y / sumWeight;
                        layout.BoneWeight3 = ( v.Bone3ReferenceIndex < 0 ) ? 0.0f : v.Weights.Z / sumWeight;
                        layout.BoneWeight4 = ( v.Bone4ReferenceIndex < 0 ) ? 0.0f : v.Weights.W / sumWeight;
                    }
                    break;
            }

            頂点レイアウトリスト.Add( ( layout ) );
        }
    }
}
