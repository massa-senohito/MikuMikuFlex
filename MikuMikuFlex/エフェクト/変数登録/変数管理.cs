using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理
{
	/// <summary>
	///     エフェクト変数を管理し更新するための基底クラス。
	/// </summary>
	public abstract class 変数管理
	{
		public abstract string セマンティクス { get; }

		public abstract 変数型[] 使える型の配列 { get; }

        /// <summary>
        ///     変数を更新する（エフェクトを適用する）タイミング。
        ///     セマンティクスごとにあらかじめ定められている。
        /// </summary>
		public virtual 更新タイミング 更新タイミング
			=> 更新タイミング.モデルごと;


		public abstract 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex );

		public void 指定されたエフェクト変数の型名が正しいか確認し不正なら例外を発出する( EffectVariable variable )
		{
			EffectType type = variable.TypeInfo;
			string typeName = type.Description.TypeName.ToLower();
			変数型 valType;
			switch( typeName )
			{
				case "float4x4":
					valType = 変数型.Float4x4;
					break;
				case "float4":
					valType = 変数型.Float4;
					break;
				case "float3":
					valType = 変数型.Float3;
					break;
				case "float2":
					valType = 変数型.Float2;
					break;
				case "float":
					valType = 変数型.Float;
					break;
				case "uint":
					valType = 変数型.Uint;
					break;
				case "texture2d":
					valType = 変数型.Texture2D;
					break;
				case "texture":
					valType = 変数型.Texture;
					break;
				case "texture3d":
					valType = 変数型.Texture3D;
					break;
				case "texturecube":
					valType = 変数型.TextureCUBE;
					break;
				case "int":
					valType = 変数型.Int;
					break;
				case "bool":
					valType = 変数型.Bool;
					break;
				case "cbuffer":
					valType = 変数型.Cbuffer;
					break;
				default:
					throw new InvalidMMEEffectShader例外(
						string.Format( "定義済みセマンティクス「{0}」に対して不適切な型「{1}」が使用されました。これは「{2}」であるべきセマンティクスです。", セマンティクス,
							typeName, _サポートされる型のリスト() ) );
			}
			if( !使える型の配列.Contains( valType ) )
			{
				throw new InvalidMMEEffectShader例外(
					string.Format( "定義済みセマンティクス「{0}」に対して不適切な型「{1}」が使用されました。これは「{2}」であるべきセマンティクスです。", セマンティクス, typeName,
						_サポートされる型のリスト() ) );
			}
		}
        
        public abstract void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 );


        private string _サポートされる型のリスト()
		{
			var builder = new StringBuilder();

			foreach( 変数型 variableType in 使える型の配列 )
			{
				builder.Append( variableType.ToString().ToLower() );
				builder.Append( "/" );
			}

			return builder.ToString();
		}
	}
}
