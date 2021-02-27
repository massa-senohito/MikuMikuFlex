using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     PMX仕様521行目参照、枠内要素にあたるクラス
    /// </summary>
    public class ElementsInTheFrame
    {
        /// <summary>
        ///     true なら <see cref="ElementTargetIndex"/> は MorphIndex であり、
        ///     false なら <see cref="ElementTargetIndex"/> は BoneIndex である。
        /// </summary>
        public bool ElementTarget { get; private set; }

        public int ElementTargetIndex { get; private set; }


        public ElementsInTheFrame()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ElementsInTheFrame( Stream fs, Header header )
        {
            this.ElementTarget = ( ParserHelper.get_Byte( fs ) == 1 );

            if( this.ElementTarget )
            {
                this.ElementTargetIndex = ParserHelper.get_Index( fs, header.MorphIndexSize );
            }
            else
            {
                this.ElementTargetIndex = ParserHelper.get_Index( fs, header.BoneIndexSize );
            }
        }
    }
}
