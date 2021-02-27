using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class MorphFrameList : List<MorphFrame>
    {
        public MorphFrameList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal MorphFrameList( Stream fs )
        {
            var NumberOfMorphFrames = (int) ParserHelper.get_DWORD( fs );

            this.Capacity = NumberOfMorphFrames;

            for( int i = 0; i < NumberOfMorphFrames; i++ )
                this.Add( new MorphFrame( fs ) );
        }
    }
}
