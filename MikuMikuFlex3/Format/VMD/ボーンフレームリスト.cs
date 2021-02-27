using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class BoneFrameList : List<BoneFrame>
    {
        public BoneFrameList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal BoneFrameList( Stream fs )
        {
            var NumberOfBoneFrames = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = NumberOfBoneFrames;

            for( int i = 0; i < NumberOfBoneFrames; i++ )
                this.Add( new BoneFrame( fs ) );
        }
    }
}
