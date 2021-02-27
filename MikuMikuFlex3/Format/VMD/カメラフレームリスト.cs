using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class CameraFrameList : List<CameraFrame>
    {
        public CameraFrameList()
            :base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal CameraFrameList( Stream fs )
        {
            var NumberOfCameraFrames = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = NumberOfCameraFrames;

            for( int i = 0; i < NumberOfCameraFrames; i++ )
                this.Add( new CameraFrame( fs ) );
        }
    }
}
