using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.Material"/> に追加情報を付与するクラス。
    /// </summary>
    public class PMXMaterialControl : IDisposable
    {
        public string GivenNames => this._PMXFMaterial.MaterialName;

        public float TessellationCoefficient { get; set; } = 1f;


        public class Status
        {
            /// <summary>
            ///     (R, G, B, A)
            /// </summary>
            public Vector4 DiffuseColor { get; set; }

            /// <summary>
            ///     (R, G, B)
            /// </summary>
            public Vector3 ReflectiveColor { get; set; }

            public float ReflectionIntensity { get; set; }

            /// <summary>
            ///     (R, G, B)
            /// </summary>
            public Vector3 EnvironmentalColor { get; set; }

            /// <summary>
            ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
            ///     (R, G, B, A)
            /// </summary>
            public Vector4 EdgeColor { get; set; }

            /// <summary>
            ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
            ///     Point 描画時は Point Size(※2.1拡張)。
            /// </summary>
            public float EdgeSize { get; set; }
        }

        public Status AdditionDifference { get; protected set; }

        public Status MultiplyDifference { get; protected set; }



        // 現在の状態（加算と乗算でアニメーション可能なもの）


        /// <summary>
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 DiffuseColor => MulEachMember( this._PMXFMaterial.DiffuseColor, this.MultiplyDifference.DiffuseColor ) + AdditionDifference.DiffuseColor;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 ReflectiveColor => MulEachMember( this._PMXFMaterial.ReflectiveColor, this.MultiplyDifference.ReflectiveColor ) + AdditionDifference.ReflectiveColor;

        public float ReflectionIntensity => this._PMXFMaterial.ReflectionIntensity * this.MultiplyDifference.ReflectionIntensity + this.AdditionDifference.ReflectionIntensity;

        /// <summary>
        ///     (R, G, B)
        /// </summary>
        public Vector3 EnvironmentalColor => MulEachMember( this._PMXFMaterial.EnvironmentalColor, this.MultiplyDifference.EnvironmentalColor ) + AdditionDifference.EnvironmentalColor;

        /// <summary>
        ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
        ///     (R, G, B, A)
        /// </summary>
        public Vector4 EdgeColor => ( this.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.Edge ) ) ? MulEachMember( this._PMXFMaterial.EdgeColor, this.MultiplyDifference.EdgeColor ) + AdditionDifference.EdgeColor : new Vector4( 0f );

        /// <summary>
        ///     <see cref="DrawingFlag.Edge"/> が指定されているときのみ有効。
        ///     Point 描画時は Point Size(※2.1拡張)。
        /// </summary>
        public float EdgeSize => ( this.DrawingFlag.HasFlag( PMXFormat.DrawingFlag.Edge ) ) ? this._PMXFMaterial.EdgeSize * this.MultiplyDifference.EdgeSize + AdditionDifference.EdgeSize : 0f;



        // Drawing


        public PMXFormat.DrawingFlag DrawingFlag => this._PMXFMaterial.DrawingFlag;

        /// <summary>
        ///     材質の描画をおこなうシェーダー。
        ///     null なら既定のシェーダーが利用される。
        /// </summary>
        public IMaterialShader MaterialDrawingShader { get; set; }



        // Texture／Note


        public int ReferenceIndexOfNormalTexture => this._PMXFMaterial.ReferenceIndexOfNormalTexture;

        public int SphereTextureReferenceIndex => this._PMXFMaterial.SphereTextureReferenceIndex;

        public PMXFormat.SphereMode SphereMode => this._PMXFMaterial.SphereMode;

        /// <summary>
        ///     0 or 1  。
        ///     <see cref="ShareToonのテクスチャ参照インデックス"/> のサマリを参照のこと。
        /// </summary>
        public byte ShareToonFlag => this._PMXFMaterial.ShareToonFlag;

        /// <summary>
        ///     <see cref="ShareToonFlag"/> が 0 の時は、Toonテクスチャテクスチャテーブルの参照インデックス。
        ///     <see cref="ShareToonFlag"/> が 1 の時は、ShareToonTexture[0~9]がそれぞれ toon01.bmp~toon10.bmp に対応。
        /// </summary>
        public int ShareToonのテクスチャ参照インデックス => this._PMXFMaterial.ShareToonのテクスチャ参照インデックス;

        /// <summary>
        ///     自由欄／スクリプト記述／エフェクトへのパラメータ配置など
        /// </summary>
        public String Note => this._PMXFMaterial.Note;

        /// <summary>
        ///     材質に対応する面数（頂点数で示す）。
        ///     １面は３頂点なので、必ず３の倍数になる。
        /// </summary>
        public int NumberOfVertices => this._PMXFMaterial.NumberOfVertices;

        public int StartingIndex => this._PMXFMaterial.StartingIndex;



        // 生成と終了


        public PMXMaterialControl( PMXFormat.Material material, IMaterialShader MaterialDrawingShader )
        {
            this._PMXFMaterial = material;
            this.MaterialDrawingShader = MaterialDrawingShader;

            this.AdditionDifference = new Status();
            this.MultiplyDifference = new Status();

            this.ResetState();
        }

        public virtual void Dispose()
        {
            this._PMXFMaterial = null;
        }



        // 更新


        public void ResetState()
        {
            this.AdditionDifference.DiffuseColor = Vector4.Zero;
            this.AdditionDifference.ReflectiveColor = Vector3.Zero;
            this.AdditionDifference.ReflectionIntensity = 0;
            this.AdditionDifference.EnvironmentalColor = Vector3.Zero;
            this.AdditionDifference.EdgeColor = Vector4.Zero;
            this.AdditionDifference.EdgeSize = 0;

            this.MultiplyDifference.DiffuseColor = new Vector4( 1f );
            this.MultiplyDifference.ReflectiveColor = new Vector3( 1f );
            this.MultiplyDifference.ReflectionIntensity = 1;
            this.MultiplyDifference.EnvironmentalColor = new Vector3( 1f );
            this.MultiplyDifference.EdgeColor = new Vector4( 1f );
            this.MultiplyDifference.EdgeSize = 1;
        }



        // private


        private PMXFormat.Material _PMXFMaterial;


        public static Vector4 MulEachMember( Vector4 vec1, Vector4 vec2 )
            => new Vector4( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z, vec1.W * vec2.W );

        public static Vector3 MulEachMember( Vector3 vec1, Vector3 vec2 )
            => new Vector3( vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z );
    }
}
