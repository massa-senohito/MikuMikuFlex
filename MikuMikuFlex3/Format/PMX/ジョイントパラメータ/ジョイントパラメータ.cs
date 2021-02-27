using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public abstract class JointParameters
    {
        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static JointParameters Read( Stream fs, Header header, JointType type )
        {
            switch( type )
            {
                // PMX 2.0
                case JointType.WithSpring6DOF:
                    var sp6 = new WithSpring6DOFJointParameters();
                    sp6.Read( fs, header );
                    return sp6;

                // PMX 2.1
                case JointType.Basic6DOF:
                case JointType.P2P:
                case JointType.ConeRotation:
                case JointType.Slider:
                case JointType.Hinge:
                    throw new NotSupportedException( "PMX2.1 以降には未対応です。" );     // todo: PMX2.1 拡張のジョイントの実装

                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        ///     指定されたストリームから読み込む。（派生クラスで実装のこと。）
        /// </summary>
        internal abstract void Read( Stream fs, Header header );
    }
}
