using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     　頂点のワールド変換×ビュー変換行列。
    ///     　型は float4x4 。
    /// </summary>
    /// <remarks>
    /// ○アノテーション
    ///     ・string Object(省略可能)
    ///         ビュー変換およびプロジェクション変換において、どこを視点とするかを指定する。
    ///         "Camera"または"Light"が指定できる。デフォルトは"Camera"である。
    ///         
    ///         通常の、カメラを視点とした座標変換を行う場合には、"Camera" を指定する。
    ///         セルフシャドウのためのZ値プロット等、光源を視点とした座標変換を行う場合には、"Light"を指定する。
    ///         
    /// ○使用例
    ///     float4x4 WorldMatrix : WORLD ;
    ///     float4x4 WorldViewProjMatrix : WORLDVIEWPROJECTION ;
    ///     float4x4 LightViewMatrix : VIEW<string Object = "Light"; > ;
    ///     float4x4 WorldInvMatrix : WORLDINVERSE ;
    ///     float4x4 WorldViewProjTransMatrix : WORLDVIEWPROJECTIONTRANSPOSE ;
    ///     
    /// ○補足
    /// ・Objectアノテーションに "Light" を指定したときに得られる行列は、
    /// 　MMDのセルフシャドウ処理に使用している行列と連動しているため、
    /// 　MME の [表示( V )]-[セルフシャドウ表示( P )] で、セルフシャドウ機能を完全にOFFにしてしまうと、
    /// 　正しい値を参照できなくなる。
    /// </remarks>
    internal sealed class WORLDVIEW変数 : 行列変数
	{
        public override string セマンティクス => "WORLDVIEW";


        public WORLDVIEW変数()
        {
        }

        private WORLDVIEW変数( Object種別 Object ) 
            : base( Object )
		{
		}

        protected override 変数管理 行列変数登録インスタンスを生成して返す( Object種別 Object )
        {
            return new WORLDVIEW変数( Object );
        }

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
            switch( ターゲットオブジェクト )
            {
                case Object種別.カメラ:
                    行列を登録する(
                        RenderContext.Instance.行列管理.ワールド行列管理.モデルのワールド変換行列を作成して返す( 引数.モデル ) *
                        RenderContext.Instance.行列管理.ビュー行列管理.ビュー行列,
                        変数 );
                    break;

                case Object種別.ライト:  // TODO: 未実装
                    break;
            }
		}
	}
}
