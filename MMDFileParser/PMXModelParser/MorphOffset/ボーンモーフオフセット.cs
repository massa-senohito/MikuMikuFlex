using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.MorphOffset
{
    public class ボーンモーフオフセット : モーフオフセット
    {
        public int ボーンインデックス { get; private set; }

        public Vector3 移動量 { get; private set; }

        /// <summary>
        ///     クォータニオン(x,y,z,w)
        /// </summary>
        public Vector4 回転量 { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ボーンモーフオフセット 読み込む( FileStream fs, PMXヘッダ header )
        {
            var offset = new ボーンモーフオフセット();

            offset.モーフ種類 = モーフ種類.ボーン;
            offset.ボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            offset.移動量 = ParserHelper.get_Float3( fs );
            offset.回転量 = ParserHelper.get_Float4( fs );

            return offset;
        }
    }
}
