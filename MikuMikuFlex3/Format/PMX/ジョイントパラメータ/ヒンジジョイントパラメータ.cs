using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     軸回転ジョイント。
    /// </summary>
    public class HingeJointParameters : JointParameters
    {
        public int RelatedRigidBodyAのインデックス { get; private set; }

        public int RelatedRigidBodyBのインデックス { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotationrad { get; private set; }

        /// <summary>
        ///     RotationLimit-下限 - X
        /// </summary>
        public float Low { get; private set; }

        /// <summary>
        ///     RotationLimit-上限 - X
        /// </summary>
        public float High { get; private set; }

        /// <summary>
        ///     バネ定数-Move - X
        /// </summary>
        public float SoftNess { get; private set; }

        /// <summary>
        ///     バネ定数-Move - Y
        /// </summary>
        public float BiasFactor { get; private set; }

        /// <summary>
        ///     バネ定数-Move - Z
        /// </summary>
        public float RelaxationFactor { get; private set; }

        /// <summary>
        ///     バネ定数-Rotation - X  | 回転モーター有効 - 0:OFF 1:ON
        /// </summary>
        public bool MotorEnabled { get; private set; }

        /// <summary>
        ///     バネ定数-Rotation - Y
        ///     ※回転モーター有効時
        /// </summary>
        public float TargetVelocity { get; private set; }

        /// <summary>
        ///     バネ定数-Rotation - Z
        ///     ※回転モーター有効時
        /// </summary>
        public float MaxMotorImpulse { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void Read( Stream fs, Header header )
        {
            this.RelatedRigidBodyAのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.RelatedRigidBodyBのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.Position = ParserHelper.get_Float3( fs );
            this.Rotationrad = ParserHelper.get_Float3( fs );

            Vector3 moveLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 moveLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 springMoveCoefficient = ParserHelper.get_Float3( fs );
            Vector3 springRotationCoefficient = ParserHelper.get_Float3( fs );

            this.Low = rotationLimitationMin.X;
            this.High = rotationLimitationMax.X;
            this.SoftNess = springMoveCoefficient.X;
            this.BiasFactor = springMoveCoefficient.Y;
            this.RelaxationFactor = springMoveCoefficient.Z;
            this.MotorEnabled = Math.Abs( springRotationCoefficient.X - 1 ) < 0.3f;   // floatなので誤差防止のため(こんなに大きくいらないけど。)
            this.TargetVelocity = springRotationCoefficient.Y;
            this.MaxMotorImpulse = springRotationCoefficient.Z;
        }
    }
}
