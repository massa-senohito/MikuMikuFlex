using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace MMF.エフェクト.変数管理.定数
{
	internal sealed class BASICMATERIALCONSTANT変数 : 変数管理, IDisposable
	{
        public override string セマンティクス => "BASICMATERIALCONSTANT";

        public override 更新タイミング 更新タイミング => 更新タイミング.材質ごと;

        public override 変数型[] 使える型の配列 => new[] { 変数型.Cbuffer };

        public struct 定数バッファレイアウト
        {
            // 順番入れ替え危険

            public Vector4 AmbientLight;

            public Vector4 DiffuseLight;

            public Vector4 SpecularLight;

            public float SpecularPower;

            // 危険ここまで


            public static int SizeInBytes
            {
                get
                {
                    int size = Marshal.SizeOf( typeof( 定数バッファレイアウト ) );

                    // 16の倍数じゃないとだめらしいので16の倍数にする
                    size = ( size % 16 == 0 ) ? size : ( size + 16 - size % 16 );

                    return size;
                }
            }
        }


        private BASICMATERIALCONSTANT変数( 定数バッファ管理<定数バッファレイアウト> manager )
        {
            _定数バッファ管理 = manager;
        }

        public BASICMATERIALCONSTANT変数()
        {
        }

        public void Dispose()
		{
			_定数バッファ管理?.Dispose();
            _定数バッファ管理 = null;
		}

		public override 変数管理 変数登録インスタンスを生成して返す( EffectVariable variable, エフェクト effect, int semanticIndex )
		{
            var manager = new 定数バッファ管理<定数バッファレイアウト>();

			manager.初期化する(
                (EffectConstantBuffer) variable,
                new 定数バッファレイアウト(),
                定数バッファレイアウト.SizeInBytes );

			return new BASICMATERIALCONSTANT変数( manager );
		}

        public override void 変数を更新する( EffectVariable 変数, 変数更新時引数 引数 )
        {
            var 更新内容 = new 定数バッファレイアウト {
                AmbientLight = 引数.材質.環境色,
                DiffuseLight = 引数.材質.拡散色,
                SpecularLight = 引数.材質.反射色,
                SpecularPower = 引数.材質.反射係数
            };

            _定数バッファ管理.定数バッファを使って変数を更新する( 更新内容 );
        }


        private 定数バッファ管理<定数バッファレイアウト> _定数バッファ管理;
    }
}
