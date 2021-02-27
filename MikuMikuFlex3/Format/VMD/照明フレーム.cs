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
    public class LightingFrame
    {
        public uint FrameNumber;

        public Vector3 Color;

		/// <summary>
		///		位置は、その場所から原点に向かう方向に光が射すことを意味する。
		///     環境光なので、その位置に光源があるわけではない。
		/// </summary>
		public Vector3 Position;


        public LightingFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal LightingFrame( Stream fs )
        {
            this.FrameNumber = ParserHelper.get_DWORD( fs );
            this.Color = ParserHelper.get_Float3( fs );
            this.Position = ParserHelper.get_Float3( fs );
        }
    }
}
