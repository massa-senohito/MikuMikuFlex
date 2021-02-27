using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuFlex3.PMXFormat
{
    [Flags]
    public enum DrawingFlag
    {
        DoubleSidedDrawing = 0x01,
        GroundShadow = 0x02,
        SelfShadowMap = 0x04,
        SelfShadow = 0x08,
        Edge = 0x10,
        /// <summary>
        ///     頂点の "AddToUV1" の値(float4)を "照明計算後の色値" として描画に利用するモード
        /// </summary>
        VertexColor = 0x20,    // todo: DrawingFlag.VertexColor への対応

        /// <summary>
        ///     描画プリミティブを PointList で描画
        ///     ※Point／Line 双方とも ON の場合は Point 優先
        /// </summary>
        PointDrawing = 0x40,     // todo: DrawingFlag.Point描画への対応

        /// <summary>
        ///     描画プリミティブを LineList で描画
        ///     ※Point／Line 双方とも ON の場合は Point 優先
        /// </summary>
        LineDrawing = 0x80,
    }
}
