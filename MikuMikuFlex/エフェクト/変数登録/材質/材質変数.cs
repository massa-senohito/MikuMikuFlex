using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.エフェクト.変数管理.材質
{
    public abstract class 材質変数 : 変数管理
    {
        public override 更新タイミング 更新タイミング => 更新タイミング.材質ごと;

        /// <summary>
        ///     true なら Vector3 、
        ///     false なら Vector4 。
        /// </summary>
        protected bool Vector3である;

        protected ターゲット種別 ターゲットオブジェクト;


        protected 材質変数()
        {
        }

        protected 材質変数( ターゲット種別 target, bool Vector3である )
        {
            ターゲットオブジェクト = target;
            this.Vector3である = Vector3である;
        }

        public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable d3dEffectVariable, エフェクト effect, int semanticIndex )
        {
            bool isVector3 = d3dEffectVariable.TypeInfo.Description.TypeName.ToLower().Equals( "float3" );


            if( !_Objectアノテーションが必須なセマンティクス.Contains( セマンティクス ) )
            {
                // (A) Object アノテーションが不要なセマンティクスの場合

                return 材質変数登録インスタンスを生成して返す( ターゲット種別.未使用, isVector3 );
            }
            else
            {
                // (B) Object アノテーションが必須のセマンティクスの場合

                EffectVariable Objectアノテーション = EffectParseHelper.アノテーションを取得する( d3dEffectVariable, "Object", "string" );

                if( Objectアノテーション == null )
                    throw new InvalidMMEEffectShader例外( $"このセマンティクス\"{セマンティクス}\"にはアノテーション「Object」が必須ですが、記述されませんでした。" );

                string annotation = Objectアノテーション.AsString().GetString().ToLower();

                if( string.IsNullOrWhiteSpace( annotation ) )
                {
                    throw new InvalidMMEEffectShader例外( $"このセマンティクス\"{セマンティクス}\"にはアノテーション「Object」が必須ですが、記述されませんでした。" );
                }

                switch( annotation )
                {
                    case "geometry":
                        return 材質変数登録インスタンスを生成して返す( ターゲット種別.ジオメトリ, isVector3 );

                    case "light":
                        return 材質変数登録インスタンスを生成して返す( ターゲット種別.ライト, isVector3 );

                    default:
                        throw new InvalidMMEEffectShader例外( $"アノテーション\"{annotation}\"は認識されません。" );
                }
            }
        }

        protected abstract 変数管理 材質変数登録インスタンスを生成して返す( ターゲット種別 target, bool isVector3 );

        protected void エフェクト変数にベクトルを設定する( Vector4 vector4, EffectVariable d3dEffectVariable, bool Vector3である )
        {
            if( Vector3である )
            {
                d3dEffectVariable.AsVector().Set( new Vector3( vector4.X, vector4.Y, vector4.Z ) );
            }
            else
            {
                d3dEffectVariable.AsVector().Set( vector4 );
            }
        }

        protected void エフェクトにスカラを設定する( float val, Effect d3dEffect, int index )
        {
            d3dEffect.GetVariableByIndex( index ).AsScalar().Set( val );
        }

        protected void エフェクト変数にスカラを設定する( float val, EffectVariable d3dEffectVariable )
        {
            d3dEffectVariable.AsScalar().Set( val );
        }
        

        private static readonly string[] _Objectアノテーションが必須なセマンティクス = new string[] {
            "DIFFUSE",
            "AMBIENT",
            "SPECULAR",
            //"EDGECOLOR",  -> 必須じゃない。
            "GROUNDSHADOWCOLOR",
        };
    }
}
