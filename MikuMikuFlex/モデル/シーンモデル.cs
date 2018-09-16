using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex
{
    /// <summary>
    ///     レンダーターゲットと同じサイズの四角形を描画する。
    ///     シーン用エフェクトを割り当てて使う前提。
    /// </summary>
    public class シーンモデル : IDrawable
    {
        public bool 表示中 { get; set; }

        public string ファイル名 { get; set; }

        public シーン用エフェクト管理 シーン用エフェクト管理 { get; protected set; }

        public サブリソースローダー サブリソースローダー { get; protected set; }

        public モデル状態 モデル状態 { get; protected set; }

        // 未使用
        public int サブセット数 => 0;

        // 未使用
        public int 頂点数 => 4;

        // 未使用
        Vector4 IDrawable.セルフシャドウ色
        {
            get => Vector4.One;
            set => throw new NotImplementedException();
        }

        // 未使用
        Vector4 IDrawable.地面影色
        {
            get => Vector4.One;
            set => throw new NotImplementedException();
        }


        public シーンモデル( string 名前, string エフェクトファイルパス, ScreenContext このシーンを使うScreenContext, サブリソースローダー loader = null )
        {
            this.表示中 = true;
            this.ファイル名 = 名前;
            this.シーン用エフェクト管理 = new シーン用エフェクト管理();
            this.モデル状態 = new Transformer基本実装();
            this._このシーンを利用するScreenContext = new WeakReference<ScreenContext>( このシーンを使うScreenContext );

            if( Path.IsPathRooted( エフェクトファイルパス ) )
                loader = new サブリソースローダー( Path.GetDirectoryName( エフェクトファイルパス ) );

            var effect = エフェクト.ファイルをエフェクトとして読み込む( エフェクトファイルパス, this, loader );

            this.シーン用エフェクト管理.エフェクトをマスタリストに登録する( エフェクトファイルパス, effect, これを既定のエフェクトに指定する: true );    // 常に最後のやつが既定
        }

        public void Dispose()
        {
            this.シーン用エフェクト管理?.Dispose();
            this.シーン用エフェクト管理 = null;
        }

        public void 更新する()
        {
            // 更新は描画時に行う。（スワップチェーンを使うので）
        }

        public void 描画する()
        {
            var effect = this.シーン用エフェクト管理.既定のエフェクト;
            if( null == effect )
                throw new InvalidOperationException( "エフェクトが登録されていません。" );

            if( !( this._このシーンを利用するScreenContext.TryGetTarget( out ScreenContext screenContext ) ) )
                return;

            // 更新
            effect.モデルごとに更新するエフェクト変数を更新する();
            effect.シーンごとに更新するエフェクト変数を更新する( screenContext.SwapChain );

            // 描画
            RenderContext.Instance.DeviceManager.D3DDeviceContext.ClearRenderTargetView(
                RenderContext.Instance.レンダーターゲット配列[ 0 ],
                RenderContext.Instance.クリア色 );

            var IA = RenderContext.Instance.DeviceManager.D3DDevice.ImmediateContext.InputAssembler;

            IA.InputLayout = null;  // 頂点は頂点シェーダー内で生成すること。
            IA.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            effect.エフェクトを適用しつつシーンを描画する();
        }


        private WeakReference<ScreenContext> _このシーンを利用するScreenContext;
    }
}
