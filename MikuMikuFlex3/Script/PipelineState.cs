using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace MikuMikuFlex3.Script
{
    /// <summary>
    ///     パイプラインステート。
    ///     スクリプトに渡されるホストオブジェクト。
    /// </summary>
    public class PipelineState : IDisposable
    {

        // スクリプト（Initialize）向け


        public void CreateVertexShader( object key, string csoFilePath )
        {
            this.RemoveVetexShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._VertexShaderes[ key ] = new VertexShader( this._d3dDevice, b ) );
        }

        public void CreateHullShader( object key, string csoFilePath )
        {
            this.RemoveHullShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._HullShaderes[ key ] = new HullShader( this._d3dDevice, b ) );
        }

        public void CreateDomainShader( object key, string csoFilePath )
        {
            this.RemoveDomainShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._DomainShaderes[ key ] = new DomainShader( this._d3dDevice, b ) );
        }

        public void CreateGeometryShader( object key, string csoFilePath )
        {
            this.RemoveGeometryShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._GeometryShaderes[ key ] = new GeometryShader( this._d3dDevice, b ) );
        }

        public void CreatePixelShader( object key, string csoFilePath )
        {
            this.RemovePixelShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._PixelShaderes[ key ] = new PixelShader( this._d3dDevice, b ) );
        }

        public void CreateComputeShader( object key, string csoFilePath )
        {
            this.RemoveComputeShader( key );

            this._CreateShader( csoFilePath, ( b ) => this._ComputeShaderes[ key ] = new ComputeShader( this._d3dDevice, b ) );
        }


        public void CreateVertexShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemoveVetexShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "vs_5_0", ( b ) => this._VertexShaderes[ key ] = new VertexShader( this._d3dDevice, b ) );
        }

        public void CreateHullShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemoveHullShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "hs_5_0", ( b ) => this._HullShaderes[ key ] = new HullShader( this._d3dDevice, b ) );
        }

        public void CreateDomainShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemoveDomainShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "ds_5_0", ( b ) => this._DomainShaderes[ key ] = new DomainShader( this._d3dDevice, b ) );
        }

        public void CreateGeometryShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemoveGeometryShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "gs_5_0", ( b ) => this._GeometryShaderes[ key ] = new GeometryShader( this._d3dDevice, b ) );
        }

        public void CreatePixelShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemovePixelShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "ps_5_0", ( b ) => this._PixelShaderes[ key ] = new PixelShader( this._d3dDevice, b ) );
        }

        public void CreateComputeShaderFromHLSL( object key, string hlslFilePath )
        {
            this.RemoveComputeShader( key );

            this._CreateShaderFromHLSL( hlslFilePath, "cs_5_0", ( b ) => this._ComputeShaderes[ key ] = new ComputeShader( this._d3dDevice, b ) );
        }


        public void RemoveVetexShader( object key )
        {
            if( this._VertexShaderes.ContainsKey( key ) )
            {
                this._VertexShaderes[ key ]?.Dispose();
                this._VertexShaderes.Remove( key );
            }
        }

        public void RemoveHullShader( object key )
        {
            if( this._HullShaderes.ContainsKey( key ) )
            {
                this._HullShaderes[ key ]?.Dispose();
                this._HullShaderes.Remove( key );
            }
        }

        public void RemoveDomainShader( object key )
        {
            if( this._DomainShaderes.ContainsKey( key ) )
            {
                this._DomainShaderes[ key ]?.Dispose();
                this._DomainShaderes.Remove( key );
            }
        }

        public void RemoveGeometryShader( object key )
        {
            if( this._GeometryShaderes.ContainsKey( key ) )
            {
                this._GeometryShaderes[ key ]?.Dispose();
                this._GeometryShaderes.Remove( key );
            }
        }

        public void RemovePixelShader( object key )
        {
            if( this._PixelShaderes.ContainsKey( key ) )
            {
                this._PixelShaderes[ key ]?.Dispose();
                this._PixelShaderes.Remove( key );
            }
        }

        public void RemoveComputeShader( object key )
        {
            if( this._ComputeShaderes.ContainsKey( key ) )
            {
                this._ComputeShaderes[ key ]?.Dispose();
                this._ComputeShaderes.Remove( key );
            }
        }


        public void CreateTexture2D( object key, string imageFilePath )
        {
            this.RemoveTexture2D( key );

            try
            {
                var srv = MikuMikuFlex3.Utility.MMFShaderResourceView.FromFile( this._d3dDevice, imageFilePath, out Texture2D tex2d );
                this._PrivateTextureList[ key ] = (tex2d, srv);
            }
            catch( Exception e )
            {
                Trace.WriteLine( $"TextureGenerationFailed。[{imageFilePath}][{e.Message}]" );
            }
        }

        public void RemoveTexture2D( object key )
        {
            if( this._PrivateTextureList.ContainsKey( key ) )
            {
                this._PrivateTextureList[ key ].tex?.Dispose();
                this._PrivateTextureList[ key ].srv?.Dispose();
                this._PrivateTextureList.Remove( key );
            }
        }



        // スクリプト（Run）向け


        public MMDPass MMDPass;

        public void SetVertexShader( object key )
        {
            this._SelectedVertexShader = key;
        }

        public void SetHullShader( object key )
        {
            this._SelectedHullShader = key;
        }

        public void SetDomainShader( object key )
        {
            this._SelectedDomainShader = key;
        }

        public void SetGeometryShader( object key )
        {
            this._SelectedGeometryShader = key;
        }

        public void SetPixelShader( object key )
        {
            this._SelectedPixelShader = key;
        }

        public void SetComputeShader( object key )
        {
            this._SelectedComputeShader = key;
        }

        public Size2 GetViewportSize()
        {
            var vs = this._d3ddc.Rasterizer.GetViewports<ViewportF>();

            return new Size2( (int) vs[ 0 ].Width, (int) vs[ 0 ].Height );
        }

        public Size2F GetViewportSize2F()
        {
            var vs = this._d3ddc.Rasterizer.GetViewports<ViewportF>();

            return new Size2F( vs[ 0 ].Width, vs[ 0 ].Height );
        }


        // 材質エフェクト用
        public void Draw()
        {
            // 選択されたシェーダーが既定値(null) ではなかったら設定する。

            if( null != this._SelectedVertexShader && this._VertexShaderes.ContainsKey( this._SelectedVertexShader ) )
                this._d3ddc.VertexShader.Set( this._VertexShaderes[ this._SelectedVertexShader ] );

            if( null != this._SelectedHullShader && this._HullShaderes.ContainsKey( this._SelectedHullShader ) )
                this._d3ddc.HullShader.Set( this._HullShaderes[ this._SelectedHullShader ] );

            if( null != this._SelectedDomainShader && this._DomainShaderes.ContainsKey( this._SelectedDomainShader ) )
                this._d3ddc.DomainShader.Set( this._DomainShaderes[ this._SelectedDomainShader ] );

            if( null != this._SelectedGeometryShader && this._GeometryShaderes.ContainsKey( this._SelectedGeometryShader ) )
                this._d3ddc.GeometryShader.Set( this._GeometryShaderes[ this._SelectedGeometryShader ] );

            if( null != this._SelectedPixelShader && this._PixelShaderes.ContainsKey( this._SelectedPixelShader ) )
                this._d3ddc.PixelShader.Set( this._PixelShaderes[ this._SelectedPixelShader ] );


            // Draw。

            this._d3ddc.DrawIndexed( this._NumberOfVertices, this._StartIndexOfVertices, 0 );
        }

        // ポストエフェクト用
        public void Blit( int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ )
        {
            // 選択されたシェーダーが既定値(null) ではなかったら設定する。

            if( null != this._SelectedComputeShader && this._ComputeShaderes.ContainsKey( this._SelectedComputeShader ) )
                this._d3ddc.ComputeShader.Set( this._ComputeShaderes[ this._SelectedComputeShader ] );


            // 実行する。

            this._d3ddc.Dispatch( threadGroupCountX, threadGroupCountY, threadGroupCountZ );
        }



        // 内部向け


        public Reason Reason = Reason.Initialize;


        public PipelineState( Device d3dDevice )
        {
            this._d3dDevice = d3dDevice;
            this._DefaultMaterialShader = new DefaultMaterialShader( d3dDevice );
        }

        public virtual void Dispose()
        {
            foreach( var kvp in this._VertexShaderes )
                kvp.Value.Dispose();

            foreach( var kvp in this._HullShaderes )
                kvp.Value.Dispose();

            foreach( var kvp in this._DomainShaderes )
                kvp.Value.Dispose();

            foreach( var kvp in this._GeometryShaderes )
                kvp.Value.Dispose();

            foreach( var kvp in this._PixelShaderes )
                kvp.Value.Dispose();

            foreach( var kvp in this._PrivateTextureList )
            {
                kvp.Value.tex.Dispose();
                kvp.Value.srv.Dispose();
            }

            this._DefaultMaterialShader?.Dispose();
            this._d3ddc = null;     // Disposeしない
            this._d3dDevice = null; // Disposeしない
        }

        public void ResetDrawState( int NumberOfVertices, int StartIndexOfVertices, MMDPass passType, DeviceContext d3ddc )
        {
            this._NumberOfVertices = NumberOfVertices;
            this._StartIndexOfVertices = StartIndexOfVertices;
            this.MMDPass = passType;
            this._d3ddc = d3ddc;

            // 既定のパイプラインステートを設定。

            switch( passType )
            {
                case MMDPass.Edge:
                    this._d3ddc.VertexShader.Set( this._DefaultMaterialShader.VertexShaderForEdge );    // Edge
                    this._d3ddc.HullShader.Set( this._DefaultMaterialShader.HullShader );
                    this._d3ddc.DomainShader.Set( this._DefaultMaterialShader.DomainShader );
                    this._d3ddc.GeometryShader.Set( this._DefaultMaterialShader.GeometryShader );
                    this._d3ddc.PixelShader.Set( this._DefaultMaterialShader.PixelShaderForEdge );      // Edge
                    break;

                default:
                    this._d3ddc.VertexShader.Set( this._DefaultMaterialShader.VertexShaderForObject );
                    this._d3ddc.HullShader.Set( this._DefaultMaterialShader.HullShader );
                    this._d3ddc.DomainShader.Set( this._DefaultMaterialShader.DomainShader );
                    this._d3ddc.GeometryShader.Set( this._DefaultMaterialShader.GeometryShader );
                    this._d3ddc.PixelShader.Set( this._DefaultMaterialShader.PixelShaderForObject );
                    break;
            }

            this._SelectedVertexShader = null;
            this._SelectedHullShader = null;
            this._SelectedDomainShader = null;
            this._SelectedGeometryShader = null;
            this._SelectedPixelShader = null;
        }

        public void ResetBlitState( DeviceContext d3ddc )
        {
            this._d3ddc = d3ddc;
            this._d3ddc.ComputeShader.Set( null );
            this._SelectedComputeShader = null;
        }


        protected int _NumberOfVertices;

        protected int _StartIndexOfVertices;

        protected DeviceContext _d3ddc;

        protected Device _d3dDevice;

        protected DefaultMaterialShader _DefaultMaterialShader;


        protected Dictionary<object, VertexShader> _VertexShaderes = new Dictionary<object, VertexShader>();

        protected Dictionary<object, HullShader> _HullShaderes = new Dictionary<object, HullShader>();

        protected Dictionary<object, DomainShader> _DomainShaderes = new Dictionary<object, DomainShader>();

        protected Dictionary<object, GeometryShader> _GeometryShaderes = new Dictionary<object, GeometryShader>();

        protected Dictionary<object, PixelShader> _PixelShaderes = new Dictionary<object, PixelShader>();

        protected Dictionary<object, ComputeShader> _ComputeShaderes = new Dictionary<object, ComputeShader>();

        protected object _SelectedVertexShader = null;

        protected object _SelectedHullShader = null;

        protected object _SelectedDomainShader = null;

        protected object _SelectedGeometryShader = null;

        protected object _SelectedPixelShader = null;

        protected object _SelectedComputeShader = null;

        protected Dictionary<object, (Texture2D tex, ShaderResourceView srv)> _PrivateTextureList = new Dictionary<object, (Texture2D tex, ShaderResourceView srv)>();


        protected void _CreateShader( string csoFilePath, Action<byte[]> create )
        {
            try
            {
                using( var fs = new FileStream( csoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );
                    create( buffer );
                }
            }
            catch( Exception e )
            {
                Trace.TraceError( $"ファイルからのシェーダーの作成に失敗しました。[{csoFilePath}][{e.Message}]" );
            }
        }

        protected void _CreateShaderFromHLSL( string hlslFilePath, string profile, Action<byte[]> create )
        {
            try
            {
                using( var fs = new FileStream( hlslFilePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                {
                    var buffer = new byte[ fs.Length ];
                    fs.Read( buffer, 0, buffer.Length );

                    ShaderFlags flags = ShaderFlags.None;
#if DEBUG
                    flags |= ShaderFlags.Debug;
                    flags |= ShaderFlags.SkipOptimization;
                    flags |= ShaderFlags.EnableBackwardsCompatibility;
#endif
                    var compileResult = ShaderBytecode.Compile( buffer, "main", profile, flags );

                    if( compileResult?.Bytecode == null )
                        throw new Exception( "このHLSLDoesItSupportFiles?、AnErrorHasOccurred。" );

                    create( compileResult.Bytecode );
                }
            }
            catch( Exception e )
            {
                Trace.TraceError( $"ファイルからのシェーダーの作成に失敗しました。[{hlslFilePath}][{e.Message}]" );
            }
        }
    }
}
