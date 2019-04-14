using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using MikuMikuFlex3.Utility;

#pragma warning disable 0649

namespace MikuMikuFlex3
{
    public class PMXモデル : IDisposable
    {

        // 構造


        public Matrix ワールド変換行列 { get; set; } = Matrix.Identity;

        public const int 最大ボーン数 = 768;

        internal PMX頂点制御 PMX頂点制御 { get; private protected set; }

        public PMXボーン制御[] ボーンリスト { get; protected set; }

        public PMX材質制御[] 材質リスト { get; protected set; }

        public PMXモーフ制御[] モーフリスト { get; protected set; }

        internal List<PMXボーン制御> IKボーンリスト { get; private protected set; }

        public List<PMXボーン制御> ルートボーンリスト { get; protected set; }



        // シェーダー


        public ISkinning スキニングシェーダー { get; set; }

        private bool _スキニングを解放する;


        /// <summary>
        ///     既定の材質描画シェーダー。
        ///     材質ごとの <see cref="PMX材質制御.材質描画シェーダー"/> が null のときに使用される。
        /// </summary>
        public IMaterialShader 既定の材質描画シェーダー { get; set; }

        private bool _材質描画を解放する;



        // 生成と終了


        /// <summary>
        ///     ファイルからPMXモデルを読み込む。
        /// </summary>
        /// <remarks>
        ///     PMXファイルで使用されるテクスチャ等のリソースファイルは、
        ///     PMXファイルと同じフォルダを基準として検索される。
        /// </remarks>
        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, string PMXファイルパス, ISkinning skinning = null, IMaterialShader 既定の材質シェーダー = null )
        {
            var stream = new FileStream( PMXファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read );

            this._読み込んで初期化する( d3dDevice, stream, skinning, 既定の材質シェーダー, リソースを開く: ( file ) => {

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
        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, Type 名前空間を示す型, string リソース名, ISkinning skinning = null, IMaterialShader 既定の材質シェーダー = null )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = $"{this.GetType().Namespace}.{リソース名}";

            var stream = assembly.GetManifestResourceStream( path );

            this._読み込んで初期化する( d3dDevice, stream, skinning, 既定の材質シェーダー, リソースを開く: ( resource ) => {

                // PMXではテクスチャ名などにパス区切り文字を使用できるが、その区切りがなんであるかはOSに依存して
                // PMXでは感知しないとのことなので、とりあえず '/' と '\' を想定する。
                var rpath = resource.Replace( Path.DirectorySeparatorChar, '.' ).Replace( Path.AltDirectorySeparatorChar, '.' );    // '.' 区切りに変換

                return assembly.GetManifestResourceStream( 名前空間を示す型, rpath );

            } );

            // stream は、上記初期化作業の中で閉じられる。
        }

        private void _読み込んで初期化する( SharpDX.Direct3D11.Device d3dDevice, Stream PMXデータ, ISkinning skinning, IMaterialShader 既定の材質シェーダー, Func<string, Stream> リソースを開く )
        {
            #region " モデルを読み込む。"
            //----------------
            this._PMXFモデル = new PMXFormat.モデル( PMXデータ );

            if( this._PMXFモデル.ボーンリスト.Count > 最大ボーン数 )
                throw new Exception( "ボーン数が多すぎます。" );

            PMXデータ.Dispose();
            //----------------
            #endregion
            #region " ボーンリストを作成する。"
            //----------------
            {
                int ボーン数 = this._PMXFモデル.ボーンリスト.Count;
                this.ボーンリスト = new PMXボーン制御[ ボーン数 ];

                for( int i = 0; i < ボーン数; i++ )
                    this.ボーンリスト[ i ] = new PMXボーン制御( this._PMXFモデル.ボーンリスト[ i ], i );

                for( int i = 0; i < ボーン数; i++ )
                    this.ボーンリスト[ i ].読み込み後の処理を行う( this.ボーンリスト );
            }
            //----------------
            #endregion
            #region " IKボーンリストを作成する。"
            //----------------
            {
                var ikBones = this.ボーンリスト.Where( ( bone ) => bone.PMXFボーン.IKボーンである );

                this.IKボーンリスト = new List<PMXボーン制御>( ikBones.Count() );

                for( int i = 0; i < ikBones.Count(); i++ )
                    this.IKボーンリスト.Add( ikBones.ElementAt( i ) );
            }
            //----------------
            #endregion
            #region " ボーンのルートリストを作成する。"
            //----------------
            {
                this.ルートボーンリスト = new List<PMXボーン制御>();

                for( int i = 0; i < this.ボーンリスト.Length; i++ )
                {
                    // 親ボーンを持たないのがルートボーン。
                    if( this.ボーンリスト[ i ].PMXFボーン.親ボーンのインデックス == -1 )
                    {
                        this.ルートボーンリスト.Add( this.ボーンリスト[ i ] );
                    }
                }
            }
            //----------------
            #endregion
            #region " PMXボーンの変形階層を設定する。"
            //----------------
            {
                foreach( var root in this.ルートボーンリスト )
                {
                    設定( root, 0 );
                }

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
                    int BoneCount = this.ボーンリスト.Length;

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
                this.ルートボーンリスト.Sort( comparison );
            }
            //----------------
            #endregion
            #region " 親付与によるFKを初期化する。"
            //----------------
            {
                this._親付与によるFK変形更新 = new 親付与によるFK変形更新( this.ボーンリスト );
            }
            //----------------
            #endregion
            #region " 物理変形を初期化する。"
            //----------------
            {
                this._物理変形更新 = new PMX物理変形更新( this.ボーンリスト, this._PMXFモデル.剛体リスト, this._PMXFモデル.ジョイントリスト );
            }
            //----------------
            #endregion
            #region " 材質リストを作成する。"
            //----------------
            {
                int 材質数 = this._PMXFモデル.材質リスト.Count;

                this.材質リスト = new PMX材質制御[ 材質数 ];

                for( int i = 0; i < 材質数; i++ )
                    this.材質リスト[ i ] = new PMX材質制御( this._PMXFモデル.材質リスト[ i ], 既定の材質シェーダー );
            }
            //----------------
            #endregion
            #region " モーフリストを作成する。"
            //----------------
            {
                int モーフ数 = this._PMXFモデル.モーフリスト.Count;

                this.モーフリスト = new PMXモーフ制御[ モーフ数 ];

                for( int i = 0; i < モーフ数; i++ )
                    this.モーフリスト[ i ] = new PMXモーフ制御( this._PMXFモデル.モーフリスト[ i ] );
            }
            //----------------
            #endregion

            #region " PMX頂点制御を生成する。"
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
            #region " 頂点レイアウトを作成する。"
            //----------------
            {
                var assembly = Assembly.GetExecutingAssembly();
                using( var fs = assembly.GetManifestResourceStream( this.GetType(), "Resources.Shaders.DefaultVertexShaderForObject.cso" ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );

                    this._D3D頂点レイアウト = new InputLayout( d3dDevice, buffer, VS_INPUT.VertexElements );
                }
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
                            SizeInBytes = (int)dataStream.Length
                        } );
                }
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
            #region " サンプラーステートを作成する。"
            //----------------
            this._PS用SamplerState = new SamplerState( d3dDevice, new SamplerStateDescription {
                Filter = Filter.MinMagLinearMipPoint,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                ComparisonFunction = Comparison.Never,
                MipLodBias = 0f,
                BorderColor = Color4.White,
                MaximumAnisotropy = 1,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue,
            } );
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
                        using( var stream = リソースを開く( texturePath ) )    // 開く方法は呼び出し元に任せる
                        {
                            var srv = MMFShaderResourceView.FromStream(
                                d3dDevice,
                                ( 拡張子 == ".tga" ) ? TargaSolver.LoadTargaImage( stream ) : stream,
                                out var tex2d );

                            this._個別テクスチャリスト[ i ] = (tex2d, srv);

                            Debug.WriteLine( "OK" );
                        }
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

            #region " ボーン用の配列を作成する。"
            //----------------
            this._ボーンのモデルポーズ配列 = new Matrix[ this._PMXFモデル.ボーンリスト.Count ];
            this._ボーンのローカル位置配列 = new Vector3[ this._PMXFモデル.ボーンリスト.Count ];
            this._ボーンの回転配列 = new Vector4[ this._PMXFモデル.ボーンリスト.Count ];
            //----------------
            #endregion
            #region " ボーン用の定数バッファを作成する。"
            //----------------
            {
                this._D3DBoneTrans定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.ボーンリスト.Length * _D3DBoneTrans.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );

                this._D3DBoneLocalPosition定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.ボーンリスト.Length * _D3DBoneLocalPosition.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );

                this._D3DBoneQuaternion定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.ボーンリスト.Length * _D3DBoneQuaternion.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );
            }
            //----------------
            #endregion

            #region " グローバルパラメータ定数バッファを作成する。"
            //----------------
            this._GlobalParameters定数バッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new BufferDescription {
                    SizeInBytes = GlobalParameters.SizeInBytes,
                    BindFlags = BindFlags.ConstantBuffer,
                } );
            //----------------
            #endregion
            #region " ISkinning を作成する。"
            //----------------
            if( null != skinning )
            {
                this.スキニングシェーダー = skinning;
                this._スキニングを解放する = false;
            }
            else
            {
                this.スキニングシェーダー = new DefaultSkinning( d3dDevice );
                this._スキニングを解放する = true;
            }
            //----------------
            #endregion
            #region " IRenderMaterial を作成する。"
            //----------------
            if( null != 既定の材質シェーダー )
            {
                this.既定の材質描画シェーダー = 既定の材質シェーダー;
                this._材質描画を解放する = false;
            }
            else
            {
                this.既定の材質描画シェーダー = new DefaultMaterialShader( d3dDevice );
                this._材質描画を解放する = true;
            }

            //----------------
            #endregion

            this._初期化完了.Set();
        }

        public virtual void Dispose()
        {
            this._初期化完了.Reset();

            if( this._材質描画を解放する )
                this.既定の材質描画シェーダー?.Dispose();

            if( this._スキニングを解放する )
                this.スキニングシェーダー?.Dispose();

            this._GlobalParameters定数バッファ?.Dispose();

            this._物理変形更新?.Dispose();
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

            this._PS用SamplerState?.Dispose();
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

            foreach( var morph in this.モーフリスト )
                morph.Dispose();
            this.モーフリスト = null;

            foreach( var material in this.材質リスト )
                material.Dispose();
            this.材質リスト = null;

            this.ルートボーンリスト = null;
            this.IKボーンリスト = null;
            this._親付与によるFK変形更新 = null;
            foreach( var bone in this.ボーンリスト )
                bone.Dispose();
            this.ボーンリスト = null;

            this._PMXFモデル = null;
        }

        private ManualResetEventSlim _初期化完了 = new ManualResetEventSlim( false );



        // 進行と描画


        /// <summary>
        ///     現在時刻におけるモデルの状態を更新し、描画する。
        /// </summary>
        /// <param name="現在時刻sec">現在時刻[秒]。</param>
        /// <param name="d3ddc">描画先のデバイスコンテキスト。</param>
        /// <param name="ワールド変換行列">モデルに適用するワールド変換行列。</param>
        /// <param name="camera">モデルに適用するカメラ。</param>
        /// <param name="light">モデルに適用する照明。</param>
        /// <param name="viewport">描画先ビューポートのサイズ。</param>
        public void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            if( !this._初期化完了.IsSet )
                this._初期化完了.Wait();


            // 進行


            #region " 材質の状態をリセットする。"
            //----------------
            foreach( var mat in this.材質リスト )
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
            foreach( var bone in this.ボーンリスト )
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
            foreach( var morph in this.モーフリスト )
                morph.モーフを適用する( 現在時刻sec, this );

            this._モデルポーズを再計算する();
            //----------------
            #endregion

            #region " ボーンモーションを適用する。"
            //----------------
            foreach( var bone in this.ボーンリスト )
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
            foreach( var root in this.ルートボーンリスト )
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
                    this.スキニングシェーダー.Run( d3ddc, this.PMX頂点制御.入力頂点配列.Length );

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

            #region " D3Dパイプライン（モデル単位）を設定する。"
            //----------------
            {
                d3ddc.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( this._D3D頂点バッファ, VS_INPUT.SizeInBytes, 0 ) );
                d3ddc.InputAssembler.SetIndexBuffer( this._D3Dインデックスバッファ, Format.R32_UInt, 0 );
                d3ddc.InputAssembler.InputLayout = this._D3D頂点レイアウト;
                d3ddc.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;

                d3ddc.Rasterizer.SetViewport( new ViewportF {
                    X = 0,
                    Y = 0,
                    Width = globalParameters.ViewportSize.X,
                    Height = globalParameters.ViewportSize.Y,
                    MinDepth = 0f,
                    MaxDepth = 1f,
                } );

                d3ddc.PixelShader.SetSampler( 0, this._PS用SamplerState );
            }
            //----------------
            #endregion

            #region " グローバルパラメータ（モデル単位）を設定する。"
            //----------------
            globalParameters.WorldMatrix = ワールド変換行列;
            globalParameters.WorldMatrix.Transpose();
            //----------------
            #endregion

            #region " すべての材質を描画する。"
            //----------------
            for( int i = 0; i < this.材質リスト.Length; i++ )
            {
                var 材質 = this.材質リスト[ i ];


                #region " グローバルパラメータ（材質単位）を設定する。"
                //----------------
                globalParameters.EdgeColor = 材質.エッジ色;
                globalParameters.EdgeWidth = 材質.エッジサイズ;
                globalParameters.TessellationFactor = 材質.テッセレーション係数;
                globalParameters.UseSelfShadow = ( 材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.セルフ影 ) );
                globalParameters.AmbientColor = new Vector4( 材質.環境色, 1f );
                globalParameters.DiffuseColor = 材質.拡散色;
                globalParameters.SpecularColor = new Vector4( 材質.反射色, 1f );
                globalParameters.SpecularPower = 材質.反射強度;

                if( -1 != 材質.通常テクスチャの参照インデックス )
                {
                    globalParameters.UseTexture = true;
                    d3ddc.PixelShader.SetShaderResource( 0, this._個別テクスチャリスト[ 材質.通常テクスチャの参照インデックス ].srv );
                }
                else
                {
                    globalParameters.UseTexture = false;
                }

                if( -1 != 材質.スフィアテクスチャの参照インデックス )
                {
                    globalParameters.UseSphereMap = true;
                    globalParameters.IsAddSphere = ( 材質.スフィアモード == PMXFormat.スフィアモード.加算 );
                    d3ddc.PixelShader.SetShaderResource( 1, this._個別テクスチャリスト[ 材質.スフィアテクスチャの参照インデックス ].srv );
                }
                else
                {
                    globalParameters.UseSphereMap = false;
                }

                if( 1 == 材質.共有Toonフラグ )
                {
                    globalParameters.UseToonTextureMap = true;
                    d3ddc.PixelShader.SetShaderResource( 2, this._共有テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                }
                else if( -1 != 材質.共有Toonのテクスチャ参照インデックス )
                {
                    globalParameters.UseToonTextureMap = true;
                    d3ddc.PixelShader.SetShaderResource( 2, this._個別テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                }
                else
                {
                    globalParameters.UseToonTextureMap = false;
                    //d3ddc.PixelShader.SetShaderResource( 2, this._共有テクスチャリスト[ 0 ].srv );
                }
                //----------------
                #endregion

                #region " グローバルパラメータを各シェーダステージの定数バッファ b0 へ転送する。"
                //----------------
                d3ddc.UpdateSubresource( ref globalParameters, this._GlobalParameters定数バッファ );

                d3ddc.VertexShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
                d3ddc.HullShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
                d3ddc.DomainShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
                d3ddc.GeometryShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
                d3ddc.PixelShader.SetConstantBuffer( 0, this._GlobalParameters定数バッファ );
                //----------------
                #endregion

                var 材質描画シェーダー = 材質.材質描画シェーダー ?? this.既定の材質描画シェーダー;


                // オブジェクト描画

                #region " D3Dパイプライン（材質単位）を設定する。"
                //----------------
                if( !材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.両面描画 ) )
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

                材質描画シェーダー.Draw( 材質.頂点数, 材質.開始インデックス, MMDPass.Object, d3ddc );


                // エッジ描画


                #region " D3Dパイプライン（材質単位）を設定する。"
                //----------------
                d3ddc.Rasterizer.State = this._裏側片面描画の際のラスタライザステート;
                //----------------
                #endregion

                材質描画シェーダー.Draw( 材質.頂点数, 材質.開始インデックス, MMDPass.Edge, d3ddc );
            }
            //----------------
            #endregion

            this._初めての描画 = false;
        }

        private void _モデルポーズを再計算する()
        {
            foreach( var root in this.ルートボーンリスト )
                root.モデルポーズを計算する();
        }

        private bool _初めての描画 = true;



        // private


        private PMXFormat.モデル _PMXFモデル;

        private (Texture2D tex2d, ShaderResourceView srv)[] _共有テクスチャリスト;

        private (Texture2D tex2d, ShaderResourceView srv)[] _個別テクスチャリスト;

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

            public float dummy1;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes = 16;
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

        private 親付与によるFK変形更新 _親付与によるFK変形更新;

        private PMX物理変形更新 _物理変形更新;

        private SharpDX.Direct3D11.Buffer _GlobalParameters定数バッファ;

        private SamplerState _PS用SamplerState;


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
