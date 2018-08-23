using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.材質
{
	internal sealed class MATERIALTEXTURE変数 : 材質変数
	{
        public override string セマンティクス => "MATERIALTEXTURE";

        public override 変数型[] 使える型の配列 => new[] { 変数型.Texture2D };


        public MATERIALTEXTURE変数()
		{
		}

		protected override 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 )
		{
			return new MATERIALTEXTURE変数();
		}

		public override void 変数を更新する( EffectVariable subscribeTo, 変数更新時引数 variable )
		{
            // ターゲットオブジェクトに依存しない
			subscribeTo.AsShaderResource().SetResource( variable.材質.テクスチャ );
		}
	}
}
