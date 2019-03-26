using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MikuMikuFlex3.PMXFormat
{
    public class モデル
    {
        public ヘッダ ヘッダ { get; private set; }

        public モデル情報 モデル情報 { get; private set; }

        public 頂点リスト 頂点リスト { get; private set; }

        public 面リスト 面リスト { get; private set; }

        public テクスチャリスト テクスチャリスト { get; private set; }

        public 材質リスト 材質リスト { get; private set; }

        public ボーンリスト ボーンリスト { get; private set; }

        public モーフリスト モーフリスト { get; private set; }

        public 表示枠リスト 表示枠リスト { get; private set; }

        public 剛体リスト 剛体リスト { get; private set; }

        public ジョイントリスト ジョイントリスト { get; private set; }

        // todo: SoftBody は未実装(PMX2.1)
        //public 軟体リスト 軟体リスト { get; private set; }


        public モデル()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        public モデル( FileStream fs )
        {
            this.ヘッダ = new ヘッダ( fs );
            this.モデル情報 = new モデル情報( fs, this.ヘッダ );
            this.頂点リスト = new 頂点リスト( fs, this.ヘッダ );
            this.面リスト = new 面リスト( fs, this.ヘッダ );
            this.テクスチャリスト = new テクスチャリスト( fs, this.ヘッダ );
            this.材質リスト = new 材質リスト( fs, this.ヘッダ );
            this.ボーンリスト = new ボーンリスト( fs, this.ヘッダ );
            this.モーフリスト = new モーフリスト( fs, this.ヘッダ );
            this.表示枠リスト = new 表示枠リスト( fs, this.ヘッダ );
            this.剛体リスト = new 剛体リスト( fs, this.ヘッダ );
            this.ジョイントリスト = new ジョイントリスト( fs, this.ヘッダ );
            if( this.ヘッダ.PMXバージョン >= 2.1 )
            {
                // Todo: SoftBody の読み込みは未対応
                //this.軟体リスト = 軟体リスト.読み込む( fs, this.ヘッダ );
            }
        }
    }
}
