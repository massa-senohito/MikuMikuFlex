using System.Collections.Generic;
using System.Linq;
using SharpDX;
using MMDFileParser.PMXModelParser;
using MMDFileParser.PMXModelParser.BoneWeight;

namespace MikuMikuFlex.モデル
{
    public class PMXバッファ管理 : バッファ管理
    {
        /// <summary>
        ///     コンピュートシェーダーの入力（の元になる配列）。
        /// </summary>
        /// <remarks>
        ///     進行時、この配列の内容は、各モーフから直接更新される。
        ///     描画時、コンピューターシェーダーの入力として、<see cref="_頂点データストリーム"/> 経由で
        ///     <see cref="D3Dスキニングバッファ"/> に書き込まれる。
        /// </remarks>
        public CS_INPUT[] 入力頂点リスト { get; private set; }

        /// <summary>
        ///     コンピュートシェーダーの入力。
        /// </summary>
        public SharpDX.Direct3D11.Buffer D3Dスキニングバッファ { get; private set; }

        /// <summary>
        ///     コンピュートシェーダーの出力、兼、頂点シェーダーの入力（＝入力アセンブラの入力）。
        /// </summary>
        public SharpDX.Direct3D11.Buffer D3D頂点バッファ { get; private set; }

        /// <summary>
        ///     入力アセンブラの入力。プリミティブの各頂点のインデックスを示す。
        /// </summary>
        public SharpDX.Direct3D11.Buffer D3Dインデックスバッファ { get; private set; }

        /// <summary>
        ///     入力アセンブラの入力。頂点バッファの要素のレイアウトを示す。
        /// </summary>
        public SharpDX.Direct3D11.InputLayout D3D頂点レイアウト { get; private set; }

        /// <summary>
        ///     コンピュートシェーダーが入力（<see cref="D3Dスキニングバッファ"/>）に対して適用するビュー。
        /// </summary>
        public SharpDX.Direct3D11.ShaderResourceView D3DスキニングバッファSRView { get; private set; }

        /// <summary>
        ///     コンピュートシェーダーが出力（<see cref="D3D頂点バッファ"/>）に対して適用するビュー。
        /// </summary>
        public SharpDX.Direct3D11.UnorderedAccessView D3D頂点バッファビューUAView { get; private set; }

        /// <summary>
        ///     これを true にすると、現在の <see cref="入力頂点リスト"/> の内容が <see cref="D3Dスキニングバッファ"/> に書き込まれる。
        ///     書き込んだ後は自動的に false に戻る。
        /// </summary>
        public bool D3Dスキニングバッファをリセットする { get; set; }


        public void 初期化する( object model, SharpDX.Direct3D11.Effect d3dEffect )
        {
            var d3dDevice = RenderContext.Instance.DeviceManager.D3DDevice;
            var モデル = (PMXモデル) model;


            // 入力頂点リストを作成する。

            var 頂点リスト = new List<CS_INPUT>();

            for( int i = 0; i < モデル.頂点リスト.Count; i++ )
                this._頂点データを頂点レイアウトリストに追加する( モデル.頂点リスト[ i ], 頂点リスト );

            this.入力頂点リスト = 頂点リスト.ToArray();


            // インデックスバッファを作成する。内容は モデルの面リスト で初期化。

            var インデックスリスト = new List<uint>();

            foreach( 面 surface in モデル.面リスト )
            {
                インデックスリスト.Add( surface.頂点1 );
                インデックスリスト.Add( surface.頂点2 );
                インデックスリスト.Add( surface.頂点3 );
            }

            this.D3Dインデックスバッファ = CGHelper.D3Dバッファを作成する( インデックスリスト, d3dDevice, SharpDX.Direct3D11.BindFlags.IndexBuffer );


            // 頂点データストリームを作成する。内容は空。

            this._頂点データストリーム = new DataStream( this.入力頂点リスト.Length * CS_INPUT.SizeInBytes, canRead: true, canWrite: true );


            // D3Dスキニングバッファを作成する。内容は空。

            this.D3Dスキニングバッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new SharpDX.Direct3D11.BufferDescription {
                    SizeInBytes = CS_INPUT.SizeInBytes * this.入力頂点リスト.Length,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.BufferStructured,   // 構造化バッファ
                    StructureByteStride = CS_INPUT.SizeInBytes,
                } );

            D3Dスキニングバッファをリセットする = true;    // 今はストリームもスキニングバッファも空なので、描画前に設定するようフラグを立てておく。


            // 頂点バッファを作成する。

            this.D3D頂点バッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new SharpDX.Direct3D11.BufferDescription {
                    SizeInBytes = VS_INPUT.SizeInBytes * this.入力頂点リスト.Length,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    BindFlags = SharpDX.Direct3D11.BindFlags.VertexBuffer | SharpDX.Direct3D11.BindFlags.ShaderResource | SharpDX.Direct3D11.BindFlags.UnorderedAccess,  // 非順序アクセス
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.BufferAllowRawViews,   // 生ビューバッファ
                } );

            // 頂点シェーダー用の入力レイアウトを作成する。

            this.D3D頂点レイアウト = new SharpDX.Direct3D11.InputLayout(
                d3dDevice,
                d3dEffect.GetTechniqueByName( "DefaultObject" ).GetPassByIndex( 0 ).Description.Signature,
                VS_INPUT.VertexElements );


            // コンピュートシェーダー入力用のシェーダーリソースビューを作成する。

            this.D3DスキニングバッファSRView = new SharpDX.Direct3D11.ShaderResourceView(
                d3dDevice,
                this.D3Dスキニングバッファ,  // 構造化バッファ
                new SharpDX.Direct3D11.ShaderResourceViewDescription {
                    Format = SharpDX.DXGI.Format.Unknown,
                    Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer,
                    BufferEx = new SharpDX.Direct3D11.ShaderResourceViewDescription.ExtendedBufferResource {
                        FirstElement = 0,
                        ElementCount = this.入力頂点リスト.Length,
                    },
                } );


            // コンピュートシェーダー出力用の非順序アクセスビューを作成する。

            this.D3D頂点バッファビューUAView = new SharpDX.Direct3D11.UnorderedAccessView(
                d3dDevice,
                this.D3D頂点バッファ,
                new SharpDX.Direct3D11.UnorderedAccessViewDescription {
                    Format = SharpDX.DXGI.Format.R32_Typeless,
                    Dimension = SharpDX.Direct3D11.UnorderedAccessViewDimension.Buffer,
                    Buffer = new SharpDX.Direct3D11.UnorderedAccessViewDescription.BufferResource {
                        FirstElement = 0,
                        ElementCount = VS_INPUT.SizeInBytes * 入力頂点リスト.Length / 4,
                        Flags = SharpDX.Direct3D11.UnorderedAccessViewBufferFlags.Raw,
                    },
                } );

            // 定数バッファを作成する。

            this._D3DBoneTransデータストリーム = new DataStream( this.入力頂点リスト.Length * _D3DBoneTrans.SizeInBytes, canRead: true, canWrite: true );

            this._D3DBoneTrans定数バッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new SharpDX.Direct3D11.BufferDescription {
                    SizeInBytes = 入力頂点リスト.Length * _D3DBoneTrans.SizeInBytes,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                } );

            this._D3DBoneLocalPositionデータストリーム = new DataStream( this.入力頂点リスト.Length * _D3DBoneLocalPosition.SizeInBytes, canRead: true, canWrite: true );

            this._D3DBoneLocalPosition定数バッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new SharpDX.Direct3D11.BufferDescription {
                    SizeInBytes = 入力頂点リスト.Length * _D3DBoneLocalPosition.SizeInBytes,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                } );

            this._D3DBoneQuaternionデータストリーム = new DataStream( this.入力頂点リスト.Length * _D3DBoneQuaternion.SizeInBytes, canRead: true, canWrite: true );

            this._D3DBoneQuaternion定数バッファ = new SharpDX.Direct3D11.Buffer(
                d3dDevice,
                new SharpDX.Direct3D11.BufferDescription {
                    SizeInBytes = 入力頂点リスト.Length * _D3DBoneQuaternion.SizeInBytes,
                    BindFlags = SharpDX.Direct3D11.BindFlags.ConstantBuffer,
                } );
        }

        public void Dispose()
        {
            this._D3DBoneQuaternionデータストリーム?.Dispose();
            this._D3DBoneQuaternionデータストリーム = null;

            this._D3DBoneLocalPositionデータストリーム?.Dispose();
            this._D3DBoneLocalPositionデータストリーム = null;

            this._D3DBoneTransデータストリーム?.Dispose();
            this._D3DBoneTransデータストリーム = null;

            this._D3DBoneQuaternion定数バッファ?.Dispose();
            this._D3DBoneQuaternion定数バッファ = null;

            this._D3DBoneLocalPosition定数バッファ?.Dispose();
            this._D3DBoneLocalPosition定数バッファ = null;

            this._D3DBoneTrans定数バッファ?.Dispose();
            this._D3DBoneTrans定数バッファ = null;

            this.D3D頂点バッファビューUAView?.Dispose();
            this.D3D頂点バッファビューUAView = null;

            this.D3DスキニングバッファSRView?.Dispose();
            this.D3DスキニングバッファSRView = null;

            this.D3D頂点レイアウト?.Dispose();
            this.D3D頂点レイアウト = null;

            this.D3D頂点バッファ?.Dispose();
            this.D3D頂点バッファ = null;

            this.D3Dスキニングバッファ?.Dispose();
            this.D3Dスキニングバッファ = null;

            this._頂点データストリーム?.Dispose();
            this._頂点データストリーム = null;

            D3Dインデックスバッファ?.Dispose();
            D3Dインデックスバッファ = null;

            this.入力頂点リスト = null;
        }

        public void D3Dスキニングバッファを更新する( スキニング skelton, エフェクト effect )
        {
            if( !( D3Dスキニングバッファをリセットする ) )
                return;

            var skinning = ( skelton as PMXスケルトン ) ?? throw new System.NotSupportedException( "PMXバッファ管理クラスでは、スキニングとして PMXスケルトン クラスを指定してください。" );
            var d3dContext = RenderContext.Instance.DeviceManager.D3DDeviceContext;


            // エフェクト変数にボーンの情報を設定する。

            this._D3DBoneTransデータストリーム.WriteRange( skinning.ボーンのモデルポーズ配列 );
            this._D3DBoneTransデータストリーム.Position = 0;
            d3dContext.UpdateSubresource( new DataBox( _D3DBoneTransデータストリーム.DataPointer, 0, 0 ), _D3DBoneTrans定数バッファ, 0 );
            effect.D3DEffect.GetConstantBufferByName( "BoneTransBuffer" ).SetConstantBuffer( this._D3DBoneTrans定数バッファ );

            this._D3DBoneLocalPositionデータストリーム.WriteRange( skinning.ボーンのローカル位置 );
            this._D3DBoneLocalPositionデータストリーム.Position = 0;
            d3dContext.UpdateSubresource( new DataBox( _D3DBoneLocalPositionデータストリーム.DataPointer, 0, 0 ), _D3DBoneLocalPosition定数バッファ, 0 );
            effect.D3DEffect.GetConstantBufferByName( "BoneLocalPositionBuffer" ).SetConstantBuffer( this._D3DBoneLocalPosition定数バッファ );

            this._D3DBoneQuaternionデータストリーム.WriteRange( skinning.ボーンの回転 );
            this._D3DBoneQuaternionデータストリーム.Position = 0;
            d3dContext.UpdateSubresource( new DataBox( _D3DBoneQuaternionデータストリーム.DataPointer, 0, 0 ), _D3DBoneQuaternion定数バッファ, 0 );
            effect.D3DEffect.GetConstantBufferByName( "BoneQuaternionBuffer" ).SetConstantBuffer( this._D3DBoneQuaternion定数バッファ );


            // 現在の入力頂点リストをスキニングバッファに転送する。

            this._頂点データストリーム.WriteRange( 入力頂点リスト );
            this._頂点データストリーム.Position = 0;
            d3dContext.UpdateSubresource( new DataBox( _頂点データストリーム.DataPointer, 0, 0 ), D3Dスキニングバッファ, 0 );


            // 使用するtechniqueを検索する。

            テクニック technique =
                ( from teq in effect.テクニックリスト
                  where
                    teq.テクニックを適用する描画対象 == MMDPass種別.スキニング
                  select teq ).FirstOrDefault();

            if( null != technique )
            {
                // パスを通じてコンピュートシェーダーステートを設定する。

                technique.パスリスト.ElementAt( 0 ).Value.D3DPass.Apply( d3dContext );


                // コンピュートシェーダーでスキニングを実行し、結果を頂点バッファに格納する。

                d3dContext.ComputeShader.SetShaderResource( 0, this.D3DスキニングバッファSRView );
                d3dContext.ComputeShader.SetUnorderedAccessView( 0, this.D3D頂点バッファビューUAView );
                d3dContext.Dispatch( ( 入力頂点リスト.Length / 64 ) + 1, 1, 1 );
            }

            // UAVを外す（このあと頂点シェーダーが使えるように）

            d3dContext.ComputeShader.SetUnorderedAccessView( 0, null );


            #region " （CPUで行ったときのソース）"
            //----------------
            /*
            var boneTrans = skinning.ボーンのモデルポーズ配列;
            var スキニング後の入力頂点リスト = new VS_INPUT[ 入力頂点リスト.Length ];

            for( int i = 0; i < 入力頂点リスト.Length; i++ )
            {
                switch( 入力頂点リスト[ i ].変形方式 )
                {
                    case (uint) 変形方式.BDEF1:
                        #region " *** "
                        //----------------
                        {
                            var 頂点 = 入力頂点リスト[ i ];

                            Matrix bt =
                                boneTrans[ 頂点.BoneIndex1 ];

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                            スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                        }
                        //----------------
                        #endregion
                        break;

                    case (uint) 変形方式.BDEF2:
                        #region " *** "
                        //----------------
                        {
                            var 頂点 = 入力頂点リスト[ i ];

                            Matrix bt =
                                boneTrans[ 頂点.BoneIndex1 ] * 頂点.BoneWeight1 +
                                boneTrans[ 頂点.BoneIndex2 ] * 頂点.BoneWeight2;

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                            スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                        }
                        //----------------
                        #endregion
                        break;

                    case (uint) 変形方式.BDEF4:
                        #region " *** "
                        //----------------
                        {
                            var 頂点 = 入力頂点リスト[ i ];

                            Matrix bt =
                                boneTrans[ 頂点.BoneIndex1 ] * 頂点.BoneWeight1 +
                                boneTrans[ 頂点.BoneIndex2 ] * 頂点.BoneWeight2 +
                                boneTrans[ 頂点.BoneIndex3 ] * 頂点.BoneWeight3 +
                                boneTrans[ 頂点.BoneIndex4 ] * 頂点.BoneWeight4;

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = Vector3.TransformNormal( 頂点.Normal, bt );
                            スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                        }
                        //----------------
                        #endregion
                        break;

                    case (uint) 変形方式.SDEF:
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
                            スキニング後の入力頂点リスト[ i ].Normal.Normalize();
                        }
                        //----------------
                        #endregion
                        break;

                    case (uint) 変形方式.QDEF:
                        #region " *** "
                        //----------------
                        {
                            // ※ QDEFを使ったモデルが見つからないのでテストしてません。あれば教えてください！

                            var 頂点 = 入力頂点リスト[ i ];

                            var dualQuaternion = new DualQuaternion[ 4 ];   // 最大４ボーンまで対応

                            var boneIndexes = new[] { 頂点.BoneIndex1, 頂点.BoneIndex2, 頂点.BoneIndex3, 頂点.BoneIndex4 };
                            var boneWeights = new[] { 頂点.BoneWeight1, 頂点.BoneWeight2, 頂点.BoneWeight3, 頂点.BoneWeight4 };

                            for( int b = 0; b < 4; b++ )
                            {
                                if( boneWeights[ b ] == 0f )
                                {
                                    dualQuaternion[ b ] = DualQuaternion.Zero;  // 未使用
                                }
                                else
                                {
                                    dualQuaternion[ b ] = new DualQuaternion( boneTrans[ boneIndexes[ b ] ] );
                                }
                            }

                            Matrix bt = (
                                dualQuaternion[ 0 ] * boneWeights[ 0 ] +
                                dualQuaternion[ 1 ] * boneWeights[ 1 ] +
                                dualQuaternion[ 2 ] * boneWeights[ 2 ] +
                                dualQuaternion[ 3 ] * boneWeights[ 3 ] ).ToMatrix();

                            if( Matrix.Zero == bt )
                                bt = Matrix.Identity;

                            スキニング後の入力頂点リスト[ i ].Position = Vector4.Transform( 頂点.Position, bt );
                            スキニング後の入力頂点リスト[ i ].Normal = 頂点.Normal;
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
            */
            //----------------
            #endregion

            D3Dスキニングバッファをリセットする = false;
        }


        /// <summary>
        ///     <see cref="入力頂点リスト"/> を <see cref="D3Dスキニングバッファ"/> に書き込む際に使用。
        ///     アンマネージドメモリに確保されるので、最初に一度だけ確保しておく。
        /// </summary>
        private DataStream _頂点データストリーム;


        // DefaultShader.fx のスキニングシェーダーでは、以下の３つのボーン情報を使用するので定数バッファとして準備する。

        protected struct _D3DBoneTrans  // private じゃなく protected なのは warning CS0649 封じのため
        {
            /// <summary>
            ///     <see cref="MikuMikuFlex.ボーン.ボーン.モデルポーズ行列"/> が格納される。
            /// </summary>
            public Matrix boneTrans;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( System.Runtime.InteropServices.Marshal.SizeOf( typeof( _D3DBoneTrans ) ) ) / 16 + 1 ) * 16;
        }
        private DataStream _D3DBoneTransデータストリーム;
        private SharpDX.Direct3D11.Buffer _D3DBoneTrans定数バッファ;

        protected struct _D3DBoneLocalPosition  // private じゃなく protected なのは warning CS0649 封じのため
        {
            /// <summary>
            ///     <see cref="MikuMikuFlex.ボーン.ボーン.移動"/> が格納される。
            /// </summary>
            public Vector3 boneLocalPosition;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( System.Runtime.InteropServices.Marshal.SizeOf( typeof( _D3DBoneLocalPosition ) ) ) / 16 + 1 ) * 16;
        }
        private DataStream _D3DBoneLocalPositionデータストリーム;
        private SharpDX.Direct3D11.Buffer _D3DBoneLocalPosition定数バッファ;

        protected struct _D3DBoneQuaternion // private じゃなく protected なのは warning CS0649 封じのため
        {
            /// <summary>
            ///     <see cref="MikuMikuFlex.ボーン.ボーン.回転"/> が格納される。
            /// </summary>
            public Vector4 boneQuaternion;

            /// <summary>
            ///     構造体の大きさ[byte] 。定数バッファで使う場合は、常に16の倍数であること。
            /// </summary>
            public static int SizeInBytes
                => ( ( System.Runtime.InteropServices.Marshal.SizeOf( typeof( _D3DBoneQuaternion ) ) ) / 16 + 1 ) * 16;
        }
        private DataStream _D3DBoneQuaternionデータストリーム;
        private SharpDX.Direct3D11.Buffer _D3DBoneQuaternion定数バッファ;


        private void _頂点データを頂点レイアウトリストに追加する( 頂点 頂点データ, List<CS_INPUT> 頂点レイアウトリスト )
        {
            var layout = new CS_INPUT() {
                Position = new Vector4( 頂点データ.位置, 1f ),
                Normal = 頂点データ.法線,
                UV = 頂点データ.UV,
                Index = (uint) 頂点レイアウトリスト.Count,    // 現在の要素数 ＝ List<>内でのこの要素のインデックス番号
                EdgeWeight = 頂点データ.エッジ倍率,
                変形方式 = (uint) 頂点データ.ウェイト変形方式,
            };

            switch( 頂点データ.ウェイト変形方式 )
            {
                case 変形方式.BDEF1:
                    {
                        var v = (BDEF1) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.boneReferenceIndex < 0 ) ? 0 : v.boneReferenceIndex );
                        layout.BoneIndex2 = 0;
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.boneReferenceIndex < 0 ) ? 0.0f : 1.0f;
                        layout.BoneWeight2 = 0.0f;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                    }
                    break;

                case 変形方式.BDEF2:
                    {
                        var v = (BDEF2) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Bone1Weight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Bone2Weight;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                    }
                    break;

                case 変形方式.SDEF:
                    {
                        var v = (SDEF) 頂点データ.ボーンウェイト;
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = 0;
                        layout.BoneIndex4 = 0;
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Bone1Weight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Bone2Weight;
                        layout.BoneWeight3 = 0.0f;
                        layout.BoneWeight4 = 0.0f;
                        layout.Sdef_C = new Vector4( v.SDEF_C, 1f );
                        layout.SdefR0 = v.SDEF_R0;
                        layout.SdefR1 = v.SDEF_R1;
                    }
                    break;

                case 変形方式.BDEF4:
                case 変形方式.QDEF:
                    {
                        var v = (BDEF4) 頂点データ.ボーンウェイト;
                        float sumWeight = v.Weights.X + v.Weights.Y + v.Weights.Z + v.Weights.W;
                        if( !( 0.99999f < sumWeight && sumWeight < 1.00001f ) && v.Weights.W == 0f )
                        {
                            // sumWeight ≒ 1.0 かつ W ＝ 0 なら、W にあまり全部を足す。
                            v.Weights.W = 1.0f - v.Weights.X - v.Weights.Y - v.Weights.Z;
                            sumWeight = 1.0f;
                        }
                        layout.BoneIndex1 = (uint) ( ( v.Bone1ReferenceIndex < 0 ) ? 0 : v.Bone1ReferenceIndex );
                        layout.BoneIndex2 = (uint) ( ( v.Bone2ReferenceIndex < 0 ) ? 0 : v.Bone2ReferenceIndex );
                        layout.BoneIndex3 = (uint) ( ( v.Bone3ReferenceIndex < 0 ) ? 0 : v.Bone3ReferenceIndex );
                        layout.BoneIndex4 = (uint) ( ( v.Bone4ReferenceIndex < 0 ) ? 0 : v.Bone4ReferenceIndex );
                        layout.BoneWeight1 = ( v.Bone1ReferenceIndex < 0 ) ? 0.0f : v.Weights.X / sumWeight;
                        layout.BoneWeight2 = ( v.Bone2ReferenceIndex < 0 ) ? 0.0f : v.Weights.Y / sumWeight;
                        layout.BoneWeight3 = ( v.Bone3ReferenceIndex < 0 ) ? 0.0f : v.Weights.Z / sumWeight;
                        layout.BoneWeight4 = ( v.Bone4ReferenceIndex < 0 ) ? 0.0f : v.Weights.W / sumWeight;
                    }
                    break;
            }

            頂点レイアウトリスト.Add( ( layout ) );
        }
    }
}
