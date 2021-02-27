using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class RigidBody
    {
        public string RigidBodyName { get; private set; }

        public string RigidBodyName_English { get; private set; }

        /// <summary>
        ///     関連なしの場合は -1
        /// </summary>
        public int RelatedBoneIndex { get; private set; }

        public byte Group { get; private set; }

        public ushort NonCollisionGroupFlag { get; private set; }

        public RigidBodyShape Shape { get; private set; }

        public Vector3 Size { get; private set; }

        public Vector3 Position { get; private set; }

        public Vector3 Rotationrad { get; private set; }

        public float Mass { get; private set; }

        public float MovementAttenuation { get; private set; }

        public float RotationalDamping { get; private set; }

        public float RepulsiveForce { get;private set; }

        public float FrictionForce { get; private set; }

        public RigidBodyPhysics Physics { get; private set; }


        public RigidBody()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal RigidBody( Stream fs, Header header )
        {
            this.RigidBodyName = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.RigidBodyName_English = ParserHelper.get_TextBuf( fs, header.EncodingMethod );
            this.RelatedBoneIndex = ParserHelper.get_Index( fs, header.BoneIndexSize );
            this.Group = ParserHelper.get_Byte( fs );
            this.NonCollisionGroupFlag = ParserHelper.get_UShort( fs );
            this.Shape = (RigidBodyShape) ParserHelper.get_Byte( fs );
            this.Size = ParserHelper.get_Float3( fs );
            this.Position = ParserHelper.get_Float3( fs );
            this.Rotationrad = ParserHelper.get_Float3( fs );
            this.Mass = ParserHelper.get_Float( fs );
            this.MovementAttenuation = ParserHelper.get_Float( fs );
            this.RotationalDamping = ParserHelper.get_Float( fs );
            this.RepulsiveForce = ParserHelper.get_Float( fs );
            this.FrictionForce = ParserHelper.get_Float( fs );
            this.Physics = (RigidBodyPhysics) ParserHelper.get_Byte( fs );
        }
    }
}
