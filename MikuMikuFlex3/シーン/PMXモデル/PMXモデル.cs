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
    public class PMXModel : IDisposable
    {

        // 構造


        public Matrix WorldTransformationMatrix { get; set; } = Matrix.Identity;

        public const int MaximumNumberOfBones = 768;

        public PMXBoneControl[] BoneList { get; protected set; }

        public PMXMaterialControl[] MaterialList { get; protected set; }

        public PMXMorphControl[] MorphList { get; protected set; }

        public List<PMXBoneControl> RootBoneList { get; protected set; }

        public List<PMXBoneControl> IKBoneList { get; private protected set; }

        public PMXVertexControl PMXVertexControl { get; private protected set; }

        public uint[] IndexList { get; protected set; }

        public PMXFormat.Model Format { get; protected set; }



        // シェーダー


        public ISkinning SkinningShader { get; set; }

        private bool _ReleaseSkinning;


        /// <summary>
        ///     DefaultMaterialDrawingShader。
        ///     材質ごとの <see cref="PMXMaterialControl.MaterialDrawingShader"/> が null のときに使用される。
        /// </summary>
        public IMaterialShader DefaultMaterialDrawingShader { get; set; }

        private bool _ReleaseMaterialDrawing;



        // 生成と終了


        /// <summary>
        ///     ファイルからPMXLoadTheModel。
        /// </summary>
        /// <remarks>
        ///     PMXファイルで使用されるテクスチャ等のリソースファイルは、
        ///     PMXファイルと同じフォルダを基準として検索される。
        /// </remarks>
        public PMXModel( SharpDX.Direct3D11.Device d3dDevice, string PMXFilePath, ISkinning skinning = null, IMaterialShader DefaultMaterialShader = null )
        {
            var stream = new FileStream( PMXFilePath, FileMode.Open, FileAccess.Read, FileShare.Read );

            this._ReadAndInitialize( d3dDevice, stream, skinning, DefaultMaterialShader, OpenResource: ( file ) => {

                var baseFolder = Path.GetDirectoryName( PMXFilePath );
                var path = Path.Combine( baseFolder, file );
                return new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.Read );

            } );

            // stream は、上記初期化作業の中で閉じられる。
        }

        /// <summary>
        ///     埋め込みリソースからPMXLoadTheModel。
        /// </summary>
        /// <remarks>
        ///     PMXリソースで使用されるテクスチャ等のリソースは、
        ///     PMXリソースと同じ名前空間を基準として検索される。
        /// </remarks>
        public PMXModel( SharpDX.Direct3D11.Device d3dDevice, Type NamespaceType, string ResourceName, ISkinning skinning = null, IMaterialShader DefaultMaterialShader = null )
        {
            var assembly = Assembly.GetExecutingAssembly();
            var path = $"{this.GetType().Namespace}.{ResourceName}";

            var stream = assembly.GetManifestResourceStream( path );

            this._ReadAndInitialize( d3dDevice, stream, skinning, DefaultMaterialShader, OpenResource: ( resource ) => {

                // PMXではテクスチャ名などにパス区切り文字を使用できるが、その区切りがなんであるかはOSに依存して
                // PMXでは感知しないとのことなので、とりあえず '/' と '\' を想定する。
                var rpath = resource.Replace( Path.DirectorySeparatorChar, '.' ).Replace( Path.AltDirectorySeparatorChar, '.' );    // '.' 区切りに変換

                return assembly.GetManifestResourceStream( NamespaceType, rpath );

            } );

            // stream は、上記初期化作業の中で閉じられる。
        }

        private void _ReadAndInitialize( SharpDX.Direct3D11.Device d3dDevice, Stream PMXData, ISkinning skinning, IMaterialShader DefaultMaterialShader, Func<string, Stream> OpenResource )
        {
            #region " LoadTheModel。"
            //----------------
            this.Format = new PMXFormat.Model( PMXData );

            if( this.Format.BoneList.Count > MaximumNumberOfBones )
                throw new Exception( "TooManyBones。" );

            PMXData.Dispose();
            //----------------
            #endregion
            #region " CreateABoneList。"
            //----------------
            {
                int NumberOfBones = this.Format.BoneList.Count;
                this.BoneList = new PMXBoneControl[ NumberOfBones ];

                for( int i = 0; i < NumberOfBones; i++ )
                    this.BoneList[ i ] = new PMXBoneControl( this.Format.BoneList[ i ], i );

                for( int i = 0; i < NumberOfBones; i++ )
                    this.BoneList[ i ].PerformPostReadingProcessing( this.BoneList );
            }
            //----------------
            #endregion
            #region " IKCreateABoneList。"
            //----------------
            {
                var ikBones = this.BoneList.Where( ( bone ) => bone.PMXFBourne.IKBone );

                this.IKBoneList = new List<PMXBoneControl>( ikBones.Count() );

                for( int i = 0; i < ikBones.Count(); i++ )
                    this.IKBoneList.Add( ikBones.ElementAt( i ) );
            }
            //----------------
            #endregion
            #region " CreateARouteListForBones。"
            //----------------
            {
                this.RootBoneList = new List<PMXBoneControl>();

                for( int i = 0; i < this.BoneList.Length; i++ )
                {
                    // 親ボーンを持たないのがルートボーン。
                    if( this.BoneList[ i ].PMXFBourne.ParentBoneIndex == -1 )
                    {
                        this.RootBoneList.Add( this.BoneList[ i ] );
                    }
                }
            }
            //----------------
            #endregion
            #region " PMXSetTheDeformationHierarchyOfBones。"
            //----------------
            {
                foreach( var root in this.RootBoneList )
                {
                    Setting( root, 0 );
                }

                void Setting( PMXBoneControl bone, int layer )
                {
                    bone.TransformationHierarchy = layer;

                    foreach( var child in bone.ChildBoneList )
                        Setting( child, layer + 1 );
                }
            }
            //----------------
            #endregion
            #region " SortBones。"
            //----------------
            {
                var comparison = new Comparison<PMXBoneControl>( ( x, y ) => {

                    // 後であればあるほどスコアが大きくなるように計算する

                    int xScore = 0;
                    int yScore = 0;
                    int BoneCount = this.BoneList.Length;

                    if( x.PMXFBourne.PostPhysicalDeformation )
                    {
                        xScore += BoneCount * BoneCount;
                    }
                    if( y.PMXFBourne.PostPhysicalDeformation )
                    {
                        yScore += BoneCount * BoneCount;
                    }
                    xScore += BoneCount * x.TransformationHierarchy;
                    yScore += BoneCount * y.TransformationHierarchy;
                    xScore += x.BoneIndex;
                    yScore += y.BoneIndex;
                    return xScore - yScore;

                } );

                this.IKBoneList.Sort( comparison );
                this.RootBoneList.Sort( comparison );
            }
            //----------------
            #endregion
            #region " ByParentalGrantFKを初期化する。"
            //----------------
            {
                this._ByParentalGrantFKDeformationUpdate = new ByParentalGrantFKDeformationUpdate( this.BoneList );
            }
            //----------------
            #endregion
            #region " InitializePhysicalDeformation。"
            //----------------
            {
                this._PhysicalTransformationUpdate = new PMXPhysicalTransformationUpdate( this.BoneList, this.Format.RigidBodyList, this.Format.JointList );
            }
            //----------------
            #endregion
            #region " CreateAMaterialList。"
            //----------------
            {
                int NumberOfMaterials = this.Format.MaterialList.Count;

                this.MaterialList = new PMXMaterialControl[ NumberOfMaterials ];

                for( int i = 0; i < NumberOfMaterials; i++ )
                    this.MaterialList[ i ] = new PMXMaterialControl( this.Format.MaterialList[ i ], DefaultMaterialShader );
            }
            //----------------
            #endregion
            #region " CreateAMorphList。"
            //----------------
            {
                int NumberOfMorphs = this.Format.MorphList.Count;

                this.MorphList = new PMXMorphControl[ NumberOfMorphs ];

                for( int i = 0; i < NumberOfMorphs; i++ )
                    this.MorphList[ i ] = new PMXMorphControl( this.Format.MorphList[ i ] );
            }
            //----------------
            #endregion

            #region " PMXGenerateVertexControl。"
            //----------------
            {
                var VertexList = new List<CS_INPUT>( this.Format.VertexList.Count );

                for( int i = 0; i < this.Format.VertexList.Count; i++ )
                {
                    this._AddVertexDataToTheVertexLayoutList( this.Format.VertexList[ i ], VertexList );
                }

                this.PMXVertexControl = new PMXVertexControl( VertexList.ToArray() );
            }
            //----------------
            #endregion
            #region " CreateASkinningBuffer。"
            //----------------
            {
                this._D3DSkinningBuffer = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = CS_INPUT.SizeInBytes * this.PMXVertexControl.InputVertexArray.Length,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.ShaderResource,// | BindFlags.UnorderedAccess,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.BufferStructured,   // 構造化バッファ
                        StructureByteStride = CS_INPUT.SizeInBytes,
                    } );

                this._D3DSkinningBufferSRView = new ShaderResourceView(
                    d3dDevice,
                    this._D3DSkinningBuffer,  // 構造化バッファ
                    new ShaderResourceViewDescription {
                        Format = SharpDX.DXGI.Format.Unknown,
                        Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                        BufferEx = new ShaderResourceViewDescription.ExtendedBufferResource {
                            FirstElement = 0,
                            ElementCount = this.PMXVertexControl.InputVertexArray.Length,
                        },
                    } );
            }
            //----------------
            #endregion
            #region " CreateAVertexBuffer。"
            //----------------
            {
                this._D3DVertexBuffer = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = VS_INPUT.SizeInBytes * this.PMXVertexControl.InputVertexArray.Length,
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.VertexBuffer | BindFlags.ShaderResource | BindFlags.UnorderedAccess,  // 非順序アクセス
                            CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.BufferAllowRawViews,   // 生ビューバッファ
                        } );

                this._D3DVertexBufferViewUAView = new UnorderedAccessView(
                    d3dDevice,
                    this._D3DVertexBuffer,
                    new UnorderedAccessViewDescription {
                        Format = SharpDX.DXGI.Format.R32_Typeless,
                        Dimension = UnorderedAccessViewDimension.Buffer,
                        Buffer = new UnorderedAccessViewDescription.BufferResource {
                            FirstElement = 0,
                            ElementCount = VS_INPUT.SizeInBytes * this.PMXVertexControl.InputVertexArray.Length / 4,
                            Flags = UnorderedAccessViewBufferFlags.Raw,
                        },
                    } );
            }
            //----------------
            #endregion
            #region " CreateAVertexLayout。"
            //----------------
            {
                var assembly = Assembly.GetExecutingAssembly();
                using( var fs = assembly.GetManifestResourceStream( this.GetType(), "Resources.Shaders.DefaultVertexShaderForObject.cso" ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );

                    this._D3DVertexLayout = new InputLayout( d3dDevice, buffer, VS_INPUT.VertexElements );
                }
            }
            //----------------
            #endregion
            #region " CreateAnIndexBuffer。"
            //----------------
            {
                var indexList = new List<uint>();

                foreach( PMXFormat.Surface surface in this.Format.FaceList )
                {
                    indexList.Add( surface.Vertex1 );
                    indexList.Add( surface.Vertex2 );
                    indexList.Add( surface.Vertex3 );
                }

                this.IndexList = indexList.ToArray();

                using( var dataStream = DataStream.Create( this.IndexList, true, true ) )
                {
                    this._D3DIndexBuffer = new SharpDX.Direct3D11.Buffer(
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

            #region " CreateARasterizerState。"
            //----------------
            {
                this._RasterizerStateWhenDrawingOnOneSide = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Solid,
                } );

                this._RasterizerStateForDoubleSidedDrawing = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Solid,
                } );

                this._RasterizerStateWhenDrawingOnOneSideLine = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                    CullMode = CullMode.Back,
                    FillMode = FillMode.Wireframe,
                } );

                this._RasterizerStateForDoubleSidedDrawingLine = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                    CullMode = CullMode.None,
                    FillMode = FillMode.Wireframe,
                } );

                this._RasterizerStateWhenDrawingOnOneSideOfTheBackSide = new RasterizerState( d3dDevice, new RasterizerStateDescription {
                    CullMode = CullMode.Front,
                    FillMode = FillMode.Solid,
                } );
            }
            //----------------
            #endregion
            #region " CreateASamplerState。"
            //----------------
            this._PSForSamplerState = new SamplerState( d3dDevice, new SamplerStateDescription {
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
            #region " CreateABlendState。"
            //----------------
            {
                var blendStateNorm = new BlendStateDescription() {
                    AlphaToCoverageEnable = false,  // アルファマスクで透過する（するならZバッファ必須）
                    IndependentBlendEnable = false, // 個別設定。false なら BendStateDescription.RenderTarget[0] だけが有効で、[1～7] は無視される。
                };
                blendStateNorm.RenderTarget[ 0 ].IsBlendEnabled = true; // true ならブレンディングが有効。
                blendStateNorm.RenderTarget[ 0 ].RenderTargetWriteMask = SharpDX.Direct3D11.ColorWriteMaskFlags.All;

                blendStateNorm.RenderTarget[ 0 ].SourceBlend = BlendOption.SourceAlpha;                  // 色値のブレンディング
                blendStateNorm.RenderTarget[ 0 ].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendStateNorm.RenderTarget[ 0 ].BlendOperation = BlendOperation.Add;

                blendStateNorm.RenderTarget[ 0 ].SourceAlphaBlend = BlendOption.One;                     // アルファ値のブレンディング
                blendStateNorm.RenderTarget[ 0 ].DestinationAlphaBlend = BlendOption.Zero;
                blendStateNorm.RenderTarget[ 0 ].AlphaBlendOperation = BlendOperation.Add;

                this._BlendStateNormalSynthesis = new BlendState( d3dDevice, blendStateNorm );
            }
            //----------------
            #endregion

            #region " LoadSharedTextures。"
            //----------------
            {
                this._SharedTextureList = new (Texture2D tex2d, ShaderResourceView srv)[ 11 ];

                var SharedTexturePath = new string[] {
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
                    this._SharedTextureList[ i ] = (null, null);

                    var path = $"{this.GetType().Namespace}.{SharedTexturePath[ i ]}";

                    try
                    {
                        if( null != assembly.GetManifestResourceInfo( path ) )
                        {
                            var stream = assembly.GetManifestResourceStream( path );
                            var srv = MMFShaderResourceView.FromStream( d3dDevice, stream, out var tex2d );

                            this._SharedTextureList[ i ] = (tex2d, srv);
                        }
                    }
                    catch( Exception e )
                    {
                        Trace.TraceError( $"FailedToLoadSharedTexture。[{path}][{e.Message}]" );
                    }
                }
            }
            //----------------
            #endregion
            #region " LoadIndividualTextures。"
            //----------------
            {
                this._IndividualTextureList = new (Texture2D tex2d, ShaderResourceView srv)[ this.Format.TextureList.Count ];

                for( int i = 0; i < this.Format.TextureList.Count; i++ )
                {
                    this._IndividualTextureList[ i ] = (null, null);

                    var texturePath = this.Format.TextureList[ i ];
                    var Extension = Path.GetExtension( texturePath ).ToLower();

                    Debug.Write( $"Loading {texturePath} ... " );

                    try
                    {
                        using( var stream = OpenResource( texturePath ) )    // 開く方法は呼び出し元に任せる
                        {
                            var srv = MMFShaderResourceView.FromStream(
                                d3dDevice,
                                ( Extension == ".tga" ) ? TargaSolver.LoadTargaImage( stream ) : stream,
                                out var tex2d );

                            this._IndividualTextureList[ i ] = (tex2d, srv);

                            Debug.WriteLine( "OK" );
                        }
                    }
                    catch( Exception e )
                    {
                        Debug.WriteLine( "error!" );
                        Trace.TraceError( $"FailedToLoadIndividualTextureFile。[{texturePath}][{e.Message}]" );
                    }
                }
            }
            //----------------
            #endregion

            #region " CreateAnArrayForBones。"
            //----------------
            this._BoneModelPoseArray = new Matrix[ this.Format.BoneList.Count ];
            this._LocalPositionArrayOfBones = new Vector4[ this.Format.BoneList.Count ];
            this._RotationalArrayOfBones = new Vector4[ this.Format.BoneList.Count ];
            //----------------
            #endregion
            #region " CreateAConstantBufferForBones。"
            //----------------
            {
                this._D3DBoneTransConstantBuffer = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.BoneList.Length * D3DBoneTrans.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );

                this._D3DBoneLocalPositionConstantBuffer = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.BoneList.Length * D3DBoneLocalPosition.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );

                this._D3DBoneQuaternionConstantBuffer = new SharpDX.Direct3D11.Buffer(
                    d3dDevice,
                    new BufferDescription {
                        SizeInBytes = this.BoneList.Length * D3DBoneQuaternion.SizeInBytes,
                        BindFlags = BindFlags.ConstantBuffer,
                    } );
            }
            //----------------
            #endregion

            #region " CreateAGlobalParameterConstantBuffer。"
            //----------------
            this._GlobalParametersConstantBuffer = new SharpDX.Direct3D11.Buffer(
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
                this.SkinningShader = skinning;
                this._ReleaseSkinning = false;
            }
            else
            {
                this.SkinningShader = new DefaultSkinning( d3dDevice );
                this._ReleaseSkinning = true;
            }
            //----------------
            #endregion
            #region " IMaterialShader を作成する。"
            //----------------
            if( null != DefaultMaterialShader )
            {
                this.DefaultMaterialDrawingShader = DefaultMaterialShader;
                this._ReleaseMaterialDrawing = false;
            }
            else
            {
                this.DefaultMaterialDrawingShader = new DefaultMaterialShader( d3dDevice );
                this._ReleaseMaterialDrawing = true;
            }

            //----------------
            #endregion

            this._InitializationCompletion.Set();
        }

        public virtual void Dispose()
        {
            this._InitializationCompletion.Reset();

            if( this._ReleaseMaterialDrawing )
                this.DefaultMaterialDrawingShader?.Dispose();

            if( this._ReleaseSkinning )
                this.SkinningShader?.Dispose();

            this._GlobalParametersConstantBuffer?.Dispose();

            this._PhysicalTransformationUpdate?.Dispose();
            this._D3DBoneTransConstantBuffer?.Dispose();
            this._D3DBoneLocalPositionConstantBuffer?.Dispose();
            this._D3DBoneQuaternionConstantBuffer?.Dispose();
            this._BoneModelPoseArray = null;
            this._LocalPositionArrayOfBones = null;
            this._RotationalArrayOfBones = null;

            foreach( var pair in this._IndividualTextureList )
            {
                pair.srv?.Dispose();
                pair.tex2d?.Dispose();
            }
            foreach( var pair in this._SharedTextureList )
            {
                pair.srv?.Dispose();
                pair.tex2d?.Dispose();
            }

            this._PSForSamplerState?.Dispose();
            this._BlendStateNormalSynthesis?.Dispose();
            this._RasterizerStateWhenDrawingOnOneSideOfTheBackSide?.Dispose();
            this._RasterizerStateWhenDrawingOnOneSide?.Dispose();
            this._RasterizerStateWhenDrawingOnOneSideLine?.Dispose();
            this._RasterizerStateForDoubleSidedDrawing?.Dispose();
            this._RasterizerStateForDoubleSidedDrawingLine?.Dispose();
            this._D3DVertexLayout?.Dispose();
            this._D3DIndexBuffer?.Dispose();
            this._D3DVertexBufferViewUAView?.Dispose();
            this._D3DVertexBuffer?.Dispose();
            this._D3DSkinningBufferSRView?.Dispose();
            this._D3DSkinningBuffer?.Dispose();

            this.PMXVertexControl = null;

            foreach( var morph in this.MorphList )
                morph.Dispose();
            this.MorphList = null;

            foreach( var material in this.MaterialList )
                material.Dispose();
            this.MaterialList = null;

            this.RootBoneList = null;
            this.IKBoneList = null;
            this._ByParentalGrantFKDeformationUpdate = null;
            foreach( var bone in this.BoneList )
                bone.Dispose();
            this.BoneList = null;

            this.Format = null;
        }

        private ManualResetEventSlim _InitializationCompletion = new ManualResetEventSlim( false );



        // 進行と描画


        /// <summary>
        ///     現在時刻におけるモデルの状態を更新し、Draw。
        /// </summary>
        /// <param name="CurrentTimesec">CurrentTime[Seconds]。</param>
        /// <param name="d3ddc">描画先のデバイスコンテキスト。</param>
        /// <param name="WorldTransformationMatrix">モデルに適用するワールド変換行列。</param>
        /// <param name="camera">モデルに適用するカメラ。</param>
        /// <param name="light">モデルに適用する照明。</param>
        /// <param name="viewport">描画先ビューポートのサイズ。</param>
        public void Draw( double CurrentTimesec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            if( !this._InitializationCompletion.IsSet )
                this._InitializationCompletion.Wait();


            // 進行


            #region " ResetTheStateOfTheMaterial。"
            //----------------
            foreach( var mat in this.MaterialList )
                mat.ResetState();
            //----------------
            #endregion

            #region " ResetVertexState。"
            //----------------
            this.PMXVertexControl.ResetState( this.Format.Header.AddToUVNumber, this.Format.VertexList );
            //----------------
            #endregion

            #region " ResetBoneState。"
            //----------------
            foreach( var bone in this.BoneList )
            {
                bone.LocalLocation = bone.PMXFBourne.Position;
                bone.Move = Vector3.Zero;
                bone.Rotation = Quaternion.Identity;
            }
            //----------------
            #endregion

            this._RecalculateTheModelPose();

            #region " ApplyMorphs。"
            //----------------
            foreach( var morph in this.MorphList )
                morph.ApplyMorphs( CurrentTimesec, this );

            this._RecalculateTheModelPose();
            //----------------
            #endregion

            #region " ApplyBoneMotion。"
            //----------------
            foreach( var bone in this.BoneList )
                bone.ApplyBoneMotion( CurrentTimesec );

            this._RecalculateTheModelPose();
            //----------------
            #endregion

            #region " IKを適用する。"
            //----------------
            CCDによるIKDeformationUpdate.UpdateTransformation( this.IKBoneList );
            //----------------
            #endregion

            #region " ByParentalGrantFKを適用する。"
            //----------------
            this._ByParentalGrantFKDeformationUpdate.UpdateTransformation();
            this._RecalculateTheModelPose();
            //----------------
            #endregion
            
            #region " ApplyTransformationByPhysics。"
            //----------------
            this._PhysicalTransformationUpdate.UpdateTransformation();
            //----------------
            #endregion

            #region " WhileRecalculatingTheModelPose、DetermineTheBoneState。"
            //----------------
            foreach( var root in this.RootBoneList )
            {
                root.CalculateModelPose();
                root.ConfirmTheState( this._BoneModelPoseArray, this._LocalPositionArrayOfBones, this._RotationalArrayOfBones );
            }
            //----------------
            #endregion


            // Drawing

            #region " DoSkinning。"
            //----------------
            {
                bool UseComputeShader = true;

                if( UseComputeShader )
                {
                    #region " SkinningWithComputeShader。"
                    //----------------

                    // ボーン用定数バッファを更新する。

                    d3ddc.UpdateSubresource( this._BoneModelPoseArray, this._D3DBoneTransConstantBuffer );
                    d3ddc.UpdateSubresource( this._LocalPositionArrayOfBones, this._D3DBoneLocalPositionConstantBuffer );
                    d3ddc.UpdateSubresource( this._RotationalArrayOfBones, this._D3DBoneQuaternionConstantBuffer );


                    // 入力頂点リスト[] を D3Dスキニングバッファへ転送する。

                    if( _FirstDrawing )
                    {
                        // 初回は全部転送。
                        d3ddc.UpdateSubresource( this.PMXVertexControl.InputVertexArray, this._D3DSkinningBuffer );
                    }
                    else
                    {
                        // ２回目以降は差分のみ転送。
                        var dstRegion = new ResourceRegion( 0, 0, 0, 1, 1, 1 );

                        for( int i = 0; i < this.PMXVertexControl.UnitUpdateFlag.Length; i++ )
                        {
                            if( this.PMXVertexControl.UnitUpdateFlag[ i ] )
                            {
                                dstRegion.Left = i * PMXVertexControl.NumberOfVerticesForUnitUpdate * CS_INPUT.SizeInBytes;
                                dstRegion.Right = Math.Min( ( i + 1 ) * PMXVertexControl.NumberOfVerticesForUnitUpdate, this.PMXVertexControl.InputVertexArray.Length ) * CS_INPUT.SizeInBytes;
                                d3ddc.UpdateSubresource( ref this.PMXVertexControl.InputVertexArray[ i * PMXVertexControl.NumberOfVerticesForUnitUpdate ], this._D3DSkinningBuffer, region: dstRegion );
                            }
                        }
                    }

                    // コンピュートシェーダーでスキニングを実行し、結果を頂点バッファに格納する。

                    d3ddc.ComputeShader.SetConstantBuffer( 1, this._D3DBoneTransConstantBuffer );           // b1
                    d3ddc.ComputeShader.SetConstantBuffer( 2, this._D3DBoneLocalPositionConstantBuffer );   // b2
                    d3ddc.ComputeShader.SetConstantBuffer( 3, this._D3DBoneQuaternionConstantBuffer );      // b3
                    d3ddc.ComputeShader.SetShaderResource( 0, this._D3DSkinningBufferSRView );        // t0
                    d3ddc.ComputeShader.SetUnorderedAccessView( 0, this._D3DVertexBufferViewUAView );   // u0

                    this.SkinningShader.Run(
                        d3ddc,
                        this.PMXVertexControl.InputVertexArray.Length,
                        this._D3DBoneTransConstantBuffer,
                        this._D3DBoneLocalPositionConstantBuffer,
                        this._D3DBoneQuaternionConstantBuffer,
                        this._D3DSkinningBufferSRView,
                        this._D3DVertexBufferViewUAView );

                    // UAVを外す（このあと頂点シェーダーが使えるように）

                    d3ddc.ComputeShader.SetUnorderedAccessView( 0, null );
                    //----------------
                    #endregion
                }
                else
                {
                    #region " CPUで行う場合 "
                    //----------------
                    var boneTrans = this._BoneModelPoseArray; // コンピュートシェーダー（HLSL）用に転置されているので注意。

                    var ListOfInputVerticesAfterSkinning = new VS_INPUT[ this.PMXVertexControl.InputVertexArray.Length ];

                    for( int i = 0; i < this.PMXVertexControl.InputVertexArray.Length; i++ )
                    {
                        switch( this.PMXVertexControl.InputVertexArray[ i ].DeformationMethod )
                        {
                            case (uint) PMXFormat.BoneWeightType.BDEF1:
                                #region " *** "
                                //----------------
                                {
                                    var Vertex = this.PMXVertexControl.InputVertexArray[ i ];

                                    var bt1 = boneTrans[ Vertex.BoneIndex1 ];
                                    bt1.Transpose();

                                    Matrix bt =
                                        bt1;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    ListOfInputVerticesAfterSkinning[ i ].Position = Vector4.Transform( Vertex.Position, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal = Vector3.TransformNormal( Vertex.Normal, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.BoneWeightType.BDEF2:
                                #region " *** "
                                //----------------
                                {
                                    var Vertex = this.PMXVertexControl.InputVertexArray[ i ];

                                    var bt1 = boneTrans[ Vertex.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ Vertex.BoneIndex2 ];
                                    bt2.Transpose();

                                    Matrix bt =
                                        bt1 * Vertex.BoneWeight1 +
                                        bt2 * Vertex.BoneWeight2;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    ListOfInputVerticesAfterSkinning[ i ].Position = Vector4.Transform( Vertex.Position, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal = Vector3.TransformNormal( Vertex.Normal, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.BoneWeightType.BDEF4:
                                #region " *** "
                                //----------------
                                {
                                    var Vertex = this.PMXVertexControl.InputVertexArray[ i ];

                                    var bt1 = boneTrans[ Vertex.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ Vertex.BoneIndex2 ];
                                    bt2.Transpose();
                                    var bt3 = boneTrans[ Vertex.BoneIndex3 ];
                                    bt3.Transpose();
                                    var bt4 = boneTrans[ Vertex.BoneIndex4 ];
                                    bt4.Transpose();

                                    Matrix bt =
                                        bt1 * Vertex.BoneWeight1 +
                                        bt2 * Vertex.BoneWeight2 +
                                        bt3 * Vertex.BoneWeight3 +
                                        bt4 * Vertex.BoneWeight4;

                                    if( Matrix.Zero == bt )
                                        bt = Matrix.Identity;

                                    ListOfInputVerticesAfterSkinning[ i ].Position = Vector4.Transform( Vertex.Position, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal = Vector3.TransformNormal( Vertex.Normal, bt );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.BoneWeightType.SDEF:
                                #region " *** "
                                //----------------
                                {
                                    // 参考: 
                                    // 自分用メモ「PMXのスフィリカルデフォームのコードっぽいもの」（sma42氏）
                                    // https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60755964

                                    var Vertex = this.PMXVertexControl.InputVertexArray[ i ];

                                    var bt1 = boneTrans[ Vertex.BoneIndex1 ];
                                    bt1.Transpose();
                                    var bt2 = boneTrans[ Vertex.BoneIndex2 ];
                                    bt2.Transpose();

                                    #region " Impact0,1 の算出 "
                                    //----------------
                                    float Impact0 = 0f;  // 固定値であるSDEFパラメータにのみ依存するので、これらの値も固定値。
                                    float Impact1 = 0f;  //
                                    {
                                        float L0 = ( Vertex.SdefR0 - (Vector3) this._LocalPositionArrayOfBones[ Vertex.BoneIndex2 ] ).Length();   // 子ボーンからR0までの距離
                                        float L1 = ( Vertex.SdefR1 - (Vector3) this._LocalPositionArrayOfBones[ Vertex.BoneIndex2 ] ).Length();   // 子ボーンからR1までの距離

                                        Impact0 = ( Math.Abs( L0 - L1 ) < 0.0001f ) ? 0.5f : MathUtil.Clamp( L0 / ( L0 + L1 ), 0.0f, 1.0f );
                                        Impact1 = 1.0f - Impact0;
                                    }
                                    //----------------
                                    #endregion

                                    Matrix ModelPoseMatrixL = bt1 * Vertex.BoneWeight1;
                                    Matrix ModelPoseMatrixR = bt2 * Vertex.BoneWeight2;
                                    Matrix ModelPoseMatrixC = ModelPoseMatrixL + ModelPoseMatrixR;

                                    Vector4 PointC = Vector4.Transform( Vertex.Sdef_C, ModelPoseMatrixC );    // BDEF2で計算された点Cの位置
                                    Vector4 PointP = Vector4.Transform( Vertex.Position, ModelPoseMatrixC );  // BDEF2で計算された頂点の位置

                                    Matrix WeightedRotationMatrix = Matrix.RotationQuaternion(
                                        Quaternion.Slerp(   // 球体線形補間
                                            new Quaternion( this._RotationalArrayOfBones[ Vertex.BoneIndex1 ].ToArray() ) * Vertex.BoneWeight1,
                                            new Quaternion( this._RotationalArrayOfBones[ Vertex.BoneIndex2 ].ToArray() ) * Vertex.BoneWeight2,
                                            Vertex.BoneWeight1 ) );

                                    Vector4 PointR0 = Vector4.Transform( new Vector4( Vertex.SdefR0, 1f ), ( ModelPoseMatrixL + ( ModelPoseMatrixC * -Vertex.BoneWeight1 ) ) );
                                    Vector4 PointR1 = Vector4.Transform( new Vector4( Vertex.SdefR1, 1f ), ( ModelPoseMatrixR + ( ModelPoseMatrixC * -Vertex.BoneWeight2 ) ) );
                                    PointC += ( PointR0 * Impact0 ) + ( PointR1 * Impact1 );   // 膨らみすぎ防止

                                    PointP -= PointC;     // 頂点を点Cが中心になるよう移動して
                                    PointP = Vector4.Transform( PointP, WeightedRotationMatrix );   // 回転して
                                    PointP += PointC;     // 元の位置へ

                                    ListOfInputVerticesAfterSkinning[ i ].Position = PointP;
                                    ListOfInputVerticesAfterSkinning[ i ].Normal = Vector3.TransformNormal( Vertex.Normal, WeightedRotationMatrix );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal.Normalize();
                                }
                                //----------------
                                #endregion
                                break;

                            case (uint) PMXFormat.BoneWeightType.QDEF:
                                #region " *** "
                                //----------------
                                {
                                    // ※ QDEFを使ったモデルが見つからないのでテストしてません。あれば教えてください！

                                    var Vertex = this.PMXVertexControl.InputVertexArray[ i ];

                                    var dualQuaternion = new DualQuaternion[ 4 ];   // 最大４ボーンまで対応

                                    var boneIndexes = new[] { Vertex.BoneIndex1, Vertex.BoneIndex2, Vertex.BoneIndex3, Vertex.BoneIndex4 };
                                    var boneWeights = new[] { Vertex.BoneWeight1, Vertex.BoneWeight2, Vertex.BoneWeight3, Vertex.BoneWeight4 };

                                    var bt = new[] { boneTrans[ boneIndexes[ 0 ] ], boneTrans[ boneIndexes[ 1 ] ], boneTrans[ boneIndexes[ 2 ] ], boneTrans[ boneIndexes[ 3 ] ] };
                                    for( int b = 0; b < 4; b++ )
                                        bt[ b ].Transpose();

                                    for( int b = 0; b < 4; b++ )
                                    {
                                        if( boneWeights[ b ] == 0f )
                                        {
                                            dualQuaternion[ b ] = DualQuaternion.Zero;  // Unused
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

                                    ListOfInputVerticesAfterSkinning[ i ].Position = Vector4.Transform( Vertex.Position, btm );
                                    ListOfInputVerticesAfterSkinning[ i ].Normal = Vertex.Normal;
                                }
                                //----------------
                                #endregion
                                break;
                        }

                        ListOfInputVerticesAfterSkinning[ i ].UV = this.PMXVertexControl.InputVertexArray[ i ].UV;
                        ListOfInputVerticesAfterSkinning[ i ].AddUV1 = this.PMXVertexControl.InputVertexArray[ i ].AddUV1;
                        ListOfInputVerticesAfterSkinning[ i ].AddUV2 = this.PMXVertexControl.InputVertexArray[ i ].AddUV2;
                        ListOfInputVerticesAfterSkinning[ i ].AddUV3 = this.PMXVertexControl.InputVertexArray[ i ].AddUV3;
                        ListOfInputVerticesAfterSkinning[ i ].AddUV4 = this.PMXVertexControl.InputVertexArray[ i ].AddUV4;
                        ListOfInputVerticesAfterSkinning[ i ].EdgeWeight = this.PMXVertexControl.InputVertexArray[ i ].EdgeWeight;
                        ListOfInputVerticesAfterSkinning[ i ].Index = this.PMXVertexControl.InputVertexArray[ i ].Index;
                    }

                    d3ddc.UpdateSubresource( ListOfInputVerticesAfterSkinning, this._D3DVertexBuffer );
                    //----------------
                    #endregion
                }
            }
            //----------------
            #endregion

            #region " D3DPipeline（ModelUnit）を設定する。"
            //----------------
            {
                d3ddc.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( this._D3DVertexBuffer, VS_INPUT.SizeInBytes, 0 ) );
                d3ddc.InputAssembler.SetIndexBuffer( this._D3DIndexBuffer, SharpDX.DXGI.Format.R32_UInt, 0 );
                d3ddc.InputAssembler.InputLayout = this._D3DVertexLayout;
                d3ddc.InputAssembler.PrimitiveTopology = PrimitiveTopology.PatchListWith3ControlPoints;

                d3ddc.Rasterizer.SetViewport( new ViewportF {
                    X = 0,
                    Y = 0,
                    Width = globalParameters.ViewportSize.X,
                    Height = globalParameters.ViewportSize.Y,
                    MinDepth = 0f,
                    MaxDepth = 1f,
                } );

                d3ddc.PixelShader.SetSampler( 0, this._PSForSamplerState );
                d3ddc.OutputMerger.BlendState = this._BlendStateNormalSynthesis;
            }
            //----------------
            #endregion

            #region " GlobalParameters（ModelUnit）を設定する。"
            //----------------
            globalParameters.WorldMatrix = WorldTransformationMatrix;
            globalParameters.WorldMatrix.Transpose();
            //----------------
            #endregion

            #region " DrawAllMaterials。"
            //----------------
            for( int i = 0; i < this.MaterialList.Length; i++ )
            {
                var Material = this.MaterialList[ i ];

                #region " InMaterialMorphZMeasuresForFighting → Hide（CompletelyTransparent）ThenDraw（DrawingOnADepthStencil）DoNotDoItItself "
                //----------------
                if( Material.DiffuseColor.W < 0.0001f ) // CompletelyTransparent ＝ 拡散色のαがほぼゼロ
                    continue;
                //----------------
                #endregion

                #region " GlobalParameters（MaterialUnit）を設定する。"
                //----------------
                globalParameters.EdgeColor = Material.EdgeColor;
                globalParameters.EdgeWidth = Material.EdgeSize;
                globalParameters.TessellationFactor = Material.TessellationCoefficient;
                globalParameters.UseSelfShadow = ( Material.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.SelfShadow ) );
                globalParameters.AmbientColor = new Vector4( Material.EnvironmentalColor, 1f );
                globalParameters.DiffuseColor = Material.DiffuseColor;
                globalParameters.SpecularColor = new Vector4( Material.ReflectiveColor, 1f );
                globalParameters.SpecularPower = Material.ReflectionIntensity;

                ShaderResourceView TextureSRV = null;
                if( -1 != Material.ReferenceIndexOfNormalTexture )
                {
                    globalParameters.UseTexture = true;
                    TextureSRV = this._IndividualTextureList[ Material.ReferenceIndexOfNormalTexture ].srv;
                    d3ddc.PixelShader.SetShaderResource( 0, TextureSRV );
                }
                else
                {
                    globalParameters.UseTexture = false;
                }

                ShaderResourceView SphereMapTextureSRV = null;
                if( -1 != Material.SphereTextureReferenceIndex )
                {
                    globalParameters.UseSphereMap = true;
                    globalParameters.IsAddSphere = ( Material.SphereMode == PMXFormat.SphereMode.Addition );
                    SphereMapTextureSRV = this._IndividualTextureList[ Material.SphereTextureReferenceIndex ].srv;
                    d3ddc.PixelShader.SetShaderResource( 1, SphereMapTextureSRV );
                }
                else
                {
                    globalParameters.UseSphereMap = false;
                }

                ShaderResourceView ToonTextureSRV = null;
                if( 1 == Material.ShareToonFlag )
                {
                    globalParameters.UseToonTextureMap = true;
                    ToonTextureSRV = this._SharedTextureList[ Material.ShareToonのテクスチャ参照インデックス ].srv;
                    d3ddc.PixelShader.SetShaderResource( 2, ToonTextureSRV );
                }
                else if( -1 != Material.ShareToonのテクスチャ参照インデックス )
                {
                    globalParameters.UseToonTextureMap = true;
                    ToonTextureSRV = this._IndividualTextureList[ Material.ShareToonのテクスチャ参照インデックス ].srv;
                    d3ddc.PixelShader.SetShaderResource( 2, ToonTextureSRV );
                }
                else
                {
                    globalParameters.UseToonTextureMap = false;
                    //d3ddc.PixelShader.SetShaderResource( 2, this._SharedTextureList[ 0 ].srv );
                }
                //----------------
                #endregion

                #region " GlobalParametersConstantBufferForEachShaderStage b0 へ転送する。"
                //----------------
                d3ddc.UpdateSubresource( ref globalParameters, this._GlobalParametersConstantBuffer );

                d3ddc.VertexShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );
                d3ddc.HullShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );
                d3ddc.DomainShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );
                d3ddc.GeometryShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );
                d3ddc.PixelShader.SetConstantBuffer( 0, this._GlobalParametersConstantBuffer );
                //----------------
                #endregion

                var MaterialDrawingShader = Material.MaterialDrawingShader ?? this.DefaultMaterialDrawingShader;


                // オブジェクト描画

                #region " D3DPipeline（MaterialUnit）を設定する。"
                //----------------
                if( !Material.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.DoubleSidedDrawing ) )
                {
                    if( Material.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.LineDrawing ) )
                        d3ddc.Rasterizer.State = this._RasterizerStateWhenDrawingOnOneSideLine;
                    else
                        d3ddc.Rasterizer.State = this._RasterizerStateWhenDrawingOnOneSide;
                }
                else
                {
                    if( Material.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.LineDrawing ) )
                        d3ddc.Rasterizer.State = this._RasterizerStateForDoubleSidedDrawingLine;
                    else
                        d3ddc.Rasterizer.State = this._RasterizerStateForDoubleSidedDrawing;
                }
                //----------------
                #endregion

                MaterialDrawingShader.Draw(
                    d3ddc,
                    Material.NumberOfVertices,
                    Material.StartingIndex,
                    MMDPass.Object,
                    globalParameters,
                    this._GlobalParametersConstantBuffer,
                    TextureSRV,
                    SphereMapTextureSRV,
                    ToonTextureSRV );


                // エッジ描画

                if( Material.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.Edge ) )
                {
                    #region " D3DPipeline（MaterialUnit）を設定する。"
                    //----------------
                    d3ddc.Rasterizer.State = this._RasterizerStateWhenDrawingOnOneSideOfTheBackSide;
                    //----------------
                    #endregion

                    MaterialDrawingShader.Draw(
                        d3ddc,
                        Material.NumberOfVertices,
                        Material.StartingIndex,
                        MMDPass.Edge,
                        globalParameters,
                        this._GlobalParametersConstantBuffer,
                        TextureSRV,
                        SphereMapTextureSRV,
                        ToonTextureSRV );
                }
            }
            //----------------
            #endregion

            this._FirstDrawing = false;
        }

        private void _RecalculateTheModelPose()
        {
            foreach( var root in this.RootBoneList )
                root.CalculateModelPose();
        }

        private bool _FirstDrawing = true;



        // private


        private (Texture2D tex2d, ShaderResourceView srv)[] _SharedTextureList;

        private (Texture2D tex2d, ShaderResourceView srv)[] _IndividualTextureList;

        private Matrix[] _BoneModelPoseArray;

        private Vector4[] _LocalPositionArrayOfBones;

        private Vector4[] _RotationalArrayOfBones;

        public struct D3DBoneTrans    // サイズ計測用構造体
        {
            public Matrix boneTrans;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( Marshal.SizeOf( typeof( D3DBoneTrans ) ) ) / 16 + 1 ) * 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneTransConstantBuffer;

        public struct D3DBoneLocalPosition    // サイズ計測用構造体
        {
            public Vector3 boneLocalPosition;

            public float dummy1;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes = 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneLocalPositionConstantBuffer;

        public struct D3DBoneQuaternion    // サイズ計測用構造体
        {
            public Vector4 boneQuaternion;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( Marshal.SizeOf( typeof( D3DBoneQuaternion ) ) ) / 16 + 1 ) * 16;
        }
        private SharpDX.Direct3D11.Buffer _D3DBoneQuaternionConstantBuffer;

        private SharpDX.Direct3D11.Buffer _D3DSkinningBuffer;

        private ShaderResourceView _D3DSkinningBufferSRView;

        private SharpDX.Direct3D11.Buffer _D3DVertexBuffer;

        private InputLayout _D3DVertexLayout;

        private UnorderedAccessView _D3DVertexBufferViewUAView;

        private SharpDX.Direct3D11.Buffer _D3DIndexBuffer;

        private RasterizerState _RasterizerStateWhenDrawingOnOneSideOfTheBackSide;

        private RasterizerState _RasterizerStateWhenDrawingOnOneSide;

        private RasterizerState _RasterizerStateWhenDrawingOnOneSideLine;

        private RasterizerState _RasterizerStateForDoubleSidedDrawing;

        private RasterizerState _RasterizerStateForDoubleSidedDrawingLine;

        private BlendState _BlendStateNormalSynthesis;

        private ByParentalGrantFKDeformationUpdate _ByParentalGrantFKDeformationUpdate;

        private PMXPhysicalTransformationUpdate _PhysicalTransformationUpdate;
        public MMFlexUtil.BulletUtil.SharpDXBulletDrawer Drawer
        {
            get { return _PhysicalTransformationUpdate.Drawer; }
        }

        public RayResult CastRay( Vector3 rayStart , Vector3 rayEnd )
        {
            return _PhysicalTransformationUpdate.CastRay( rayStart , rayEnd );
        }

        private SharpDX.Direct3D11.Buffer _GlobalParametersConstantBuffer;

        private SamplerState _PSForSamplerState;


        /// <summary>
        ///     指定されたリソースパスを、埋め込みリソースまたはファイルとして開き、
        ///     Stream として返す。
        /// </summary>
        /// <param name="リソースパス">ファイルパスまたはリソースパス。</param>
        /// <returns></returns>
        private void _AddVertexDataToTheVertexLayoutList( PMXFormat.Vertex VertexData, List<CS_INPUT> VertexLayoutList )
        {
            var layout = new CS_INPUT() {
                Position = new Vector4( VertexData.Position, 1f ),
                Normal = VertexData.Normal,
                UV = VertexData.UV,
                Index = (uint) VertexLayoutList.Count,    // 現在の要素数 ＝ List<>内でのこの要素のインデックス番号
                EdgeWeight = VertexData.EdgeMagnification,
                DeformationMethod = (uint) VertexData.WeightDeformationMethod,
            };

            switch( VertexData.WeightDeformationMethod )
            {
                case PMXFormat.BoneWeightType.BDEF1:
                    {
                        var v = (PMXFormat.BDEF1) VertexData.BoneWeight;
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

                case PMXFormat.BoneWeightType.BDEF2:
                    {
                        var v = (PMXFormat.BDEF2) VertexData.BoneWeight;
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

                case PMXFormat.BoneWeightType.SDEF:
                    {
                        var v = (PMXFormat.SDEF) VertexData.BoneWeight;
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

                case PMXFormat.BoneWeightType.BDEF4:
                case PMXFormat.BoneWeightType.QDEF:
                    {
                        var v = (PMXFormat.BDEF4) VertexData.BoneWeight;
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

            VertexLayoutList.Add( ( layout ) );
        }
    }
}
