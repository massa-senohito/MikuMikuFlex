using System;

namespace MMDFileParser
{
    public static class CGHelper
    {
        public static float 値を最小値と最大値の範囲に収める( float 値, float 最小値, float 最大値 )
        {
            if( 最小値 > 値 )
                return 最小値;

            if( 最大値 < 値 )
                return 最大値;

            return 値;
        }
    }
}
