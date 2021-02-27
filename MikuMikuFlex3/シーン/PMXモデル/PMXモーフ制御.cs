using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.Morph"/> に追加情報を付与するクラス。
    /// </summary>
    public class PMXMorphControl : IDisposable
    {

        // 基本情報


        public string GivenNames => this.PMXFMorph.MorphName;

        public PMXFormat.MorphType MorphType => this.PMXFMorph.MorphType;

        public PMXFormat.Morph PMXFMorph { get; protected set; }

        public float MorphValue { get; set; }

        public AnimeVariables<float> AnimeVariables_Morph;



        // 生成と終了


        public PMXMorphControl( PMXFormat.Morph morph )
        {
            this.PMXFMorph = morph;
            this.MorphValue = 0;
            this.AnimeVariables_Morph = new AnimeVariables<float>( 0f );
        }

        public virtual void Dispose()
        {
            this.PMXFMorph = null;
        }



        // 更新


        internal void ApplyMorphs( double CurrentTimesec, PMXModel PMXModel )
        {
            var PresentValue = this.AnimeVariables_Morph.Update( CurrentTimesec );

            this._ApplyMorphs( PresentValue, PMXModel, this );
        }

        private void _ApplyMorphs( float PresentValue, PMXModel PMXModel, PMXMorphControl ApplicableMorph )
        {
            switch( ApplicableMorph.PMXFMorph.MorphType )
            {
                case PMXFormat.MorphType.Vertex:
                    #region " VertexMorph "
                    //----------------
                    {
                        foreach( PMXFormat.VertexMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].Position.X += offset.CoordinateOffsetAmount.X * PresentValue;
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].Position.Y += offset.CoordinateOffsetAmount.Y * PresentValue;
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].Position.Z += offset.CoordinateOffsetAmount.Z * PresentValue;

                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.UV:
                    #region " UVMorph "
                    //----------------
                    {
                        foreach( PMXFormat.UVMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].UV += new Vector2( offset.UVOffsetAmount.X, offset.UVOffsetAmount.Y ) * PresentValue;
                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.AddToUV1:
                    #region " AddToUV1Morph "
                    //----------------
                    {
                        foreach( PMXFormat.UVMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].AddUV1 += offset.UVOffsetAmount * PresentValue;
                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.AddToUV2:
                    #region " AddToUV2Morph "
                    //----------------
                    {
                        foreach( PMXFormat.UVMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].AddUV2 += offset.UVOffsetAmount * PresentValue;
                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.AddToUV3:
                    #region " AddToUV3Morph "
                    //----------------
                    {
                        foreach( PMXFormat.UVMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].AddUV3 += offset.UVOffsetAmount * PresentValue;
                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.AddToUV4:
                    #region " AddToUV4Morph "
                    //----------------
                    {
                        foreach( PMXFormat.UVMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            PMXModel.PMXVertexControl.InputVertexArray[ offset.VertexIndex ].AddUV4 += offset.UVOffsetAmount * PresentValue;
                            PMXModel.PMXVertexControl.NotifyChangesOfVertices( (int) offset.VertexIndex );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.Bourne:
                    #region " BoneMorph "
                    //----------------
                    {
                        foreach( PMXFormat.BoneMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            var bone = PMXModel.BoneList[ offset.BoneIndex ];

                            bone.Move += offset.AmountOfMovement * PresentValue;
                            bone.Rotation *= new Quaternion(
                                offset.RotationAmount.X * PresentValue,
                                offset.RotationAmount.Y * PresentValue,
                                offset.RotationAmount.Z * PresentValue,
                                offset.RotationAmount.W * PresentValue );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.Material:
                    #region " MaterialMorph "
                    //----------------
                    {
                        // todo: 材質モーフ・テクスチャ係数への対応
                        // todo: 材質モーフ・スフィアテクスチャ係数への対応
                        // todo: 材質モーフ・Toonテクスチャ係数への対応

                        foreach( PMXFormat.MaterialMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            if( offset.MaterialIndex == -1 ) // -1:全材質が対象
                            {
                                foreach( var Material in PMXModel.MaterialList )
                                    DifferenceSet( offset, Material );
                            }
                            else
                            {
                                var Material = PMXModel.MaterialList[ offset.MaterialIndex ];

                                DifferenceSet( offset, Material );
                            }
                        }


                        void DifferenceSet( PMXFormat.MaterialMorphOffset offset, PMXMaterialControl Material )
                        {
                            switch( offset.OffsetCalculationFormat )
                            {
                                case 0: // Multiply
                                    Material.MultiplyDifference.DiffuseColor += offset.DiffuseColor * PresentValue;
                                    Material.MultiplyDifference.ReflectiveColor += offset.ReflectiveColor * PresentValue;
                                    Material.MultiplyDifference.ReflectionIntensity += offset.ReflectionIntensity * PresentValue;
                                    Material.MultiplyDifference.EnvironmentalColor += offset.EnvironmentalColor * PresentValue;
                                    Material.MultiplyDifference.EdgeColor += offset.EdgeColor * PresentValue;
                                    Material.MultiplyDifference.EdgeSize += offset.EdgeSize * PresentValue;
                                    break;

                                case 1: // Addition
                                    Material.AdditionDifference.DiffuseColor += offset.DiffuseColor * PresentValue;
                                    Material.AdditionDifference.ReflectiveColor += offset.ReflectiveColor * PresentValue;
                                    Material.AdditionDifference.ReflectionIntensity += offset.ReflectionIntensity * PresentValue;
                                    Material.AdditionDifference.EnvironmentalColor += offset.EnvironmentalColor * PresentValue;
                                    Material.AdditionDifference.EdgeColor += offset.EdgeColor * PresentValue;
                                    Material.AdditionDifference.EdgeSize += offset.EdgeSize * PresentValue;
                                    break;
                            }
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.Group:
                    #region " GroupMorph "
                    //----------------
                    {
                        foreach( PMXFormat.GroupMorphOffset offset in ApplicableMorph.PMXFMorph.MorphOffsetList )
                        {
                            var MemberMorph = PMXModel.MorphList[ offset.MorphIndex ];

                            if( MemberMorph.PMXFMorph.MorphType == PMXFormat.MorphType.Group )
                                throw new InvalidOperationException( "GroupMorphIsSpecifiedAsAGroupOfGroupMorph。" );

                            this._ApplyMorphs( PresentValue * offset.Impact, PMXModel, MemberMorph );
                        }
                    }
                    //----------------
                    #endregion
                    break;

                case PMXFormat.MorphType.Flip:
                    // todo: フリップモーフの実装
                    break;

                case PMXFormat.MorphType.Impulse:
                    // todo: インパルスモーフの実装
                    break;
            }
        }
    }
}
