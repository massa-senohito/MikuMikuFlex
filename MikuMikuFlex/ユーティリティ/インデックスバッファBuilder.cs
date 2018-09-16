using System.Collections.Generic;
using SharpDX.Direct3D11;

namespace MikuMikuFlex
{
	/// <summary>
	///		インデックスバッファを作成するためのビルダ。
	/// </summary>
	/// <remarks>
	///		PrimitiveTopology = TriangleList を想定し、三角形や四角形からなるインデックスバッファを作成することができる。
	/// </remarks>
	public class インデックスバッファBuilder
	{
		public インデックスバッファBuilder()
		{
		}

		public void 三角形を追加する( uint v1, uint v2, uint v3 )
		{
			this._IndexList.Add( v1 );
			this._IndexList.Add( v2 );
			this._IndexList.Add( v3 );
		}

		public void 四角形を追加する( uint v1, uint v2, uint v3, uint v4 )
		{
			this._IndexList.Add( v1 );
			this._IndexList.Add( v2 );
			this._IndexList.Add( v4 );
			this._IndexList.Add( v4 );
			this._IndexList.Add( v2 );
			this._IndexList.Add( v3 );
		}

		/// <summary>
		///		現時点までに蓄積された三角形や四角形から、インデックスバッファを生成して返す。
		/// </summary>
		public Buffer インデックスバッファを作成する()
		{
			return CGHelper.D3Dバッファを作成する( _IndexList, RenderContext.Instance.DeviceManager.D3DDevice, BindFlags.IndexBuffer );
		}


        private List<uint> _IndexList = new List<uint>();
	}
}
