using MMDFileParser.PMXModelParser;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///  spadd (bool型)
    ///  スフィアマップ加算合成フラグ（trueで加算合成）。
    /// </summary>
	internal class spadd変数 : 特殊パラメータ変数
	{
		public override string 変数名 => "spadd";

		public override 変数型 変数型 => 変数型.Bool;


        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			変数.AsScalar().Set( 引数.材質.スフィアモード == スフィアモード.加算 );
		}
	}
}
