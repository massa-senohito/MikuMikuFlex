using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    public class PMXVertexControl
    {
        public CS_INPUT[] InputVertexArray;

        public bool[] UnitUpdateFlag;

        public const int NumberOfVerticesForUnitUpdate = 1500;
        


        // 生成と終了


        public PMXVertexControl( CS_INPUT[] InitialArray )
        {
            this.InputVertexArray = InitialArray;

            int NumberOfUnits = InitialArray.Length / NumberOfVerticesForUnitUpdate + 1;

            this.UnitUpdateFlag = new bool[ NumberOfUnits ];
            this._UnitUpdateFlagStatus = new UnitUpdateFlagStatus[ NumberOfUnits ];

            for( int i = 0; i < this.UnitUpdateFlag.Length; i++ )
            {
                this.UnitUpdateFlag[ i ] = false;
                this._UnitUpdateFlagStatus[ i ] = UnitUpdateFlagStatus.NoChange;
            }
        }



        // 更新


        public void ResetState( int AddToUVNumber, PMXFormat.VertexList InitialList )
        {
            // 移動された頂点について、状態を初期化する。

            foreach( int i in this._IndexSetOfModifiedVertices )
            {
                var iv = InitialList[ i ];

                this.InputVertexArray[ i ].Position.X = iv.Position.X;
                this.InputVertexArray[ i ].Position.Y = iv.Position.Y;
                this.InputVertexArray[ i ].Position.Z = iv.Position.Z;
                this.InputVertexArray[ i ].Position.W = 1f;
                this.InputVertexArray[ i ].UV = iv.UV;
                switch( AddToUVNumber )
                {
                    case 0:
                        break;

                    case 1:
                        this.InputVertexArray[ i ].AddUV1 = iv.AddToUV[ 0 ];
                        break;

                    case 2:
                        this.InputVertexArray[ i ].AddUV1 = iv.AddToUV[ 0 ];
                        this.InputVertexArray[ i ].AddUV2 = iv.AddToUV[ 1 ];
                        break;

                    case 3:
                        this.InputVertexArray[ i ].AddUV1 = iv.AddToUV[ 0 ];
                        this.InputVertexArray[ i ].AddUV2 = iv.AddToUV[ 1 ];
                        this.InputVertexArray[ i ].AddUV3 = iv.AddToUV[ 2 ];
                        break;

                    case 4:
                        this.InputVertexArray[ i ].AddUV1 = iv.AddToUV[ 0 ];
                        this.InputVertexArray[ i ].AddUV2 = iv.AddToUV[ 1 ];
                        this.InputVertexArray[ i ].AddUV3 = iv.AddToUV[ 2 ];
                        this.InputVertexArray[ i ].AddUV4 = iv.AddToUV[ 3 ];
                        break;
                }
            }

            this._IndexSetOfModifiedVertices.Clear();


            // フラグをローテーションする。
            //   NoChange/false   → NoChange/false
            //   ThereIsAChange/true    → WithInitialization/true
            //   初期かあり/true  → NoChange/false

            for( int i = 0; i < this._UnitUpdateFlagStatus.Length; i++ )
            {
                if( this._UnitUpdateFlagStatus[ i ] == UnitUpdateFlagStatus.ThereIsAChange )
                {
                    this._UnitUpdateFlagStatus[ i ] = UnitUpdateFlagStatus.WithInitialization;
                }
                else if( this._UnitUpdateFlagStatus[ i ] == UnitUpdateFlagStatus.WithInitialization )
                {
                    this._UnitUpdateFlagStatus[ i ] = UnitUpdateFlagStatus.NoChange;
                    this.UnitUpdateFlag[ i ] = false;
                }
            }
        }

        public void NotifyChangesOfVertices( int VertexIndex )
        {
            this._IndexSetOfModifiedVertices.Add( VertexIndex );

            int UnitIndex = VertexIndex / NumberOfVerticesForUnitUpdate;

            this.UnitUpdateFlag[ UnitIndex ] = true;
            this._UnitUpdateFlagStatus[ UnitIndex ] = UnitUpdateFlagStatus.ThereIsAChange;
        }



        // private


        /// <summary>
        ///     すべての頂点を初期化するには数が多いので、移動された頂点を記録しておいて、
        ///     記録された頂点についてのみ初期化するようにする。
        /// </summary>
        private List<int> _IndexSetOfModifiedVertices = new List<int>();

        private enum UnitUpdateFlagStatus
        {                 // 更新フラグが:
            NoChange,     // 前回 false      → 今回 false
            ThereIsAChange,     // 前回 false/true → 今回 true
            WithInitialization,   // 前回 true       → 今回 false
        }
        private UnitUpdateFlagStatus[] _UnitUpdateFlagStatus;
    }
}
