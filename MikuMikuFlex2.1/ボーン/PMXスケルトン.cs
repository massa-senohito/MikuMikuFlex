using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using MMDFileParser.PMXModelParser;
using Debug =System.Diagnostics.Debug;

namespace MikuMikuFlex
{
    /// <summary>
    ///     モデルからボーンだけを抽出して管理するクラス。
    /// </summary>
	public class PMXスケルトン : スキニング
	{
		public Matrix[] ボーンのモデルポーズ配列;
        public Vector3[] ボーンのローカル位置;
        public Vector4[] ボーンの回転;

        public event EventHandler スケルトンが更新された = delegate { };

		/// <summary>
		///     FK, IK を行うインターフェースのリスト
		/// </summary>
		public List<変形更新> 変形更新リスト { get; private set; }

		/// <summary>
		///     ボーンのルート（ルートが2つ以上の場合があるのでListを使用）
		/// </summary>
		public List<PMXボーン> ボーンのルート = new List<PMXボーン>();

        /// <summary>
        ///     ボーン(インデックス順)
        /// </summary>
        public PMXボーン[] ボーン配列 { get; set; }

        public Dictionary<string, PMXボーン> ボーンマップ { get; private set; }

        public List<PMXボーン> IKボーンリスト { get; set; }


        public PMXスケルトン( PMXモデル model )
		{
			// ボーンの数だけ初期化
			ボーン配列 = new PMXボーン[ model.ボーンリスト.Count ];
            ボーンのモデルポーズ配列 = new Matrix[ model.ボーンリスト.Count ];
            ボーンのローカル位置 = new Vector3[ model.ボーンリスト.Count ];
            ボーンの回転 = new Vector4[ model.ボーンリスト.Count ];
            IKボーンリスト = new List<PMXボーン>();
			
            // ボーンを読み込む
			_LoadBones( model );

            // ボーンマップ作製
			ボーンマップ = new Dictionary<string, PMXボーン>();
			foreach( var bone in ボーン配列 )
			{
				if( ボーンマップ.ContainsKey( bone.ボーン名 ) )
				{
					int i = 0;
					do
					{
						i++;
					} while( ボーンマップ.ContainsKey( bone.ボーン名 + i.ToString() ) );
					ボーンマップ.Add( bone.ボーン名 + i.ToString(), bone );
					Debug.WriteLine( "ボーン名{0}は重複しています。自動的にボーン名{1}と読み替えられました。", bone.ボーン名, bone.ボーン名 + i );
				}
				else
					ボーンマップ.Add( bone.ボーン名, bone );
			}

            // 変形更新プロバイダリスト作成

            変形更新リスト = new List<変形更新>();
            変形更新リスト.Add( new CCDによるIK変形更新( new WeakReference<List<PMXボーン>>( IKボーンリスト ) ) );
            変形更新リスト.Add( new 親付与によるFK変形更新( ボーン配列 ) );

            if( ボーン配列.Length > 768 )
			{
				throw new InvalidOperationException( "MMFでは現在768以上のボーンを持つモデルについてサポートしていません。\nただし、Resource\\Shader\\DefaultShader.fx内のボーン変形行列の配列float4x4 BoneTrans[512]:BONETRANS;の要素数を拡張しこの部分をコメントアウトすれば暫定的に利用することができるかもしれません。" );
			}
		}

        public virtual void Dispose()
        {
        }

        public void エフェクトを適用する( Effect d3dEffect )
		{
        }

        public virtual void 更新する()
		{
            this.ボーンのすべての変形をリセットする();

            現在の回転行列に基づいてルートボーンからモデルポーズを再計算する();

            foreach( 変形更新 kinematicsProvider in 変形更新リスト )
			{
				if( kinematicsProvider.変形を更新する() )  // すぐ行列を更新するなら true
				{
					現在の回転行列に基づいてルートボーンからモデルポーズを再計算する();
                    //ボーンのすべての変形をリセットする();// BUG これなんでいれたっけ？
                }
            }
            foreach( var pmxBone in ボーンのルート )
            {
            	pmxBone.モデルポーズを更新する();
            }

            スケルトンが更新された?.Invoke( this, new EventArgs() );

            for( int i = 0; i < ボーン配列.Length; i++ )
            {
                int index = ボーン配列[ i ].ボーンインデックス;

                ボーンのモデルポーズ配列[ index ] = ボーン配列[ i ].モデルポーズ行列;
                ボーンのモデルポーズ配列[ index ].Transpose();  // SharpDX.Matrix は行優先だが HLSL の既定は列優先

                ボーンのローカル位置[ index ] = ボーン配列[ i ].ローカル位置;

                ボーンの回転[ index ] = new Vector4( ボーン配列[ i ].回転.ToArray() );
            }
        }

		public void ボーンのすべての変形をリセットする()
		{
            // すべての回転・移動を元に戻す。
            foreach( PMXボーン item in ボーン配列 )
			{
				item.回転 = Quaternion.Identity;
				item.移動 = Vector3.Zero;
			}
		}


        protected void 現在の回転行列に基づいてルートボーンからモデルポーズを再計算する()
        {
            foreach( var root in ボーンのルート )
            {
                root.モデルポーズを更新する();
            }
        }


        private void _LoadBones( PMXモデル model )
		{
			for( int i = 0; i < model.ボーンリスト.Count; i++ )
			{
				if( model.ボーンリスト[ i ].親ボーンのインデックス == -1 )
				{
					ボーンのルート.Add( new PMXボーン( model.ボーンリスト, i, 0, this ) );
				}
			}

            var comparison = new Comparison<PMXボーン>( ( x, y ) => {

                // 後であればあるほどスコアが大きくなるように計算する

                int xScore = 0;
                int yScore = 0;
                int BoneCount = model.ボーンリスト.Count;

                if( x.変形階層の物理前後 == 変形階層の物理前後指定種別.物理演算後 )
                {
                    xScore += BoneCount * BoneCount;
                }
                if( y.変形階層の物理前後 == 変形階層の物理前後指定種別.物理演算後 )
                {
                    yScore += BoneCount * BoneCount;
                }
                xScore += BoneCount * x.変形階層;
                yScore += BoneCount * y.変形階層;
                xScore += x.ボーンインデックス;
                yScore += y.ボーンインデックス;
                return xScore - yScore;

            } );

			IKボーンリスト.Sort( comparison );
			ボーンのルート.Sort( comparison );
		}
	}
}
