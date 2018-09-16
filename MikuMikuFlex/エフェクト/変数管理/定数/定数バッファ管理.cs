using System;
using SharpDX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     定数バッファを対象とする変数管理。
    /// </summary>
    /// <remarks>
    ///     定数バッファ（cbuffer）にはセマンティクスを指定することができないため（指定しても認識されない；コンパイルは通る）、変数名で識別する。
    ///     特定の名前を指定された cbuffer に対しては、レイアウトにしたがって、その定数バッファに（材質ごとに）必要なデータが MMF 側から渡される。
    ///     なお、cbuffer 名の大文字小文字は識別されず、cbuffer 内の各要素の名前、セマンティクスも考慮されないので、
    ///     cbuffer 内の要素は、レイアウトの順番に記載する必要はあるが、その名前は自由である。
    ///     cbuffer 内の要素は、その名前をもって、グローバル変数と同様に扱うことが可能。
    /// </remarks>
    /// <typeparam name="T">定数バッファレイアウトを表す構造体。</typeparam>
	public class 定数バッファ管理<T> : IDisposable where T : struct
	{
		public Buffer D3D定数バッファ;


        public void 初期化する( EffectConstantBuffer d3dエフェクト定数バッファ, T 定数バッファのレイアウト構造体, int 定数バッファのレイアウト構造体のサイズbytes )
		{
			_D3Dエフェクト定数バッファ = d3dエフェクト定数バッファ;

            _BufferDataStream = DataStream.Create(
                new[] { 定数バッファのレイアウト構造体 },
                canRead: true, 
                canWrite: true, 
                pinBuffer: true );  // copy & pinned

            D3D定数バッファ = new Buffer( 
                RenderContext.Instance.DeviceManager.D3DDevice,
                new BufferDescription {
                    SizeInBytes = 定数バッファのレイアウト構造体のサイズbytes,
                    BindFlags = BindFlags.ConstantBuffer,
                } );
		}

        public void Dispose()
        {
            _BufferDataStream?.Dispose();
            _BufferDataStream = null;

            D3D定数バッファ?.Dispose();
            D3D定数バッファ = null;

            _D3Dエフェクト定数バッファ = null;
        }

        public virtual void 定数バッファを使って変数を更新する( T 更新内容 )
        {
            // 定数バッファに書き込む
            _BufferDataStream.WriteRange( new[] { 更新内容 } );
            _BufferDataStream.Position = 0;
            RenderContext.Instance.DeviceManager.D3DDeviceContext.UpdateSubresource(
                source: new DataBox( _BufferDataStream.DataPointer, 0, 0 ),
                resource: D3D定数バッファ,
                subresource: 0 );

            // エフェクト定数バッファに定数バッファをセットする
            _D3Dエフェクト定数バッファ.SetConstantBuffer( D3D定数バッファ );
        }


        private EffectConstantBuffer _D3Dエフェクト定数バッファ;

        private DataStream _BufferDataStream;
    }
}
