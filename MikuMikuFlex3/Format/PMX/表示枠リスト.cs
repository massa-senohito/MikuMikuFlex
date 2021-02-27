using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class DisplayFrameList : List<DisplayFrame>
    {
        public DisplayFrameList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal DisplayFrameList( Stream st, Header header )
        {
            int NumberOfDisplayFrames = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfDisplayFrames: {NumberOfDisplayFrames}" );

            this.Capacity = NumberOfDisplayFrames;

            for( int i = 0; i < NumberOfDisplayFrames; i++ )
                this.Add( new DisplayFrame( st, header ) );
        }
    }
}
