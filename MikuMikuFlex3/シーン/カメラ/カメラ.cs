using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     ごく単純に、行列やベクトルなどの生値で表現されるカメラ。
    /// </summary>
    public class カメラ
    {
        public virtual Matrix ビュー変換行列 { get; protected set; } = Matrix.Identity;

        public virtual Matrix 射影変換行列 { get; protected set; } = Matrix.Identity;

        public virtual Vector3 カメラ位置 { get; protected set; } = Vector3.Zero;



        // 更新


        public virtual void 状態を設定する( Matrix viewMatrix, Matrix projMatrix, Vector3 cameraPosLH )
        {
            this.ビュー変換行列 = viewMatrix;
            this.射影変換行列 = projMatrix;
            this.カメラ位置 = cameraPosLH;
        }

        public virtual void 更新する( double 現在時刻sec )
        {
        }
    }
}
