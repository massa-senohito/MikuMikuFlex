using System;
using SharpDX;

namespace MikuMikuFlex
{
    /// <summary>
    ///     描画可能リソース
    /// </summary>
    public interface IDrawable : IDisposable
    {
        bool 表示中 { get; set; }

        string ファイル名 { get; }

        int サブセット数 { get; }

        int 頂点数 { get; }

        Vector4 セルフシャドウ色 { get; set; }

        Vector4 地面影色 { get; set; }

        /// <summary>
        ///     モデルを動かす際に使用するクラス
        /// </summary>
        モデル状態 モデル状態 { get; }

        
        void 描画する();

        void 更新する();
    }
}