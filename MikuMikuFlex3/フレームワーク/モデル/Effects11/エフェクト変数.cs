using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    class エフェクト変数 : IDisposable
    {
        public EffectVectorVariable EDGECOLOR { get; protected set; }

        public EffectScalarVariable EDGEWIDTH { get; protected set; }

        public EffectMatrixVariable WORLDVIEWPROJECTION { get; protected set; }

        public EffectMatrixVariable WORLDVIEW { get; protected set; }

        public EffectMatrixVariable WORLD { get; protected set; }

        public EffectMatrixVariable VIEW { get; protected set; }

        public EffectMatrixVariable VIEWPROJECTION { get; protected set; }

        public EffectVectorVariable VIEWPORTPIXELSIZE { get; protected set; }

        public EffectVectorVariable POSITION_camera { get; protected set; }

        public EffectVectorVariable POSITION_light { get; protected set; }

        public EffectShaderResourceVariable MATERIALTEXTURE { get; protected set; }

        public EffectShaderResourceVariable MATERIALSPHEREMAP { get; protected set; }

        public EffectShaderResourceVariable MATERIALTOONTEXTURE { get; protected set; }

        public EffectScalarVariable TESSFACTOR { get; protected set; }

        public EffectScalarVariable use_spheremap { get; protected set; }

        public EffectScalarVariable spadd { get; protected set; }

        public EffectScalarVariable use_texture { get; protected set; }

        public EffectScalarVariable use_toontexturemap { get; protected set; }

        public EffectScalarVariable use_selfshadow { get; protected set; }

        public EffectVectorVariable ambientcolor { get; protected set; }

        public EffectVectorVariable diffusecolor { get; protected set; }

        public EffectVectorVariable specularcolor { get; protected set; }

        public EffectScalarVariable specularpower { get; protected set; }


        public エフェクト変数( Effect effect )
        {
            for( int j = 0; j < effect.Description.GlobalVariableCount; j++ )
            {
                var 変数 = effect.GetVariableByIndex( j );

                switch( 変数.Description.Semantic?.ToUpper() )
                {
                    case "EDGECOLOR":
                        this.EDGECOLOR = 変数.AsVector();
                        break;

                    case "EDGEWIDTH":
                        this.EDGEWIDTH = 変数.AsScalar();
                        break;

                    case "WORLDVIEWPROJECTION":
                        this.WORLDVIEWPROJECTION = 変数.AsMatrix();
                        break;

                    case "WORLDVIEW":
                        this.WORLDVIEW = 変数.AsMatrix();
                        break;

                    case "WORLD":
                        this.WORLD = 変数.AsMatrix();
                        break;

                    case "VIEW":
                        this.VIEW = 変数.AsMatrix();
                        break;

                    case "VIEWPROJECTION":
                        this.VIEWPROJECTION = 変数.AsMatrix();
                        break;

                    case "VIEWPORTPIXELSIZE":
                        this.VIEWPORTPIXELSIZE = 変数.AsVector();
                        break;

                    case "POSITION":
                        switch( 変数.GetAnnotationByName( "object" ).AsString().GetString().ToLower() )
                        {
                            case "camera":
                                this.POSITION_camera = 変数.AsVector();
                                break;

                            case "light":
                                this.POSITION_light = 変数.AsVector();
                                break;
                        }
                        break;

                    case "MATERIALTEXTURE":
                        this.MATERIALTEXTURE = 変数.AsShaderResource();
                        break;

                    case "MATERIALSPHEREMAP":
                        this.MATERIALSPHEREMAP = 変数.AsShaderResource();
                        break;

                    case "MATERIALTOONTEXTURE":
                        this.MATERIALTOONTEXTURE = 変数.AsShaderResource();
                        break;

                    case "TESSFACTOR":
                        this.TESSFACTOR = 変数.AsScalar();
                        break;

                    default:
                        switch( 変数.Description.Name.ToLower() )
                        {
                            case "use_spheremap":
                                this.use_spheremap = 変数.AsScalar();
                                break;

                            case "spadd":
                                this.spadd = 変数.AsScalar();
                                break;

                            case "use_texture":
                                this.use_texture = 変数.AsScalar();
                                break;

                            case "use_toontexturemap":
                                this.use_toontexturemap = 変数.AsScalar();
                                break;

                            case "use_selfshadow":
                                this.use_selfshadow = 変数.AsScalar();
                                break;

                            case "ambientcolor":
                                this.ambientcolor = 変数.AsVector();
                                break;

                            case "diffusecolor":
                                this.diffusecolor = 変数.AsVector();
                                break;

                            case "specularcolor":
                                this.specularcolor = 変数.AsVector();
                                break;

                            case "specularpower":
                                this.specularpower = 変数.AsScalar();
                                break;
                        }
                        break;
                }
            }
        }

        public virtual void Dispose()
        {
            this.EDGECOLOR?.Dispose();
            this.EDGEWIDTH?.Dispose();
            this.WORLDVIEWPROJECTION?.Dispose();
            this.WORLDVIEW?.Dispose();
            this.WORLD?.Dispose();
            this.VIEW?.Dispose();
            this.VIEWPROJECTION?.Dispose();
            this.VIEWPORTPIXELSIZE?.Dispose();
            this.POSITION_camera?.Dispose();
            this.POSITION_light?.Dispose();
            this.MATERIALTEXTURE?.Dispose();
            this.MATERIALSPHEREMAP?.Dispose();
            this.MATERIALTOONTEXTURE?.Dispose();
            this.TESSFACTOR?.Dispose();
            this.use_spheremap?.Dispose();
            this.spadd?.Dispose();
            this.use_texture?.Dispose();
            this.use_toontexturemap?.Dispose();
            this.use_selfshadow?.Dispose();
            this.ambientcolor?.Dispose();
            this.diffusecolor?.Dispose();
            this.specularcolor?.Dispose();
            this.specularpower?.Dispose();
        }
    }
}
