using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;

namespace MMDFileParser.MotionParser
{
    /// <summary>
    ///     光源が不可視の平行光線。環境光。
    /// </summary>
    /// <remarks>
    ///     位置は、その場所から原点に向かう方向に光が射すことを意味する。
    ///     環境光なので、その位置に光源があるわけではない。
    /// </remarks>
    public class 照明フレーム
    {
        public uint フレーム番号;

        public Vector3 色;

        public Vector3 位置;

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static 照明フレーム 読み込む( Stream fs )
        {
            var frame = new 照明フレーム();

            frame.フレーム番号 = ParserHelper.get_DWORD( fs );
            frame.色 = ParserHelper.get_Float3( fs );
            frame.位置 = ParserHelper.get_Float3( fs );

            return frame;
        }
    }
}
