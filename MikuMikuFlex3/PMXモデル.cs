﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SharpDX;

#pragma warning disable 0649

namespace MikuMikuFlex3
{
    public class PMXモデル : IDisposable
    {

        // 生成と終了


        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, string PMXファイルパス )
        {
            using( var stream = new FileStream( PMXファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            {
                this._読み込む( d3dDevice, stream, リソースを開く: ( file ) => {

                    var baseFolder = Path.GetDirectoryName( PMXファイルパス );
                    var path = Path.Combine( baseFolder, file );
                    return new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read );

                } );
            }
        }

        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, Type 名前空間を持つ型, string リソース名 )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = $"{this.GetType().Namespace}.{リソース名}";

            using( var stream = assembly.GetManifestResourceStream( path ) )
            {
                this._読み込む( d3dDevice, stream, リソースを開く: ( resource ) => {

                    // PMXではテクスチャ名などにパス区切り文字を使用できるが、その区切りがなんであるかはOSに依存して
                    // PMXでは感知しないとのことなので、とりあえず '/' と '\' を想定する。
                    var rpath = resource.Replace( Path.DirectorySeparatorChar, '.' ).Replace( Path.AltDirectorySeparatorChar, '.' );    // '.' 区切りに変換

                    return assembly.GetManifestResourceStream( 名前空間を持つ型, rpath );

                } );
            }
        }

        public virtual void Dispose()
        {
            this._ボーンのモデルポーズ配列 = null;
            this._ボーンのローカル位置配列 = null;
            this._ボーンの回転配列 = null;

            this._D3DBoneTransデータストリーム?.Dispose();
            this._D3DBoneTrans定数バッファ?.Dispose();
            this._D3DBoneLocalPositionデータストリーム?.Dispose();
            this._D3DBoneLocalPosition定数バッファ?.Dispose();
            this._D3DBoneQuaternionデータストリーム?.Dispose();
            this._D3DBoneQuaternion定数バッファ?.Dispose();

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
            this._D3Dスキニングバッファデータストリーム?.Dispose();
            this._D3DスキニングバッファSRView?.Dispose();
            this._D3Dスキニングバッファ?.Dispose();

            this._D3D頂点バッファビューUAView?.Dispose();
            this._D3D頂点バッファ?.Dispose();

            foreach( var tech in this._D3Dテクニックリスト )
                tech?.Dispose();
            this._Effect?.Dispose();
            this._PMXFモデル = null;
        }

        private void _読み込む( SharpDX.Direct3D11.Device d3dDevice, Stream PMXデータ, Func<string, Stream> リソースを開く )
        {
            #region " モデルを読み込む。"
            //----------------
            this._PMXFモデル = new PMXFormat.モデル( PMXデータ );
            //----------------
            #endregion

            #region " PMXボーンリストを作成する。"
            //----------------
            {
                int ボーン数 = this._PMXFモデル.ボーンリスト.Count;

                this._PMXボーン配列 = new PMXボーン[ ボーン数 ];

                for( int i = 0; i < ボーン数; i++ )
                {
                    this._PMXボーン配列[ i ] = new PMXボーン( this._PMXFモデル.ボーンリスト[ i ], i );
                }

                for( int i = 0; i < ボーン数; i++ )
                {
                    this._PMXボーン配列[ i ].子ボーンリストを構築する( this._PMXボーン配列 );
                }
            }
            //----------------
            #endregion
            #region " ボーンのルートリストを作成する。"
            //----------------
            {
                this._ルートボーンリスト = new List<PMXボーン>();

                for( int i = 0; i < this._PMXボーン配列.Length; i++ )
                {
                    // 親ボーンを持たないのがルートボーン。
                    if( this._PMXボーン配列[ i ].PMXFボーン.親ボーンのインデックス == -1 )
                    {
                        this._ルートボーンリスト.Add( this._PMXボーン配列[ i ] );
                    }
                }
            }
            //----------------
            #endregion

            #region " エフェクトを生成する "
            //----------------
            {
                var assembly = Assembly.GetExecutingAssembly();

                using( var st = assembly.GetManifestResourceStream( this.GetType(), "Resources.Shaders.DefaultShader.cso" ) )
                {
                    var effectByteCode = new byte[ st.Length ];
                    st.Read( effectByteCode, 0, (int) st.Length );

                    this._Effect = new SharpDX.Direct3D11.Effect( d3dDevice, effectByteCode );
                }
            }
            //----------------
            #endregion
            #region " テクニックリストを生成する。"
            //----------------
            {
                var techList = new List<SharpDX.Direct3D11.EffectTechnique>();

                for( int i = 0; i < this._Effect.Description.TechniqueCount; i++ )
                {
                    var tech = this._Effect.GetTechniqueByIndex( i );

                    // 名前が重複しないテクニックのみ採択
                    if( techList.FindIndex( ( t ) => ( t.Description.Name == tech.Description.Name ) ) == -1 )
                        techList.Add( tech );
                }

                this._D3Dテクニックリスト = new List<テクニック>();

                int subsetCount = this._PMXFモデル.材質リスト.Count;

                foreach( var d3dTech in techList )
                {
                    this._D3Dテクニックリスト.Add( new テクニック( this._Effect, d3dTech, subsetCount ) );
                }

                techList.Clear();
            }
            //----------------
            #endregion

            #region " 行列を初期化する "
            //----------------
            {
                this._カメラ位置 = new Vector3( 0f, 0f, -45f );

                this._光源位置 = new Vector3( -0.5f, 1.0f, -0.5f );

                this._ワールド変換行列 = Matrix.Identity;

                this._ビュー行列 = Matrix.LookAtLH(
                    this._カメラ位置,
                    new Vector3( 0f, 10f, 0f ),     // 注視点
                    new Vector3( 0f, 1f, 0f ) );    // 上方向

                this._射影行列 = Matrix.PerspectiveFovLH(
                    MathUtil.Pi * 30f / 180f,   // 視野角[rad]
                    1.618f,                     // アスペクト比
                    1f,                         // 近面Z
                    200f );                     // 遠面Z
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

                this._入力頂点リスト = 頂点リスト.ToArray();
            }
            //----------------
            #endregion

            #region " スキニングバッファを作成する。"
            //----------------
            {
                this._D3Dスキニングバッファデータストリーム = new DataStream( this._入力頂点リスト.Length * CS_INPUT.SizeInBytes, canRead: true, canWrite: true );

                this._D3Dスキニングバッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new SharpDX.Direct3D11.BufferDescription {
                        SizeInBytes = CS_INPUT.SizeInBytes * this._入力頂点リスト.Length,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.BufferStructured,   // 構造化バッファ
                        StructureByteStride = CS_INPUT.SizeInBytes,
                    } );

                this._D3DスキニングバッファSRView = new SharpDX.Direct3D11.ShaderResourceView(
                    d3dDevice,
                    this._D3Dスキニングバッファ,  // 構造化バッファ
                    new SharpDX.Direct3D11.ShaderResourceViewDescription {
                        Format = SharpDX.DXGI.Format.Unknown,
                        Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer,
                        BufferEx = new SharpDX.Direct3D11.ShaderResourceViewDescription.ExtendedBufferResource {
                            FirstElement = 0,
                            ElementCount = this._入力頂点リスト.Length,
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
                    new SharpDX.Direct3D11.BufferDescription {
                        SizeInBytes = VS_INPUT.SizeInBytes * this._入力頂点リスト.Length,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer | SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess,  // 非順序アクセス
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.BufferAllowRawViews,   // 生ビューバッファ
                    } );

                this._D3D頂点バッファビューUAView = new SharpDX.Direct3D11.UnorderedAccessView(
                    d3dDevice,
                    this._D3D頂点バッファ,
                    new SharpDX.Direct3D11.UnorderedAccessViewDescription {
                        Format = SharpDX.DXGI.Format.R32_Typeless,
                        Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Buffer,
                        Buffer = new SharpDX.Direct3D11.UnorderedAccessViewDescription.BufferResource {
                            FirstElement = 0,
                            ElementCount = VS_INPUT.SizeInBytes * this._入力頂点リスト.Length / 4,
                            Flags = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Raw,
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
                        new SharpDX.Direct3D11.BufferDescription {
                            BindFlags = SharpDX.Direct3D11.BindFlags.IndexBuffer,
                            SizeInBytes = (int) dataStream.Length
                        } );
                }
            }
            //----------------
            #endregion
            #region " 頂点レイアウトを作成する。"
            //----------------
            {
                this._D3D頂点レイアウト = new SharpDX.Direct3D11.InputLayout(
                    d3dDevice,
                    this._Effect.GetTechniqueByName( "DefaultObject" ).GetPassByIndex( 0 ).Description.Signature,
                    VS_INPUT.VertexElements );
            }
            //----------------
            #endregion
            #region " ラスタライザステートを作成する。"
            //----------------
            {
                this._片面描画の際のラスタライザステート = new SharpDX.Direct3D11.RasterizerState( d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription {
                    CullMode = SharpDX.Direct3D11.CullMode.Back,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                } );

                this._両面描画の際のラスタライザステート = new SharpDX.Direct3D11.RasterizerState( d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription {
                    CullMode = SharpDX.Direct3D11.CullMode.None,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                } );

                this._片面描画の際のラスタライザステートLine = new SharpDX.Direct3D11.RasterizerState( d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription {
                    CullMode = SharpDX.Direct3D11.CullMode.Back,
                    FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                } );

                this._両面描画の際のラスタライザステートLine = new SharpDX.Direct3D11.RasterizerState( d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription {
                    CullMode = SharpDX.Direct3D11.CullMode.None,
                    FillMode = SharpDX.Direct3D11.FillMode.Wireframe,
                } );

                this._裏側片面描画の際のラスタライザステート = new SharpDX.Direct3D11.RasterizerState( d3dDevice, new SharpDX.Direct3D11.RasterizerStateDescription {
                    CullMode = SharpDX.Direct3D11.CullMode.Front,
                    FillMode = SharpDX.Direct3D11.FillMode.Solid,
                } );
            }
            //----------------
            #endregion

            #region " 共有テクスチャを読み込む "
            //----------------
            {
                this._共有テクスチャリスト = new (SharpDX.Direct3D11.Texture2D tex2d, SharpDX.Direct3D11.ShaderResourceView srv)[ 11 ];

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
            #region " 個別テクスチャを読み込む "
            //----------------
            {
                this._個別テクスチャリスト = new (SharpDX.Direct3D11.Texture2D tex2d, SharpDX.Direct3D11.ShaderResourceView srv)[ this._PMXFモデル.テクスチャリスト.Count ];

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

            #region " ボーン用バッファを作成する "
            //----------------
            this._ボーンのモデルポーズ配列 = new Matrix[ this._PMXFモデル.ボーンリスト.Count ];
            this._ボーンのローカル位置配列 = new Vector3[ this._PMXFモデル.ボーンリスト.Count ];
            this._ボーンの回転配列 = new Vector4[ this._PMXFモデル.ボーンリスト.Count ];
            //----------------
            #endregion
            #region " ボーン用定数バッファを作成する "
            //----------------
            {
                this._D3DBoneTransデータストリーム = new DataStream( this._入力頂点リスト.Length * _D3DBoneTrans.SizeInBytes, canRead: true, canWrite: true );

                this._D3DBoneTrans定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new SharpDX.Direct3D11.BufferDescription {
                        SizeInBytes = _入力頂点リスト.Length * _D3DBoneTrans.SizeInBytes,
                        BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                    } );


                this._D3DBoneLocalPositionデータストリーム = new DataStream( this._入力頂点リスト.Length * _D3DBoneLocalPosition.SizeInBytes, canRead: true, canWrite: true );

                this._D3DBoneLocalPosition定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new SharpDX.Direct3D11.BufferDescription {
                        SizeInBytes = _入力頂点リスト.Length * _D3DBoneLocalPosition.SizeInBytes,
                        BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                    } );


                this._D3DBoneQuaternionデータストリーム = new DataStream( this._入力頂点リスト.Length * _D3DBoneQuaternion.SizeInBytes, canRead: true, canWrite: true );

                this._D3DBoneQuaternion定数バッファ = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new SharpDX.Direct3D11.BufferDescription {
                        SizeInBytes = _入力頂点リスト.Length * _D3DBoneQuaternion.SizeInBytes,
                        BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                    } );
            }
            //----------------
            #endregion
        }


        // 進行と描画


        /// <summary>
        ///     現在時刻におけるモデルの各種状態を更新する。
        /// </summary>
        public void 進行する()
        {
            foreach( var root in this._ルートボーンリスト )
                root.モデルポーズを更新する();

            for( int i = 0; i < this._PMXボーン配列.Length; i++ )
            {
                this._ボーンのモデルポーズ配列[ i ] = this._PMXボーン配列[ i ].モデルポーズ行列;
                this._ボーンのモデルポーズ配列[ i ].Transpose();  // SharpDX.Matrix は行優先だが HLSL の既定は列優先
                this._ボーンのローカル位置配列[ i ] = this._PMXボーン配列[ i ].ローカル位置;
                this._ボーンの回転配列[ i ] = new Vector4( this._PMXボーン配列[ i ].回転.ToArray() );
            }

        }

        /// <summary>
        ///     進行処理によって得られた各種状態を描画する。
        /// </summary>
        /// <param name="d3ddc">描画先のデバイスコンテキスト。</param>
        /// <param name="viewport">描画先ビューポートのサイズ。</param>
        public void 描画する( SharpDX.Direct3D11.DeviceContext d3ddc, ViewportF viewport )
        {
            #region " スキニングを行う。"
            //----------------
            {
                // ボーン用定数バッファを更新する。
                this._D3DBoneTransデータストリーム.WriteRange( this._ボーンのモデルポーズ配列 );
                this._D3DBoneTransデータストリーム.Position = 0;
                d3ddc.UpdateSubresource( new DataBox( this._D3DBoneTransデータストリーム.DataPointer, 0, 0 ), this._D3DBoneTrans定数バッファ, 0 );
                this._Effect.GetConstantBufferByName( "BoneTransBuffer" ).SetConstantBuffer( this._D3DBoneTrans定数バッファ );

                this._D3DBoneLocalPositionデータストリーム.WriteRange( this._ボーンのローカル位置配列 );
                this._D3DBoneLocalPositionデータストリーム.Position = 0;
                d3ddc.UpdateSubresource( new DataBox( this._D3DBoneLocalPositionデータストリーム.DataPointer, 0, 0 ), this._D3DBoneLocalPosition定数バッファ, 0 );
                this._Effect.GetConstantBufferByName( "BoneLocalPositionBuffer" ).SetConstantBuffer( this._D3DBoneLocalPosition定数バッファ );

                this._D3DBoneQuaternionデータストリーム.WriteRange( this._ボーンの回転配列 );
                this._D3DBoneQuaternionデータストリーム.Position = 0;
                d3ddc.UpdateSubresource( new DataBox( this._D3DBoneQuaternionデータストリーム.DataPointer, 0, 0 ), this._D3DBoneQuaternion定数バッファ, 0 );
                this._Effect.GetConstantBufferByName( "BoneQuaternionBuffer" ).SetConstantBuffer( this._D3DBoneQuaternion定数バッファ );

                // 入力頂点リスト[] を D3Dスキニングバッファへ転送する。
                this._D3Dスキニングバッファデータストリーム.WriteRange( this._入力頂点リスト );
                this._D3Dスキニングバッファデータストリーム.Position = 0;
                d3ddc.UpdateSubresource( new DataBox( _D3Dスキニングバッファデータストリーム.DataPointer, 0, 0 ), this._D3Dスキニングバッファ, 0 );

                // 使用するtechniqueを検索する。
                テクニック technique =
                    ( from teq in this._D3Dテクニックリスト
                      where
                        teq.テクニックを適用する描画対象 == MMDPass種別.スキニング
                      select teq ).FirstOrDefault();

                if( null != technique )
                {
                    // パスを通じてコンピュートシェーダーステートを設定する。
                    technique.パスリスト.ElementAt( 0 ).Value.D3DPass.Apply( d3ddc );

                    // コンピュートシェーダーでスキニングを実行し、結果を頂点バッファに格納する。
                    d3ddc.ComputeShader.SetShaderResource( 0, this._D3DスキニングバッファSRView );
                    d3ddc.ComputeShader.SetUnorderedAccessView( 0, this._D3D頂点バッファビューUAView );
                    d3ddc.Dispatch( ( this._入力頂点リスト.Length / 64 ) + 1, 1, 1 );
                }

                // UAVを外す（このあと頂点シェーダーが使えるように）
                d3ddc.ComputeShader.SetUnorderedAccessView( 0, null );
            }
            //----------------
            #endregion

            // すべての材質を描画する。
            for( int i = 0; i < this._PMXFモデル.材質リスト.Count; i++ )
            {
                var 材質 = this._PMXFモデル.材質リスト[ i ];

                #region " D3Dパイプラインを構築する。"
                //----------------
                {
                    d3ddc.InputAssembler.SetVertexBuffers( 0, new SharpDX.Direct3D11.VertexBufferBinding( this._D3D頂点バッファ, VS_INPUT.SizeInBytes, 0 ) );
                    d3ddc.InputAssembler.SetIndexBuffer( this._D3Dインデックスバッファ, SharpDX.DXGI.Format.R32_UInt, 0 );
                    d3ddc.InputAssembler.InputLayout = this._D3D頂点レイアウト;
                    d3ddc.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PatchListWith3ControlPoints;

                    d3ddc.Rasterizer.SetViewport( viewport );
                }
                //----------------
                #endregion

                #region " エフェクト変数を設定する。"
                //----------------
                for( int j = 0; j < this._Effect.Description.GlobalVariableCount; j++ )
                {
                    var 変数 = this._Effect.GetVariableByIndex( j );

                    switch( 変数.Description.Semantic?.ToUpper() )
                    {
                        case "EDGECOLOR":
                            変数.AsVector().Set( 材質.エッジ色 );
                            break;

                        case "EDGEWIDTH":
                            変数.AsScalar().Set( 材質.エッジサイズ );
                            break;

                        case "WORLDVIEWPROJECTION":
                            変数.AsMatrix().SetMatrix( this._ワールド変換行列 * this._ビュー行列 * this._射影行列 );
                            break;

                        case "WORLDVIEW":
                            変数.AsMatrix().SetMatrix( this._ワールド変換行列 * this._ビュー行列 );
                            break;

                        case "WORLD":
                            変数.AsMatrix().SetMatrix( this._ワールド変換行列 );
                            break;

                        case "VIEW":
                            変数.AsMatrix().SetMatrix( this._ビュー行列 );
                            break;

                        case "VIEWPROJECTION":
                            変数.AsMatrix().SetMatrix( this._ビュー行列 * this._射影行列 );
                            break;

                        case "VIEWPORTPIXELSIZE":
                            変数.AsVector().Set( viewport );
                            break;

                        case "POSITION":
                            switch( 変数.GetAnnotationByName( "object" ).AsString().GetString().ToLower() )
                            {
                                case "camera":
                                    switch( 変数.TypeInfo.Description.TypeName )
                                    {
                                        case "float4":
                                            変数.AsVector().Set( new Vector4( this._カメラ位置, 0f ) );
                                            break;

                                        case "float3":
                                            変数.AsVector().Set( this._カメラ位置 );
                                            break;
                                    }
                                    break;

                                case "light":
                                    変数.AsVector().Set( new Vector4( this._光源位置, 0f ) );
                                    break;
                            }
                            break;

                        case "MATERIALTEXTURE":
                            if( -1 != 材質.通常テクスチャの参照インデックス )
                            {
                                変数.AsShaderResource().SetResource( this._個別テクスチャリスト[ 材質.通常テクスチャの参照インデックス ].srv );
                            }
                            break;

                        case "MATERIALSPHEREMAP":
                            if( -1 != 材質.スフィアテクスチャの参照インデックス )
                            {
                                変数.AsShaderResource().SetResource( this._個別テクスチャリスト[ 材質.スフィアテクスチャの参照インデックス ].srv );
                            }
                            break;

                        case "MATERIALTOONTEXTURE":
                            if( 1 == 材質.共有Toonフラグ )
                            {
                                変数.AsShaderResource().SetResource( this._共有テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                            }
                            else if( -1 != 材質.共有Toonのテクスチャ参照インデックス )
                            {
                                変数.AsShaderResource().SetResource( this._個別テクスチャリスト[ 材質.共有Toonのテクスチャ参照インデックス ].srv );
                            }
                            else
                            {
                                変数.AsShaderResource().SetResource( this._共有テクスチャリスト[ 0 ].srv );
                            }
                            break;

                        case "TESSFACTOR":
                            変数.AsScalar().Set( 1.0f );
                            break;

                        default:
                            switch( 変数.Description.Name.ToLower() )
                            {
                                case "use_spheremap":
                                    変数.AsScalar().Set( 材質.スフィアテクスチャの参照インデックス != -1 );
                                    break;

                                case "spadd":
                                    変数.AsScalar().Set( 材質.スフィアモード == PMXFormat.スフィアモード.加算 );
                                    break;

                                case "use_texture":
                                    変数.AsScalar().Set( 材質.通常テクスチャの参照インデックス != -1 );
                                    break;

                                case "use_toontexturemap":
                                    変数.AsScalar().Set( 材質.共有Toonのテクスチャ参照インデックス != -1 );
                                    break;

                                case "use_selfshadow":
                                    変数.AsScalar().Set( 材質.描画フラグ.HasFlag( PMXFormat.描画フラグ.セルフ影 ) );
                                    break;

                                case "ambientcolor":
                                    変数.AsVector().Set( new Vector4( 材質.環境色, 1f ) );
                                    break;

                                case "diffusecolor":
                                    変数.AsVector().Set( 材質.拡散色 );
                                    break;

                                case "specularcolor":
                                    変数.AsVector().Set( new Vector4( 材質.反射色, 1f ) );
                                    break;

                                case "specularpower":
                                    変数.AsScalar().Set( 材質.反射強度 );
                                    break;
                            }
                            break;
                    }
                }
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


                // セルフ影描画


            }
        }



        // private


        private PMXFormat.モデル _PMXFモデル;

        private PMXボーン[] _PMXボーン配列;

        private (SharpDX.Direct3D11.Texture2D tex2d, SharpDX.Direct3D11.ShaderResourceView srv)[] _共有テクスチャリスト;

        private (SharpDX.Direct3D11.Texture2D tex2d, SharpDX.Direct3D11.ShaderResourceView srv)[] _個別テクスチャリスト;

        private SharpDX.Direct3D11.Effect _Effect;

        private List<テクニック> _D3Dテクニックリスト;

        private CS_INPUT[] _入力頂点リスト;

        private const int _最大ボーン数 = 768;

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
        private DataStream _D3DBoneTransデータストリーム;
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
        private DataStream _D3DBoneLocalPositionデータストリーム;
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
        private DataStream _D3DBoneQuaternionデータストリーム;
        private SharpDX.Direct3D11.Buffer _D3DBoneQuaternion定数バッファ;

        private Vector3 _カメラ位置;

        private Vector3 _光源位置;

        private Matrix _ワールド変換行列;

        private Matrix _ビュー行列;

        private Matrix _射影行列;

        private SharpDX.Direct3D11.Buffer _D3Dスキニングバッファ;

        private SharpDX.Direct3D11.ShaderResourceView _D3DスキニングバッファSRView;

        private DataStream _D3Dスキニングバッファデータストリーム;

        private SharpDX.Direct3D11.Buffer _D3D頂点バッファ;

        private SharpDX.Direct3D11.UnorderedAccessView _D3D頂点バッファビューUAView;

        private SharpDX.Direct3D11.Buffer _D3Dインデックスバッファ;

        private SharpDX.Direct3D11.InputLayout _D3D頂点レイアウト;

        private SharpDX.Direct3D11.RasterizerState _裏側片面描画の際のラスタライザステート;

        private SharpDX.Direct3D11.RasterizerState _片面描画の際のラスタライザステート;

        private SharpDX.Direct3D11.RasterizerState _片面描画の際のラスタライザステートLine;

        private SharpDX.Direct3D11.RasterizerState _両面描画の際のラスタライザステート;

        private SharpDX.Direct3D11.RasterizerState _両面描画の際のラスタライザステートLine;

        private List<PMXボーン> _ルートボーンリスト;


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

        private void _エフェクトを適用しつつ材質を描画する( SharpDX.Direct3D11.DeviceContext d3ddc, PMXFormat.材質 ipmxSubset, MMDPass種別 passType, Action<PMXFormat.材質> drawAction )
        {
            if( ipmxSubset.拡散色.W == 0 )
                return;

            // 使用するtechniqueを検索する

            テクニック technique =
                ( from teq in _D3Dテクニックリスト
                  where
                    //teq.描画するサブセットIDの集合.Contains( ipmxSubset.サブセットID ) &&
                    teq.テクニックを適用する描画対象 == passType
                  select teq ).FirstOrDefault();

            if( null != technique )
            {
                // 最初の１つだけ有効（複数はないはずだが）
                technique.パスの適用と描画をパスの数だけ繰り返す( d3ddc, drawAction, ipmxSubset );
            }
        }
    }
}
