using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.定数
{
	internal class FULLMATERIALCONSTANT変数 : 変数管理, IDisposable
	{
        public override string セマンティクス => "FULLMATERIALCONSTANT";

        public override 更新タイミング 更新タイミング => 更新タイミング.材質ごと;

        public override 変数型[] 使える型の配列 => new[] { 変数型.Cbuffer };

        public struct FULLMATERIALCONSTANT変数用の定数バッファレイアウト
        {
            // 順番入れ替え危険

            public Vector4 AmbientColor;

            public Vector4 DiffuseColor;

            public Vector4 SpecularColor;

            public Vector4 EmissiveColor;

            public Vector4 ToonColor;

            public Vector4 EdgeColor;

            public Vector4 GroundShadowColor;

            public Vector4 AddingTexture;

            public Vector4 MultiplyingTexture;

            public Vector4 AddingSphereTexture;

            public Vector4 MultiplyingSphereTexture;

            public float SpecularPower;

            // 危険ここまで


            public static int SizeInBytes
            {
                get
                {
                    int size = Marshal.SizeOf( typeof( FULLMATERIALCONSTANT変数用の定数バッファレイアウト ) );

                    // 16の倍数じゃないとだめらしいので16の倍数にする
                    size = ( size % 16 == 0 ) ? size : ( size + 16 - size % 16 );

                    return size;
                }
            }
        }


        private FULLMATERIALCONSTANT変数( 定数バッファ管理<FULLMATERIALCONSTANT変数用の定数バッファレイアウト> manager )
		{
			_Manager = manager;
		}

        public FULLMATERIALCONSTANT変数()
        {
        }

        public void Dispose()
		{
			_Manager?.Dispose();
            _Manager = null;
        }

		public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
		{
			var layout = new FULLMATERIALCONSTANT変数用の定数バッファレイアウト {
				AmbientColor = 引数.材質.環境色,
				DiffuseColor = 引数.材質.拡散色,
				SpecularColor = 引数.材質.反射色,
				SpecularPower = 引数.材質.反射係数,
				AddingSphereTexture = 引数.材質.スフィア加算値,
				AddingTexture = 引数.材質.テクスチャ加算値,
				EdgeColor = 引数.材質.エッジ色,
				EmissiveColor = 引数.材質.放射色,
				GroundShadowColor = 引数.材質.地面影色,
				MultiplyingSphereTexture = 引数.材質.スフィア乗算値,
				MultiplyingTexture = 引数.材質.テクスチャ乗算値,
				ToonColor = 引数.材質.トゥーン色
			};

			_Manager.定数バッファを使って変数を更新する( layout );
		}

		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
			var manager = new 定数バッファ管理<FULLMATERIALCONSTANT変数用の定数バッファレイアウト>();

			manager.初期化する(
                (EffectConstantBuffer) variable,
                new FULLMATERIALCONSTANT変数用の定数バッファレイアウト(),
                FULLMATERIALCONSTANT変数用の定数バッファレイアウト.SizeInBytes );

			return new FULLMATERIALCONSTANT変数( manager );
		}


        private 定数バッファ管理<FULLMATERIALCONSTANT変数用の定数バッファレイアウト> _Manager;
    }
}
