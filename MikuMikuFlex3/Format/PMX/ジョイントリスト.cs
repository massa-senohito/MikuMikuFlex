using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class JointList : List<Joint>
    {
        public JointList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal JointList( Stream st, Header header )
        {
            int NumberOfJoints = ParserHelper.get_Int( st );
            Debug.WriteLine( $"NumberOfJoints: {NumberOfJoints}" );

            this.Capacity = NumberOfJoints;

            for( int i = 0; i < NumberOfJoints; i++ )
                this.Add( new Joint( st, header ) );
        }
    }
}
