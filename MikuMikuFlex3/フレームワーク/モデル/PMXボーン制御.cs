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
    class PMXボーン制御
    {

        // 基本情報


        public PMXFormat.ボーン PMXFボーン { get; protected set; }

        public int ボーンインデックス { get; protected set; }

        public PMXボーン制御 親ボーン { get; protected set; }

        public List<PMXボーン制御> 子ボーンリスト { get; protected set; }



        // 動的情報（入力）


        public Vector3 ローカル位置 { get; protected set; }

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



        // 動的情報（出力）


        public Matrix モデルポーズ行列 { get; protected set; }

        public Matrix ローカルポーズ行列 { get; protected set; }



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
        }

        public void 子ボーンリストを構築する( PMXボーン制御[] 全ボーン )
        {
            for( int i = 0; i < 全ボーン.Length; i++ )
            {
                if( 全ボーン[ i ].PMXFボーン.親ボーンのインデックス == this.ボーンインデックス )
                {
                    全ボーン[ i ].親ボーン = this;
                    this.子ボーンリスト.Add( 全ボーン[ i ] );
                }
            }
        }



        // 更新と出力


        public void 更新する( Matrix[] モデルポーズ配列, Vector3[] ローカル位置配列, Vector4[] 回転配列 )
        {
            // ポーズ計算。

            this.ローカルポーズ行列 =
                Matrix.Translation( -this.ローカル位置 ) * // 原点に戻って
                Matrix.RotationQuaternion( this.回転 ) *   // 回転して
                Matrix.Translation( this.移動 ) *          // 平行移動したのち
                Matrix.Translation( this.ローカル位置 );   // 元の位置に戻す

            this.モデルポーズ行列 =
                this.ローカルポーズ行列 *
                ( this.親ボーン?.モデルポーズ行列 ?? Matrix.Identity );    // 親ボーンがあるなら親ボーンのモデルポーズを反映。


            // 計算結果を出力。

            モデルポーズ配列[ this.ボーンインデックス ] = this.モデルポーズ行列;
            モデルポーズ配列[ this.ボーンインデックス ].Transpose(); // エフェクトを介さない場合は自分で転置する必要がある。
            ローカル位置配列[ this.ボーンインデックス ] = this.ローカル位置;
            回転配列[ this.ボーンインデックス ] = new Vector4( this.回転.ToArray() );  // Quaternion → Vector4


            // すべての子ボーンについても更新。

            foreach( var 子ボーン in this.子ボーンリスト )
                子ボーン.更新する( モデルポーズ配列, ローカル位置配列, 回転配列 );
        }



        // private


        private Quaternion _回転;
    }
}
