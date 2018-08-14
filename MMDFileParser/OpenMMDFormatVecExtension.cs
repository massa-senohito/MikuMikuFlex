using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace OpenMMDFormat
{
    public static class OpenMMDFormatVecExtension
    {
        public static Vector2 ToSharpDX( this bvec2 vec )
        {
            return new Vector2( vec.x, vec.y );
        }

        public static Quaternion ToSharpDX( this vec4 vec )
        {
            return new Quaternion( vec.x, vec.y, vec.z, vec.w );
        }
    }
}
