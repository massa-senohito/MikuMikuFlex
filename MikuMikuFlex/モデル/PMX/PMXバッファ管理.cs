using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.BoneWeight;
using MMF.Utility;
using SharpDX;

namespace MMF.モデル.PMX
{
	public class PMXバッファ管理 : バッファ管理
	{
        // スキニング 前
        public MMM_SKINNING_INPUT[] 入力頂点リスト { get; private set; }

        public int 頂点数 => 入力頂点リスト.Length;

        // スキニング 後
        public SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; private set; }

		public SharpDX.Direct3D11.Buffer D3Dインデックスバッファ { get; private set; }

        // スキニング 後
        public SharpDX.Direct3D11.InputLayout D3D頂点レイアウト { get; private set; }

        public bool D3D頂点バッファをリセットする { get; set; }


        public void 初期化する( object model, SharpDX.Direct3D11.Effect d3dEffect )
		{
            _バッファを初期化する( model );

            this.D3D頂点レイアウト = new SharpDX.Direct3D11.InputLayout(
                RenderContext.Instance.DeviceManager.D3DDevice,
                d3dEffect.GetTechniqueByIndex( 0 ).GetPassByIndex( 0 ).Description.Signature, 
                SKINNING_OUTPUT.VertexElements );
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

        public void D3D頂点バッファを更新する( MMF.ボーン.スキニング skelton )
        {
            if( !( D3D頂点バッファをリセットする ) )
                return;

            var skinning = ( skelton as MMF.ボーン.PMXスケルトン ) ?? throw new System.NotSupportedException( "PMXバッファ管理クラスでは、スキニングとして PMXスケルトン クラスを指定してください。" );
            var boneTrans = skinning.ボーンのモデルポーズ配列;

            // 現在の入力頂点リストに対して、スキニングを実行。
            var スキニング後の入力頂点リスト = new SKINNING_OUTPUT[ 入力頂点リスト.Length ];
            for( int i = 0; i < 入力頂点リスト.Length; i++ )
            {
                switch( 入力頂点リスト[ i ].変形方式 )
                {
                    case 変形方式.BDEF1:
                        #region " *** "
                        //----------------
                        {
                            Matrix bt =
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex1 ];

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 入力頂点リスト[ i ].Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = 入力頂点リスト[ i ].Normal;
                        }
                        //----------------
                        #endregion
                        break;

                    case 変形方式.BDEF2:
                        #region " *** "
                        //----------------
                        {
                            Matrix bt =
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex1 ] * 入力頂点リスト[ i ].BoneWeight1 +
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex2 ] * 入力頂点リスト[ i ].BoneWeight2;

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 入力頂点リスト[ i ].Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = 入力頂点リスト[ i ].Normal;
                        }
                        //----------------
                        #endregion
                        break;

                    case 変形方式.BDEF4:
                    case 変形方式.QDEF:
                        #region " *** "
                        //----------------
                        {
                            Matrix bt =
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex1 ] * 入力頂点リスト[ i ].BoneWeight1 +
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex2 ] * 入力頂点リスト[ i ].BoneWeight2 +
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex3 ] * 入力頂点リスト[ i ].BoneWeight3 +
                                boneTrans[ 入力頂点リスト[ i ].BoneIndex4 ] * 入力頂点リスト[ i ].BoneWeight4;

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 入力頂点リスト[ i ].Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = 入力頂点リスト[ i ].Normal;
                        }
                        //----------------
                        #endregion
                        break;

                    case 変形方式.SDEF:
                        #region " *** "
                        //----------------
                        {
                            // 参考: 
                            // 自分用メモ「PMXのスフィリカルデフォームのコードっぽいもの」（sma42氏）
                            // https://www.pixiv.net/member_illust.php?mode=medium&illust_id=60755964

                            var 頂点 = 入力頂点リスト[ i ];

                            #region " 影響度0,1 の算出 "
                            //----------------
                            float 影響度0 = 0f;  // 固定値であるSDEFパラメータにのみ依存するので、これらの値も固定値。
                            float 影響度1 = 0f;  //
                            {
                                float L0 = ( 頂点.SdefR0 - (Vector3) skinning.ボーン配列[ 頂点.BoneIndex2 ].ローカル位置 ).Length();   // 子ボーンからR0までの距離
                                float L1 = ( 頂点.SdefR1 - (Vector3) skinning.ボーン配列[ 頂点.BoneIndex2 ].ローカル位置 ).Length();   // 子ボーンからR1までの距離

                                影響度0 = ( System.Math.Abs( L0 - L1 ) < 0.0001f ) ? 0.5f : SharpDX.MathUtil.Clamp( L0 / ( L0 + L1 ), 0.0f, 1.0f );
                                影響度1 = 1.0f - 影響度0;
                            }
                            //----------------
                            #endregion

                            Matrix モデルポーズ行列L = boneTrans[ 頂点.BoneIndex1 ] * 頂点.BoneWeight1;
                            Matrix モデルポーズ行列R = boneTrans[ 頂点.BoneIndex2 ] * 頂点.BoneWeight2;
                            Matrix モデルポーズ行列C = モデルポーズ行列L + モデルポーズ行列R;

                            Vector4 点C = Vector4.Transform( 頂点.Sdef_C, モデルポーズ行列C );    // BDEF2で計算された点Cの位置
                            Vector4 点P = Vector4.Transform( 頂点.Position, モデルポーズ行列C );  // BDEF2で計算された頂点の位置

                            Matrix 重み付き回転行列 = Matrix.RotationQuaternion(
                                Quaternion.Slerp(   // 球体線形補間
                                    skinning.ボーン配列[ 頂点.BoneIndex1 ].回転 * 頂点.BoneWeight1,
                                    skinning.ボーン配列[ 頂点.BoneIndex2 ].回転 * 頂点.BoneWeight2,
                                    頂点.BoneWeight1 ) );

                            Vector4 点R0 = Vector4.Transform( new Vector4( 頂点.SdefR0, 1f ), ( モデルポーズ行列L + ( モデルポーズ行列C * -頂点.BoneWeight1 ) ) );
                            Vector4 点R1 = Vector4.Transform( new Vector4( 頂点.SdefR1, 1f ), ( モデルポーズ行列R + ( モデルポーズ行列C * -頂点.BoneWeight2 ) ) );
                            点C += ( 点R0 * 影響度0 ) + ( 点R1 * 影響度1 );   // 膨らみすぎ防止

                            点P -= 点C;     // 頂点を点Cが中心になるよう移動して
                            点P = Vector4.Transform( 点P, 重み付き回転行列 );   // 回転して
                            点P += 点C;     // 元の位置へ

                            スキニング後の入力頂点リスト[ i ].Position = 点P;
                            スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, 重み付き回転行列 );
                        }
                        //----------------
                        #endregion
                        break;
                }

                スキニング後の入力頂点リスト[ i ].UV = 入力頂点リスト[ i ].UV;
                スキニング後の入力頂点リスト[ i ].AddUV1 = 入力頂点リスト[ i ].AddUV1;
                スキニング後の入力頂点リスト[ i ].AddUV2 = 入力頂点リスト[ i ].AddUV2;
                スキニング後の入力頂点リスト[ i ].AddUV3 = 入力頂点リスト[ i ].AddUV3;
                スキニング後の入力頂点リスト[ i ].AddUV4 = 入力頂点リスト[ i ].AddUV4;
                スキニング後の入力頂点リスト[ i ].EdgeWeight = 入力頂点リスト[ i ].EdgeWeight;
                スキニング後の入力頂点リスト[ i ].Index = 入力頂点リスト[ i ].Index;
            }

            // データストリームに、スキニング後の入力頂点リストを書き込む。
            _頂点データストリーム.WriteRange( スキニング後の入力頂点リスト );
            _頂点データストリーム.Position = 0;

            // D3D頂点バッファに、スキニング後の入力頂点リストを（データストリーム経由で）書き込む。
            RenderContext.Instance.DeviceManager.D3DDeviceContext.UpdateSubresource( new DataBox( _頂点データストリーム.DataPointer, 0, 0 ), D3D頂点バッファ, 0 );

            D3D頂点バッファをリセットする = false;
        }

        // スキニング 後
        private DataStream _頂点データストリーム;


        private void _バッファを初期化する( object model )
		{
            var d3dDevice = RenderContext.Instance.DeviceManager.D3DDevice;
			var モデル = (PMXモデル) model;

            
            // モデルの頂点リストから入力頂点リスト（スキニング前）を作成する。

            var 頂点リスト = new List<MMM_SKINNING_INPUT>(); // スキニング 前

            for( int i = 0; i < モデル.頂点リスト.Count; i++ )
				_頂点データを頂点レイアウトリストに追加する( モデル.頂点リスト[ i ], 頂点リスト );

            入力頂点リスト = 頂点リスト.ToArray();


            // スキニング後のデータストリームと頂点バッファを作成する。

            _頂点データストリーム = new DataStream( 頂点リスト.Count * SKINNING_OUTPUT.SizeInBytes, canRead: true, canWrite: true );  // アンマネージドメモリとして確保される。
            D3D頂点バッファ = CGHelper.D3Dバッファを作成する( d3dDevice, 頂点リスト.Count * SKINNING_OUTPUT.SizeInBytes, SharpDX.Direct3D11.BindFlags.VertexBuffer );

            D3D頂点バッファをリセットする = true;    // どちらも空なので、描画前に設定すること。


            // モデルの面リストから、D3Dインデックスバッファを作成する。

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
                Index = (uint) 頂点レイアウトリスト.Count,    // 現在の要素数 ＝ List<>内でのこの要素のインデックス番号
                EdgeWeight = 頂点データ.エッジ倍率,
                変形方式 = 頂点データ.ウェイト変形方式,
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