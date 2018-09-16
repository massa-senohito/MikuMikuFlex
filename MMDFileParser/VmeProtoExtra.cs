using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// 自動実装コード VmeProto の追加要素

namespace MMDFileParser.OpenMMDFormat
{
    /// <summary>
    ///     BoneFrameクラスに IFrameData インターフェースを追加実装
    /// </summary>
    public partial class BoneFrame : MMDFileParser.IFrameData
    {
        public uint フレーム番号 => (uint) _frameNumber;

        public int CompareTo( Object x )
        {
            return (int) フレーム番号 - (int) ( (MMDFileParser.IFrameData) x ).フレーム番号;
        }
    }

    /// <summary>
    ///     MorphFrameクラスに IFrameData インターフェースを追加実装
    /// </summary>
    public partial class MorphFrame : MMDFileParser.IFrameData
    {
        public uint フレーム番号 => (uint) _frameNumber;

        public int CompareTo( Object x )
        {
            return (int) フレーム番号 - (int) ( (MMDFileParser.IFrameData) x ).フレーム番号;
        }
    }
}
