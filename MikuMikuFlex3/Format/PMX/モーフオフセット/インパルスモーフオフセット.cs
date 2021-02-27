using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class ImpulseMorphOffset : MorphOffset
    {
        public int RigidBodyIndex { get; private set; }

        /// <summary>
        ///     0:OFF, 1:ON
        /// </summary>
        public byte LocalFlag { get; private set; }

        /// <summary>
        ///     すべて 0 の場合は"停止制御"として特殊化
        /// </summary>
        public Vector3 MovingSpeed { get; private set; }

        /// <summary>
        ///     すべて 0 の場合は"停止制御"として特殊化
        /// </summary>
        public Vector3 RotationTorque { get; private set; }


        public ImpulseMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ImpulseMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Impulse;
            this.RigidBodyIndex = ParserHelper.get_Index( st, header.RigidBodyIndexSize );
            this.LocalFlag = ParserHelper.get_Byte( st );
            this.MovingSpeed = ParserHelper.get_Float3( st );
            this.RotationTorque = ParserHelper.get_Float3( st );
        }
    }
}
