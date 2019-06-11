using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.ボーン"/> に追加情報を付与するクラス。
    /// </summary>
    public class PMXボーン制御 : IDisposable
    {

        // 基本情報


        public string 名前 => this.PMXFボーン.ボーン名;

        public string 名前_英 => this.PMXFボーン.ボーン名_英;

        public PMXボーン制御 親ボーン { get; protected set; }

        public List<PMXボーン制御> 子ボーンリスト { get; protected set; }

        internal PMXFormat.ボーン PMXFボーン { get; private protected set; }

        internal int ボーンインデックス { get; private protected set; }

        internal PMXボーン制御 IKターゲットボーン { get; private protected set; }

        internal IKリンク[] IKリンクリスト { get; private protected set; }

        internal int 変形階層 { get; set; }



        // 動的情報（入力）


        public Vector3 ローカル位置 { get; set; }

        public Vector3 移動 { get; set; }

        public Quaternion 回転
        {
            get { return _回転; }
            set
            {
                _回転 = value;
                _回転.Normalize();
            }
        }

        public アニメ変数<Vector3> アニメ変数_移動 { get; protected set; }

        public アニメ変数<Quaternion> アニメ変数_回転 { get; protected set; }



        // 動的情報（出力）


        internal Matrix モデルポーズ行列 { get; private protected set; }

        internal Matrix ローカルポーズ行列 { get; private protected set; }



        // 生成と終了


        public PMXボーン制御( PMXFormat.ボーン bone, int index )
        {
            this.PMXFボーン = bone;
            this.ボーンインデックス = index;
            this.親ボーン = null;
            this.子ボーンリスト = new List<PMXボーン制御>();
            this.ローカル位置 = bone.位置;
            this.移動 = Vector3.Zero;
            this.回転 = Quaternion.Identity;
            this.モデルポーズ行列 = Matrix.Identity;
            this.ローカルポーズ行列 = Matrix.Identity;
            this.アニメ変数_移動 = new アニメ変数<Vector3>( Vector3.Zero );
            this.アニメ変数_回転 = new アニメ変数<Quaternion>( Quaternion.Identity );
        }

        internal void 読み込み後の処理を行う( PMXボーン制御[] 全ボーン )
        {
            // 子ボーンとの階層化

            for( int i = 0; i < 全ボーン.Length; i++ )
            {
                if( 全ボーン[ i ].PMXFボーン.親ボーンのインデックス == this.ボーンインデックス )
                {
                    全ボーン[ i ].親ボーン = this;
                    this.子ボーンリスト.Add( 全ボーン[ i ] );
                }
            }


            // IK

            if( this.PMXFボーン.IKボーンである )
            {
                this.IKターゲットボーン = 全ボーン[ this.PMXFボーン.IKターゲットボーンインデックス ];

                this.IKリンクリスト = new IKリンク[ this.PMXFボーン.IKリンクリスト.Count ];
                for( int i = 0; i < this.PMXFボーン.IKリンクリスト.Count; i++ )
                {
                    this.IKリンクリスト[ i ] = new IKリンク( this.PMXFボーン.IKリンクリスト[ i ] ) {
                        IKリンクボーン = 全ボーン[ this.PMXFボーン.IKリンクリスト[ i ].リンクボーンのボーンインデックス ],
                    };
                }
            }
        }

        public virtual void Dispose()
        {
            this.PMXFボーン = null;
            this.親ボーン = null;
            this.子ボーンリスト = null;
        }



        // 更新と出力


        internal void ボーンモーションを適用する( double 現在時刻sec )
        {
            this.移動 += this.アニメ変数_移動.更新する( 現在時刻sec );
            this.回転 *= this.アニメ変数_回転.更新する( 現在時刻sec );
        }

        internal void モデルポーズを計算する()
        {
            // ポーズ計算。

            this.ローカルポーズ行列 =
                Matrix.Translation( -this.ローカル位置 ) *    // 原点に戻って
                Matrix.RotationQuaternion( this.回転 ) *      // 回転して
                Matrix.Translation( this.移動 ) *             // 平行移動したのち
                Matrix.Translation( this.ローカル位置 );      // 元の位置に戻す

            this.モデルポーズ行列 =
                this.ローカルポーズ行列 *
                ( this.親ボーン?.モデルポーズ行列 ?? Matrix.Identity );    // 親ボーンがあるなら親ボーンのモデルポーズを反映。


            // すべての子ボーンについても更新。

            foreach( var 子ボーン in this.子ボーンリスト )
                子ボーン.モデルポーズを計算する();
        }

        internal void 状態を確定する( Matrix[] モデルポーズ配列, Vector4[] ローカル位置配列, Vector4[] 回転配列 )
        {
            モデルポーズ配列[ this.ボーンインデックス ] = this.モデルポーズ行列;
            モデルポーズ配列[ this.ボーンインデックス ].Transpose(); // エフェクトを介さない場合は自分で転置する必要がある。
            ローカル位置配列[ this.ボーンインデックス ] = new Vector4( this.ローカル位置, 0f );
            回転配列[ this.ボーンインデックス ] = new Vector4( this.回転.ToArray() );  // Quaternion → Vector4

            // すべての子ボーンについても更新。

            foreach( var 子ボーン in this.子ボーンリスト )
                子ボーン.状態を確定する( モデルポーズ配列, ローカル位置配列, 回転配列 );
        }



        // private


        private Quaternion _回転;


        public class IKリンク
        {
            public PMXボーン制御 IKリンクボーン;

            public bool 回転制限がある => this._ikLink.角度制限あり;

            public Vector3 最大回転量 { get; }

            public Vector3 最小回転量 { get; }


            public IKリンク( PMXFormat.ボーン.IKリンク ikLink )
            {
                this._ikLink = ikLink;

                // minとmaxを正しく読み込む
                Vector3 maxVec = ikLink.角度制限の上限rad;
                Vector3 minVec = ikLink.角度制限の下限rad; 
                this.最小回転量 = new Vector3( Math.Min( maxVec.X, minVec.X ), Math.Min( maxVec.Y, minVec.Y ), Math.Min( maxVec.Z, minVec.Z ) );
                this.最大回転量 = new Vector3( Math.Max( maxVec.X, minVec.X ), Math.Max( maxVec.Y, minVec.Y ), Math.Max( maxVec.Z, minVec.Z ) );
                this.最小回転量 = Vector3.Clamp( 最小回転量, CGHelper.オイラー角の最小値, CGHelper.オイラー角の最大値 );
                this.最大回転量 = Vector3.Clamp( 最大回転量, CGHelper.オイラー角の最小値, CGHelper.オイラー角の最大値 );
            }


            private PMXFormat.ボーン.IKリンク _ikLink;
        }
    }
}
