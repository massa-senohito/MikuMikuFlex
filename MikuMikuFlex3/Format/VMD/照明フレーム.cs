using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.VMDFormat
{
    /// <summary>
    ///     光源が不可視の平行光線。環境光。
    /// </summary>
    public class 照明フレーム
    {
        public uint フレーム番号;

        public Vector3 色;

		/// <summary>
		///		位置は、その場所から原点に向かう方向に光が射すことを意味する。
		///     環境光なので、その位置に光源があるわけではない。
		/// </summary>
		public Vector3 位置;


        public 照明フレーム()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal 照明フレーム( Stream fs )
        {
            this.フレーム番号 = ParserHelper.get_DWORD( fs );
            this.色 = ParserHelper.get_Float3( fs );
            this.位置 = ParserHelper.get_Float3( fs );
        }
    }
}
