using MikuMikuFlex3.VMDFormat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex.VMD
{
    public class Motion
    {
        public Header Header;

        public BoneFrameList BoneFrameList;

        public MorphFrameList MorphFrameList;

        public CameraFrameList CameraFrameList;

        public LightingFrameList LightingFrameList;

        // Todo: セルフ影には未対応
        //public セルフ影リスト セルフ影リスト;

        // TODO: モデル表示・IK on/off には未対応
        //public モデル表示_IKリスト モデル表示_IKリスト;


        public Motion()
        {
        }

		/// <summary>
		///     指定されたストリームから読み込む。
		/// </summary>
		public Motion( Stream fs )
        {
            this.Header = new Header( fs );
            this.BoneFrameList = new BoneFrameList( fs );
            this.MorphFrameList = new MorphFrameList( fs );
            this.CameraFrameList = new CameraFrameList( fs );
            this.LightingFrameList = new LightingFrameList( fs );
            // 拡張
            //this.セルフ影リスト = new セルフ影リスト( fs );
        }
    }
}
