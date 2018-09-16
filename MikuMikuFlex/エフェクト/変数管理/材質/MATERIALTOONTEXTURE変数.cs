using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
	internal sealed class MATERIALTOONTEXTURE変数 : 材質変数
	{
		public override string セマンティクス => "MATERIALTOONTEXTURE";

		public override 変数型[] 使える型の配列 => new[] { 変数型.Texture2D };


        public MATERIALTOONTEXTURE変数()
        {
        }

		protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
		{
            return new MATERIALTOONTEXTURE変数();
		}

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
            // ターゲットオブジェクトに依存しない
			変数.AsShaderResource().SetResource( 引数.材質.トゥーンテクスチャ );
		}
	}
}
