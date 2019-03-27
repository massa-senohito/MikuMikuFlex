using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    class PMXボーン
    {
        public PMXFormat.ボーン PMXFボーン { get; protected set; }

        public int ボーンインデックス { get; protected set; }

        public PMXボーン 親ボーン { get; protected set; }

        public List<PMXボーン> 子ボーンリスト { get; protected set; }

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

        public Matrix モデルポーズ行列 { get; protected set; }

        public Matrix ローカルポーズ行列 { get; protected set; }


        public PMXボーン( PMXFormat.ボーン bone, int index )
        {
            this.PMXFボーン = bone;
            this.ボーンインデックス = index;
            this.親ボーン = null;
            this.子ボーンリスト = new List<PMXボーン>();
            this.ローカル位置 = bone.位置;
            this.移動 = Vector3.Zero;
            this.回転 = Quaternion.Identity;
            this.モデルポーズ行列 = Matrix.Identity;
            this.ローカルポーズ行列 = Matrix.Identity;
        }

        public void 子ボーンリストを構築する( PMXボーン[] 全ボーン )
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

        public void モデルポーズを更新する()
        {
            this.ローカルポーズ行列 =
                Matrix.Translation( -this.ローカル位置 ) * // 原点に戻って
                Matrix.RotationQuaternion( this.回転 ) *   // 回転して
                Matrix.Translation( this.移動 ) *          // 平行移動したのち
                Matrix.Translation( this.ローカル位置 );   // 元の位置に戻す

            this.モデルポーズ行列 =
                this.ローカルポーズ行列 *
                ( this.親ボーン?.モデルポーズ行列 ?? Matrix.Identity );    // 親ボーンがあるなら親ボーンのモデルポーズを反映。

            // すべての子ボーンについて更新。
            foreach( var 子ボーン in this.子ボーンリスト )
                子ボーン.モデルポーズを更新する();
        }


        private Quaternion _回転;
    }
}
