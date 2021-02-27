using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    public class RigidBodyList : List<RigidBody>
    {
        public RigidBodyList()
            : base()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal RigidBodyList( Stream st, Header header )
        {
            int RigidBodyNumber = ParserHelper.get_Int( st );
            Debug.WriteLine( $"RigidBodyNumber: {RigidBodyNumber}" );

            this.Capacity = RigidBodyNumber;

            for( int i = 0; i < RigidBodyNumber; i++ )
                this.Add( new RigidBody( st, header ) );
        }
    }
}
