using System;
using System.Collections.Generic;
using MMDFileParser.PMXModelParser;
using SharpDX;

namespace MikuMikuFlex
{
	/// <summary>
	///     PMX用のボーン実装
	/// </summary>
	public class PMXボーン : ボーン
	{
		public int ボーンインデックス;

		public string ボーン名;

		public List<PMXボーン> 子ボーンリスト = new List<PMXボーン>();

		public Vector3 DefaultLocalX;

        public Vector3 DefaultLocalY;

        public Vector3 DefaultLocalZ;

		public PMXボーン 親ボーン;

		public Vector3 ローカル位置;

		public Vector3 移動 { get; set; }

		public bool ローカル軸あり;

        public Quaternion 回転
        {
            get { return _回転行列; }
            set
            {
                _回転行列 = value;
                _回転行列.Normalize();
            }
        }

        public int 変形階層 { get; private set; }

        public 変形階層の物理前後指定種別 変形階層の物理前後 { get; private set; }

        public Vector3 LocalX
            => Vector3.TransformNormal( DefaultLocalX, Matrix.RotationQuaternion( 回転 ) );

        public Vector3 LocalY
            => Vector3.TransformNormal( DefaultLocalY, Matrix.RotationQuaternion( 回転 ) );

        public Vector3 LocalZ
            => Vector3.TransformNormal( DefaultLocalZ, Matrix.RotationQuaternion( 回転 ) );

        public Matrix ローカルポーズ行列 { get; set; }

        public Matrix モデルポーズ行列 { get; private set; }


        public PMXボーン( List<MMDFileParser.PMXModelParser.ボーン> bones, int index, int layer, スキニング skinning )
		{
            var me = bones[ index ]; //このボーン

            _スキニング = skinning;
            _スキニング.ボーン配列[ index ] = this;
			ボーンインデックス = index;
			ローカル位置 = me.位置;
			ボーン名 = me.ボーン名;
			ローカル軸あり = me.ローカル軸あり;
            変形階層 = layer;
            変形階層の物理前後 = me.物理後変形である ? 変形階層の物理前後指定種別.物理演算後 : 変形階層の物理前後指定種別.物理演算前;

			if( ローカル軸あり )
			{
				DefaultLocalX = me.ローカル軸のX軸の方向ベクトル;
				DefaultLocalY = Vector3.Cross( me.ローカル軸のZ軸の方向ベクトル, DefaultLocalX );
				DefaultLocalZ = Vector3.Cross( DefaultLocalX, DefaultLocalY );
			}

            if( me.IKボーンである )
			{
				skinning.IKボーンリスト.Add( this );

                // IK関連の情報をボーン(me)から取得

                IKボーンである = true;
				IK単位角 = me.IK単位角rad;
				_IKターゲットボーンインデックス = me.IKターゲットボーンインデックス;
				IK演算のLoop回数 = me.IKループ回数;
                foreach( MMDFileParser.PMXModelParser.ボーン.IKリンク ikLink in me.IKリンクリスト )
                    this.IKリンクリスト.Add( new IKリンク( new WeakReference<スキニング>( skinning ), ikLink ) );
			}

			回転付与される = me.回転付与される;
			移動付与される = me.移動付与される;

			if( me.付与親ボーンインデックス == -1 )
			{
                回転付与される = false;
                移動付与される = false;
			}

			if( 移動付与される || 回転付与される )
			{
				付与親ボーンインデックス = me.付与親ボーンインデックス;
				付与率 = me.付与率;
			}
			else
			{
				付与親ボーンインデックス = -1;

            }

			for( int i = 0; i < bones.Count; i++ )
			{
                MMDFileParser.PMXModelParser.ボーン bone = bones[ i ];

				if( bone.親ボーンのインデックス == index )
				{
					var child = new PMXボーン( bones, i, layer + 1, skinning );

					子ボーンを追加する( child );
				}
			}
		}


		// IK 関連

        public int IK演算のLoop回数;

		public float IK単位角;

		public List<IKリンク> IKリンクリスト = new List<IKリンク>();

		public bool IKボーンである = false;

		public PMXボーン IKターゲットボーン
			=> _スキニング.ボーン配列[ _IKターゲットボーンインデックス ];

        private readonly int _IKターゲットボーンインデックス;


        // 付与関連

        public bool 移動付与される { get; private set; }

		public bool 回転付与される { get; private set; }

		public int 付与親ボーンインデックス { get; private set; }

		public float 付与率 { get; private set; }

        // その他

		public void 子ボーンを追加する( PMXボーン child )
		{
			子ボーンリスト.Add( child );
			child.親ボーン = this;
		}

		public void モデルポーズを更新する()
		{
			ローカルポーズ行列 = 
                Matrix.Translation( -ローカル位置 ) * // 原点に戻って
                Matrix.RotationQuaternion( 回転 ) *   // 回転や
                Matrix.Translation( 移動 ) *          // 平行移動したのち
                Matrix.Translation( ローカル位置 );   // 元の位置に戻す

            モデルポーズ行列 = 
                ローカルポーズ行列 *
                ( 親ボーン?.モデルポーズ行列 ?? Matrix.Identity );    // 親ボーンがあるなら親ボーンのモデルポーズを反映。

            // すべての子ボーンについて更新。
            foreach( var 子ボーン in 子ボーンリスト )
				子ボーン.モデルポーズを更新する();
		}



        private readonly スキニング _スキニング;

        private Quaternion _回転行列;
    }
}
