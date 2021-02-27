using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class LightingFrameList : List<LightingFrame>
    {
        public LightingFrameList()
            :base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal LightingFrameList( Stream fs )
        {
            var NumberOfLightingFrames = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = NumberOfLightingFrames;

            for( int i = 0; i < NumberOfLightingFrames; i++ )
                this.Add( new LightingFrame( fs ) );
        }
    }
}
