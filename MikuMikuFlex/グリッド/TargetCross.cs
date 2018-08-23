using System.Collections.Generic;
using MikuMikuFlex.モデル;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace MikuMikuFlex.グリッド
{
	/// <summary>
	///     デバッグ用にボーンの位置などを示す際などに利用するもの
	/// </summary>
	public class TargetCross : IDrawable
	{
        public Vector4 セルフシャドウ色 { get; set; }

        public Vector4 地面影色 { get; set; }

        public bool 表示中 { get; set; }

        public string ファイル名 { get; set; }

        public int サブセット数 { get; private set; }

        public int 頂点数 { get; private set; }

        public モデル状態 モデル状態 { get; private set; }

        public bool 可視である { get; set; }


        public TargetCross()
		{
			モデル状態 = new Transformer基本実装();
		}

        public void 初期化する()
        {
            可視である = true;

            _バッファを作成する();

            サブセット数 = 1;
        }

        public void Dispose()
        {
            _軸の頂点バッファ?.Dispose();
            _軸の頂点バッファ = null;

            _軸の入力レイアウト?.Dispose();
            _軸の入力レイアウト = null;

            _エフェクト?.Dispose();
            _エフェクト = null;
        }

        public void 描画する()
		{
			var d3dContext = RenderContext.Instance.DeviceManager.D3DDevice.ImmediateContext;

			d3dContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;

            _エフェクト.GetVariableBySemantic( "WORLDVIEWPROJECTION" ).AsMatrix().SetMatrix( RenderContext.Instance.行列管理.ワールドビュー射影行列を作成する( モデル状態.倍率, モデル状態.回転, モデル状態.位置 ) );

            if( 可視である )
			{
				d3dContext.InputAssembler.InputLayout = _軸の入力レイアウト;
                d3dContext.InputAssembler.SetVertexBuffers( 0, new VertexBufferBinding( _軸の頂点バッファ, AxisGridLayout.SizeInBytes, 0 ) );

                _エフェクト.GetTechniqueByIndex( 0 ).GetPassByIndex( 1 ).Apply( d3dContext );

                d3dContext.Draw( 7 * _軸の頂点数, 0 );
			}
		}

		public void 更新する()
		{
		}


        private const int _軸の長さ = 3;

        private InputLayout _軸の入力レイアウト;

        private int _軸の頂点数;

        private Buffer _軸の頂点バッファ;

        private Effect _エフェクト;


        private void _バッファを作成する()
		{
			//エフェクトを読み込む
			using( ShaderBytecode byteCode = ShaderBytecode.CompileFromFile( @"Shader\grid.fx", "fx_5_0" ) )
			{
				_エフェクト = new Effect( RenderContext.Instance.DeviceManager.D3DDevice, byteCode );
			}
			//まずリストに頂点を格納
			List<float> axisVector = new List<float>();
			_軸として頂点を格納する( axisVector, _軸の長さ, 0, 0, new Vector4( 1, 0, 0, 1 ) );
			_軸として頂点を格納する( axisVector, 0, _軸の長さ, 0, new Vector4( 0, 1, 0, 1 ) );
			_軸として頂点を格納する( axisVector, 0, 0, _軸の長さ, new Vector4( 0, 0, 1, 1 ) );
			//バッファを作成
			using( var vs = DataStream.Create( axisVector.ToArray(), true, true ) )
			{
				BufferDescription bufDesc = new BufferDescription {
					BindFlags = BindFlags.VertexBuffer,
					SizeInBytes = (int) vs.Length
				};
				_軸の頂点バッファ = new Buffer( RenderContext.Instance.DeviceManager.D3DDevice, vs, bufDesc );
			}
			_軸の頂点数 = axisVector.Count;
			//入力レイアウトを作成
			var v = _エフェクト.GetTechniqueByIndex( 0 ).GetPassByIndex( 1 ).Description.Signature;
			_軸の入力レイアウト = new InputLayout( RenderContext.Instance.DeviceManager.D3DDevice, v, AxisGridLayout.VertexElements );
			頂点数 = _軸の頂点数;
		}

		/// <summary>
		///     軸として頂点を格納する
		///     x,y,zはどれか１つがnot 0,それ以外0を想定
		/// </summary>
		/// <param name="vertexList">頂点バッファ作成用リスト</param>
		/// <param name="x">xの長さ</param>
		/// <param name="y">yの長さ</param>
		/// <param name="z">zの長さ</param>
		/// <param name="color">色</param>
		private static void _軸として頂点を格納する( List<float> vertexList, float x, float y, float z, Vector4 color )
		{
			vertexList.Add( x );
			vertexList.Add( y );
			vertexList.Add( z );
			vertexList.Add( color.X );
			vertexList.Add( color.Y );
			vertexList.Add( color.Z );
			vertexList.Add( color.W );
			vertexList.Add( -x );
			vertexList.Add( -y );
			vertexList.Add( -z );
			vertexList.Add( color.X );
			vertexList.Add( color.Y );
			vertexList.Add( color.Z );
			vertexList.Add( color.W );
		}
	}
}
