using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.ワールド情報
{
	public abstract class ワールド情報変数 : 変数管理
	{
		internal Object種別 Object;

        public override 変数型[] 使える型の配列 => new 変数型[] { 変数型.Float3, 変数型.Float4 };


        protected ワールド情報変数( Object種別 obj )
		{
			Object = obj;
		}

		internal ワールド情報変数()
		{
    	}

		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effectManager, int semanticIndex )
		{
            // アノテーション "string Object = ..." を取得

			EffectVariable annotation = EffectParseHelper.アノテーションを取得する( variable, "Object", "string" );
            if( annotation == null || string.IsNullOrWhiteSpace( annotation.AsString().GetString() ) )
			{
				throw new InvalidMMEEffectShader例外(
					string.Format( 
                        "変数「{0} {1}:{2}」にはアノテーション「string Object=\"Light\"」または「string object=\"Camera\"」が必須ですが指定されませんでした。",
						variable.TypeInfo.Description.TypeName.ToLower(), variable.Description.Name,
						variable.Description.Semantic ) );
			}


            // Object の内容で type を決める。

            Object種別 type;
            string objectStr = annotation.AsString().GetString().ToLower();

            switch( objectStr )
			{
				case "camera":
					type = Object種別.カメラ;
					break;

                case "light":
					type = Object種別.ライト;
					break;

                default:
					throw new InvalidMMEEffectShader例外(
						string.Format(
							"変数「{0} {1}:{2}」にはアノテーション「string Object=\"Light\"」または「string object=\"Camera\"」が必須ですが指定されたのは,「string object=\"{3}\"でした。(スペルミス?)",
							variable.TypeInfo.Description.TypeName.ToLower(), 
							variable.Description.Name,
							variable.Description.Semantic, 
							objectStr ) );
			}


            // 決定された type へ引き継ぐ。

			return GetSubscriberInstance( type );
		}

		protected abstract 変数管理 GetSubscriberInstance( Object種別 type );
	}
}
