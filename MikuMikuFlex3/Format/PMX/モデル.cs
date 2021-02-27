using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class Model
    {
        public Header Header { get; private set; }

        public ModelInformation ModelInformation { get; private set; }

        public VertexList VertexList { get; private set; }

        public FaceList FaceList { get; private set; }

        public TextureList TextureList { get; private set; }

        public MaterialList MaterialList { get; private set; }

        public BoneList BoneList { get; private set; }

        public MorphList MorphList { get; private set; }

        public DisplayFrameList DisplayFrameList { get; private set; }

        public RigidBodyList RigidBodyList { get; private set; }

        public JointList JointList { get; private set; }

        // todo: SoftBody は未実装(PMX2.1)
        //public 軟体リスト 軟体リスト { get; private set; }


        public Model()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        public Model( Stream st )
        {
            this.Header = new Header( st );
            this.ModelInformation = new ModelInformation( st, this.Header );
            this.VertexList = new VertexList( st, this.Header );
            this.FaceList = new FaceList( st, this.Header );
            this.TextureList = new TextureList( st, this.Header );
            this.MaterialList = new MaterialList( st, this.Header );
            this.BoneList = new BoneList( st, this.Header );
            this.MorphList = new MorphList( st, this.Header );
            this.DisplayFrameList = new DisplayFrameList( st, this.Header );
            this.RigidBodyList = new RigidBodyList( st, this.Header );
            this.JointList = new JointList( st, this.Header );
            if( this.Header.PMXVersion >= 2.1 )
            {
                // Todo: SoftBody の読み込みは未対応
                //this.軟体リスト = 軟体リスト.Read( st, this.Header );
            }
        }
    }
}
