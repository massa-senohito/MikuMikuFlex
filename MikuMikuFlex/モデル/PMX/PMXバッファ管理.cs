using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.BoneWeight;
using MMF.Utility;
using SharpDX;

namespace MMF.モデル.PMX
{
	public class PMXバッファ管理 : バッファ管理
	{
		public SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; private set; }

		public SharpDX.Direct3D11.Buffer D3Dインデックスバッファ { get; private set; }

        public MMM_SKINNING_INPUT[] 入力頂点リスト { get; private set; }

        public int 頂点数 => 入力頂点リスト.Length;

        public SharpDX.Direct3D11.InputLayout D3D頂点レイアウト { get; set; }

        public bool リセットが必要である { get; set; }


        public void 初期化する( object model, SharpDX.Direct3D11.Effect d3dEffect )
		{
            _バッファを初期化する( model );

            this.D3D頂点レイアウト = new SharpDX.Direct3D11.InputLayout(
                RenderContext.Instance.DeviceManager.D3DDevice,
                d3dEffect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Description.Signature, 
                MMM_SKINNING_INPUT.VertexElements );
		}

        public void Dispose()
        {
            D3D頂点バッファ?.Dispose();
            D3D頂点バッファ = null;

            D3Dインデックスバッファ?.Dispose();
            D3Dインデックスバッファ = null;

            D3D頂点レイアウト?.Dispose();
            D3D頂点レイアウト = null;

            _頂点データストリーム?.Dispose();   // pinned 解放
            _頂点データストリーム = null;
        }

        public void 必要であれば頂点を再作成する()
        {
            if( リセットが必要である )
            {
                _頂点データストリーム.WriteRange( 入力頂点リスト );
                _頂点データストリーム.Position = 0;

                RenderContext.Instance.DeviceManager.D3DDeviceContext.UpdateSubresource( new DataBox( _頂点データストリーム.DataPointer, 0, 0 ), D3D頂点バッファ, 0 );

                リセットが必要である = false;
            }
        }


        private DataStream _頂点データストリーム;


        private void _バッファを初期化する( object model )
		{
            var d3dDevice = RenderContext.Instance.DeviceManager.D3DDevice;
			var モデル = (PMXモデル) model;
			var 頂点リスト = new List<MMM_SKINNING_INPUT>();


            // モデルの頂点リストから、頂点データストリームを構築し（ピン止めあり）、D3D頂点バッファと入力頂点リストを更新する。

            for( int i = 0; i < モデル.頂点リスト.Count; i++ )
				_頂点データを頂点レイアウトリストに追加する( モデル.頂点リスト[ i ], 頂点リスト );

            _頂点データストリーム = DataStream.Create( 頂点リスト.ToArray(), true, true, pinBuffer: true );   // pinned

			D3D頂点バッファ = CGHelper.D3Dバッファを作成する( d3dDevice, 頂点リスト.Count * MMM_SKINNING_INPUT.SizeInBytes, SharpDX.Direct3D11.BindFlags.VertexBuffer );
            d3dDevice.ImmediateContext.UpdateSubresource( new DataBox( _頂点データストリーム.DataPointer, 0, 0 ), D3D頂点バッファ, 0 );

            入力頂点リスト = 頂点リスト.ToArray();


            // モデルの面リストから、D3Dインデックスバッファを更新する。

            var インデックスリスト = new List<uint>();

            foreach( 面 surface in モデル.面リスト )
			{
				インデックスリスト.Add( surface.頂点1 );
				インデックスリスト.Add( surface.頂点2 );
				インデックスリスト.Add( surface.頂点3 );
			}

            D3Dインデックスバッファ = CGHelper.D3Dバッファを作成する( インデックスリスト, d3dDevice, SharpDX.Direct3D11.BindFlags.IndexBuffer );
		}

		private void _頂点データを頂点レイアウトリストに追加する( 頂点 頂点データ, List<MMM_SKINNING_INPUT> 頂点レイアウトリスト )
		{
            var layout = new MMM_SKINNING_INPUT() {
                Position = new Vector4( 頂点データ.位置, 1f ),
                Normal = 頂点データ.法線,
                UV = 頂点データ.UV,
                Index = (uint) 頂点レイアウトリスト.Count,
                EdgeWeight = 頂点データ.エッジ倍率,
            };

            switch( 頂点データ.ウェイト変形方式 )
            {
                case 変形方式.BDEF1:
                    {
                        var v = (BDEF1) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) v.boneReferenceIndex;
                        layout.BoneWeight1 = 1.0f;
                    }
                    break;

                case 変形方式.BDEF2:
                    {
                        var v = (BDEF2) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) v.Bone1ReferenceIndex;
                        layout.BoneIndex2 = (uint) v.Bone2ReferenceIndex;
                        layout.BoneWeight1 = v.Bone1Weight;
                        layout.BoneWeight2 = 1f - v.Bone1Weight;
                    }
                    break;

                case 変形方式.SDEF:
                    {
                        //TODO: 以下はまだBDEF2としての実装であり、これをSDEFとして実装すること。
                        var v = (SDEF) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) v.Bone1ReferenceIndex;
                        layout.BoneIndex2 = (uint) v.Bone2ReferenceIndex;
                        layout.BoneWeight1 = v.Bone1Weight;
                        layout.BoneWeight2 = 1f - v.Bone1Weight;
                        layout.Sdef_C = new Vector4( v.SDEF_C, 1f );
                        layout.SdefR0 = v.SDEF_R0;
                        layout.SdefR1 = v.SDEF_R1;
                    }
                    break;

                case 変形方式.BDEF4:
                case 変形方式.QDEF:
                    {
                        var v = (BDEF4) 頂点データ.ボーンウェイト;
                        float sumWeight = v.Weights.X + v.Weights.X + v.Weights.Z + v.Weights.W;
                        layout.BoneIndex1 = (uint) v.Bone1ReferenceIndex;
                        layout.BoneIndex2 = (uint) v.Bone2ReferenceIndex;
                        layout.BoneIndex3 = (uint) v.Bone3ReferenceIndex;
                        layout.BoneIndex4 = (uint) v.Bone4ReferenceIndex;
                        layout.BoneWeight1 = v.Weights.X / sumWeight;
                        layout.BoneWeight2 = v.Weights.Y / sumWeight;
                        layout.BoneWeight3 = v.Weights.Z / sumWeight;
                        layout.BoneWeight4 = v.Weights.W / sumWeight;
                    }
                    break;
            }

            頂点レイアウトリスト.Add( ( layout ) );
		}
    }
}