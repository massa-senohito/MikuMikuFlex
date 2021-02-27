using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class BoneMorphOffset : MorphOffset
    {
        public int BoneIndex { get; private set; }

        public Vector3 AmountOfMovement { get; private set; }

        /// <summary>
        ///     クォータニオン(x,y,z,w)
        /// </summary>
        public Vector4 RotationAmount { get; private set; }


        public BoneMorphOffset()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal BoneMorphOffset( Stream st, Header header )
        {
            this.MorphType = MorphType.Bourne;
            this.BoneIndex = ParserHelper.get_Index( st, header.BoneIndexSize );
            this.AmountOfMovement = ParserHelper.get_Float3( st );
            this.RotationAmount = ParserHelper.get_Float4( st );
        }
    }
}
