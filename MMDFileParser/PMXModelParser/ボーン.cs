using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser
{
    public class ボーン
    {
        public String ボーン名 { get; private set; }

        public String ボーン名_英 { get; private set; }


        // 接続元（親）

        public int 親ボーンのインデックス { get; private set; }


        // 位置

        public Vector3 位置 { get; private set; }


        // 方向（接続先; "＞" の表示先）

        public enum E接続先表示方法 : byte
        {
            相対座標で指定 = 0,
            ボーンで指定 = 1,
        }
        public E接続先表示方法 ボーンの接続先表示方法 { get; private set; }

        public Vector3 ボーン位置からの相対位置 { get; private set; }       // 接続先を 座標オフセットで指定 する場合

        public int 接続先ボーンのボーンインデックス { get; private set; }   // 接続先を ボーンで指定 する場合


        // ボーンの性能（属性）

        public bool 回転可能である { get; private set; }

        public bool 移動可能である { get; private set; }

        public bool IKボーンである { get; private set; }

        public bool 表示可能である { get; private set; }

        public bool 操作可能である { get; private set; }

        
        // 変形階層

        public int 変形階層 { get; private set; }

        public bool 物理後変形である { get; private set; }


        // 付与

        public bool 回転付与される { get; private set; }

        public bool 移動付与される { get; private set; }

        public int 付与親ボーンインデックス { get; private set; }   // 回転付与 or 移動付与 である場合

        public float 付与率 { get; private set; }                          // 回転付与 or 移動付与 である場合


        // 回転軸

        public bool 回転軸あり { get; private set; }

        public Vector3 回転軸の方向ベクトル { get; private set; }         // 回転軸あり の場合

        
        // ローカル軸

        public enum Eローカル付与対象 : byte
        {
            ユーザ変形値_IKリンク_多重付与 = 0,
            親のローカル変形量 = 1,
        }
        public Eローカル付与対象 ローカル付与対象 { get; private set; }

        public bool ローカル軸あり { get; private set; }

        public Vector3 ローカル軸のX軸の方向ベクトル { get; private set; }  // ローカル軸あり である場合

        public Vector3 ローカル軸のZ軸の方向ベクトル { get; private set; }  // ローカル軸あり である場合


        // 外部親

        public bool 外部親変形である { get; private set; }  // Todo: 外部親は現状未使用？

        public int 親Key { get; private set; }       // 外部親変形 である場合;  現状未使用


        //--------------- IK 関連 -------------------

        public int IKターゲットボーンインデックス { get; private set; }

        /// <summary>
        ///     PMD及びMMD環境では255回が最大になるようです
        /// </summary>
        public int IKループ回数 { get; private set; }

        /// <summary>
        ///     IKループ計算時の1回あたりの制限角度[ラジアン]。
        ///     PMDのIK値とは4倍異なるので注意。
        /// </summary>
        public float IK単位角rad { get; private set; }

        public class IKリンク
        {
            public int リンクボーンのボーンインデックス { get; private set; }

            public bool 角度制限あり { get; private set; }

            public Vector3 角度制限の下限rad { get; private set; }     // 角度制限あり の場合

            public Vector3 角度制限の上限rad { get; private set; }     // 角度制限あり の場合

            /// <summary>
            ///     指定されたストリームから読み込む。
            /// </summary>
            internal static IKリンク 読み込む( FileStream fs, PMXヘッダ header )
            {
                var ikLinkData = new IKリンク();

                ikLinkData.リンクボーンのボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
                ikLinkData.角度制限あり = ParserHelper.get_Byte( fs ) == 1 ? true : false;

                if( ikLinkData.角度制限あり )
                {
                    ikLinkData.角度制限の下限rad = ParserHelper.get_Float3( fs );
                    ikLinkData.角度制限の上限rad = ParserHelper.get_Float3( fs );
                }

                return ikLinkData;
            }
        }

        public List<IKリンク> IKリンクリスト { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal static ボーン 読み込む( FileStream fs, PMXヘッダ header )
        {
            var bone = new ボーン();

            bone.IKリンクリスト = new List<IKリンク>();
            bone.ボーン名 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            bone.ボーン名_英 = ParserHelper.get_TextBuf( fs, header.エンコード方式 );
            bone.位置 = ParserHelper.get_Float3( fs );
            bone.親ボーンのインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            bone.変形階層 = ParserHelper.get_Int( fs );

            var flag = new byte[ 2 ];
            flag[ 0 ] = ParserHelper.get_Byte( fs );
            flag[ 1 ] = ParserHelper.get_Byte( fs );
            Int16 flagnum = BitConverter.ToInt16( flag, 0 );
            bone.ボーンの接続先表示方法 = ParserHelper.isFlagEnabled( flagnum, 0x0001 ) ? E接続先表示方法.ボーンで指定 : E接続先表示方法.相対座標で指定;
            bone.回転可能である = ParserHelper.isFlagEnabled( flagnum, 0x0002 );
            bone.移動可能である = ParserHelper.isFlagEnabled( flagnum, 0x0004 );
            bone.表示可能である = ParserHelper.isFlagEnabled( flagnum, 0x0008 );
            bone.操作可能である = ParserHelper.isFlagEnabled( flagnum, 0x0010 );
            bone.IKボーンである = ParserHelper.isFlagEnabled( flagnum, 0x0020 );
            bone.ローカル付与対象 = ParserHelper.isFlagEnabled( flagnum, 0x0080 ) ? Eローカル付与対象.親のローカル変形量 : Eローカル付与対象.ユーザ変形値_IKリンク_多重付与;
            bone.回転付与される = ParserHelper.isFlagEnabled( flagnum, 0x0100 );
            bone.移動付与される = ParserHelper.isFlagEnabled( flagnum, 0x0200 );
            bone.回転軸あり = ParserHelper.isFlagEnabled( flagnum, 0x0400 );
            bone.ローカル軸あり = ParserHelper.isFlagEnabled( flagnum, 0x0800 );
            bone.物理後変形である = ParserHelper.isFlagEnabled( flagnum, 0x1000 );
            bone.外部親変形である = ParserHelper.isFlagEnabled( flagnum, 0x2000 );

            if( bone.ボーンの接続先表示方法 == E接続先表示方法.相対座標で指定 )
            {
                bone.ボーン位置からの相対位置 = ParserHelper.get_Float3( fs );
            }
            else
            {
                bone.接続先ボーンのボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
            }

            if( bone.回転付与される || bone.移動付与される )
            {
                bone.付与親ボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
                bone.付与率 = ParserHelper.get_Float( fs );
            }

            if( bone.回転軸あり )
                bone.回転軸の方向ベクトル = ParserHelper.get_Float3( fs );

            if( bone.ローカル軸あり )
            {
                bone.ローカル軸のX軸の方向ベクトル = ParserHelper.get_Float3( fs );
                bone.ローカル軸のZ軸の方向ベクトル = ParserHelper.get_Float3( fs );
            }

            if( bone.外部親変形である )
                bone.親Key = ParserHelper.get_Int( fs );

            if( bone.IKボーンである )
            {
                bone.IKターゲットボーンインデックス = ParserHelper.get_Index( fs, header.ボーンインデックスサイズ );
                bone.IKループ回数 = ParserHelper.get_Int( fs );
                bone.IK単位角rad = ParserHelper.get_Float( fs );
                int IKリンク数 = ParserHelper.get_Int( fs );
                for( int i = 0; i < IKリンク数; i++ )
                    bone.IKリンクリスト.Add( IKリンク.読み込む( fs, header ) );
            }

            return bone;
        }
    }
}
