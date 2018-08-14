using System.IO;

namespace MMDFileParser.PMXModelParser
{
    public class PMXモデル
    {
        public PMXヘッダ ヘッダ { get; private set; }

        public PMXモデル情報 モデル情報 { get; private set; }

        public 頂点リスト 頂点リスト { get; private set; }

        public 面リスト 面リスト { get; private set; }

        public テクスチャリスト テクスチャリスト { get; private set; }

        public 材質リスト 材質リスト { get; private set; }

        public ボーンリスト ボーンリスト { get; private set; }

        public モーフリスト モーフリスト { get; private set; }

        public 表示枠リスト 表示枠リスト { get; private set; }

        public 剛体リスト 剛体リスト { get; private set; }

        public ジョイントリスト ジョイントリスト { get; private set; }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        public static PMXモデル 読み込む( FileStream fs )
        {
            var model = new PMXモデル();

            model.ヘッダ = PMXヘッダ.読み込む( fs );
            model.モデル情報 = PMXモデル情報.読み込む( fs, model.ヘッダ );
            model.頂点リスト = 頂点リスト.読み込む( fs, model.ヘッダ );
            model.面リスト = 面リスト.読み込む( fs, model.ヘッダ );
            model.テクスチャリスト = テクスチャリスト.読み込む( fs, model.ヘッダ );
            model.材質リスト = 材質リスト.読み込む( fs, model.ヘッダ );
            model.ボーンリスト = ボーンリスト.読み込む( fs, model.ヘッダ );
            model.モーフリスト = モーフリスト.読み込む( fs, model.ヘッダ );
            model.表示枠リスト = 表示枠リスト.読み込む( fs, model.ヘッダ );
            model.剛体リスト = 剛体リスト.読み込む( fs, model.ヘッダ );
            model.ジョイントリスト = ジョイントリスト.読み込む( fs, model.ヘッダ );
            if( model.ヘッダ.PMXバージョン >= 2.1 )
            {
                // Todo: SoftBody の読み込みは未対応
                //model.SoftBodyデータリスト = SoftBodyリスト.読み込む( fs, model.ヘッダ );
            }
            return model;
        }
    }
}
