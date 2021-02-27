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
    public class Camera
    {
        public virtual Matrix ViewTransformationMatrix { get; protected set; } = Matrix.Identity;

        public virtual Matrix HomographicTransformationMatrix { get; protected set; } = Matrix.Identity;

        public virtual Vector3 CameraPosition { get; protected set; } = Vector3.Zero;



        // 更新


        public virtual void SetTheState( Matrix viewMatrix, Matrix projMatrix, Vector3 cameraPosLH )
        {
            this.ViewTransformationMatrix = viewMatrix;
            this.HomographicTransformationMatrix = projMatrix;
            this.CameraPosition = cameraPosLH;
        }

        public virtual void Update( double CurrentTimesec )
        {
        }
    }
}
