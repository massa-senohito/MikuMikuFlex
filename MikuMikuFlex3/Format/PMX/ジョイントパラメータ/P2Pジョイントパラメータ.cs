using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     点結合ジョイント。
    /// </summary>
    public class P2PJointParameters : JointParameters
    {
        public int RelatedRigidBodyAのインデックス { get; private set; }

        public int RelatedRigidBodyBのインデックス { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotationrad { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void Read( Stream fs, Header header )
        {
            this.RelatedRigidBodyAのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.RelatedRigidBodyBのインデックス = ParserHelper.get_Index( fs, header.RigidBodyIndexSize );
            this.Position = ParserHelper.get_Float3( fs );
            this.Rotationrad = ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );   // 使わないフィールドをスキップ
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
            ParserHelper.get_Float3( fs );
        }
    }
}
