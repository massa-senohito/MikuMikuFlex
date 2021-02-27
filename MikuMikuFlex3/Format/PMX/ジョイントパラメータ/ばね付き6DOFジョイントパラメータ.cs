using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class WithSpring6DOFJointParameters : JointParameters
    {
        public int RelatedRigidBodyAのインデックス { get; private set; }

        public int RelatedRigidBodyBのインデックス { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotationrad { get; private set; }

        public Vector3 LowerLimitOfMovementLimit { get; private set; }

        public Vector3 UpperLimitOfMovementLimit { get; private set; }

        public Vector3 LowerLimitOfRotationLimitrad { get; private set; }

        public Vector3 UpperLimitOfRotationLimitrad { get; private set; }

        public Vector3 SpringMovementConstant { get; private set; }

        public Vector3 SpringRotationConstant { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void Read( Stream fs, Header header )
        {
            this.RelatedRigidBodyAのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.RelatedRigidBodyBのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.Position = ParserHelper.get_Float3( fs );
            this.Rotationrad = ParserHelper.get_Float3( fs );
            this.LowerLimitOfMovementLimit = ParserHelper.get_Float3( fs );
            this.UpperLimitOfMovementLimit = ParserHelper.get_Float3( fs );
            this.LowerLimitOfRotationLimitrad = ParserHelper.get_Float3( fs );
            this.UpperLimitOfRotationLimitrad = ParserHelper.get_Float3( fs );
            this.SpringMovementConstant = ParserHelper.get_Float3( fs );
            this.SpringRotationConstant = ParserHelper.get_Float3( fs );
        }
    }
}
