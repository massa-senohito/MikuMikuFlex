using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.VMDFormat
{
    public class Motion
    {
        public Header Header;

        public BoneFrameList BoneFrameList;

        public MorphFrameList MorphFrameList;

        public CameraFrameList CameraFrameList;

        public LightingFrameList LightingFrameList;

        // todo: VMDのセルフ影への対応
        //public セルフ影リスト セルフ影リスト;

        // todo: VMDのモデル表示・IK on/off への対応
        //public モデル表示_IKリスト モデル表示_IKリスト;


        public Motion()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        public Motion( Stream fs )
            : this()
        {
            this._Read( fs );
        }

        /// <summary>
        ///     指定されたファイルから読み込む。
        /// </summary>
        public Motion( string filePath )
            : this()
        {
            using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                this._Read( fs );
        }

        private void _Read( Stream fs )
        {
            this.Header = new Header( fs );
            this.BoneFrameList = new BoneFrameList( fs );
            this.MorphFrameList = new MorphFrameList( fs );
            this.CameraFrameList = new CameraFrameList( fs );
            this.LightingFrameList = new LightingFrameList( fs );
            //this.セルフ影リスト = new セルフ影リスト( fs );
            //this.モデル表示_IKリスト = new モデル表示_IKリスト( fs );
        }
    }
}
