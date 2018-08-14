using SharpDX;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.行列
{
	public abstract class 行列変数 : 変数管理
	{
		protected Object種別 ターゲットオブジェクト;

        public override 変数型[] 使える型の配列 => new[] { 変数型.Float4x4 };


        protected 行列変数( Object種別 Object )
		{
			ターゲットオブジェクト = Object;
		}

		protected 行列変数()
		{
		}

		protected void 行列を登録する( Matrix 行列, EffectVariable 変数 )
		{
			変数.AsMatrix().SetMatrix( 行列 );
		}

		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
            // 行列の場合は、それぞれCameraかLightか調べる。

            EffectVariable Ojbectアノテーション = EffectParseHelper.アノテーションを取得する( variable, "Object", "string" );

			string obj = ( Ojbectアノテーション == null ) ? "" : Ojbectアノテーション.AsString().GetString(); // アノテーションが存在しない時は""とする

            switch( obj.ToLower() )
            {
                case "camera":
                    return 行列変数登録インスタンスを生成して返す( Object種別.カメラ );

                case "light":
                    return 行列変数登録インスタンスを生成して返す( Object種別.ライト );

                case "":
                    return 行列変数登録インスタンスを生成して返す( Object種別.カメラ );   // 既定値

                default:
                    throw new InvalidMMEEffectShader例外(
                        $"変数「{variable.TypeInfo.Description.TypeName.ToLower()} {variable.Description.Name}:{variable.Description.Semantic}」には、" +
                        $"アノテーション「string Object=\"Camera\"」または、「string Object=\"Light\"」が必須ですが指定されたのは「string Object=\"{obj}\"」でした。(スペルミス?)" );
            }
		}

		protected abstract 変数管理 行列変数登録インスタンスを生成して返す( Object種別 Object );
	}
}
