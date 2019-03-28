using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class ボーン
    {
        public string ボーン名 { get; private set; }

        public string ボーン名_英 { get; private set; }


        // 接続元（親）

        public int 親ボーンのインデックス { get; private set; }


        // 位置

        public Vector3 位置 { get; private set; }


        // 方向（接続先; "＞" の表示先）

        public ボーンの接続先表示方法 ボーンの接続先表示方法 { get; private set; }

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

        public ローカル付与対象 ローカル付与対象 { get; private set; }

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
            internal IKリンク( Stream st, ヘッダ header )
            {
                this.リンクボーンのボーンインデックス = ParserHelper.get_Index( st, header.ボーンインデックスサイズ );
                this.角度制限あり = ParserHelper.get_Byte( st ) == 1 ? true : false;

                if( this.角度制限あり )
                {
                    this.角度制限の下限rad = ParserHelper.get_Float3( st );
                    this.角度制限の上限rad = ParserHelper.get_Float3( st );
                }
            }
        }

        public List<IKリンク> IKリンクリスト { get; private set; }


        public ボーン()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal ボーン( Stream st, ヘッダ header )
        {
            this.IKリンクリスト = new List<IKリンク>();
            this.ボーン名 = ParserHelper.get_TextBuf( st, header.エンコード方式 );
            this.ボーン名_英 = ParserHelper.get_TextBuf( st, header.エンコード方式 );
            this.位置 = ParserHelper.get_Float3( st );
            this.親ボーンのインデックス = ParserHelper.get_Index( st, header.ボーンインデックスサイズ );
            this.変形階層 = ParserHelper.get_Int( st );

            var flag = new byte[ 2 ];
            flag[ 0 ] = ParserHelper.get_Byte( st );
            flag[ 1 ] = ParserHelper.get_Byte( st );
            Int16 flagnum = BitConverter.ToInt16( flag, 0 );
            this.ボーンの接続先表示方法 = ParserHelper.isFlagEnabled( flagnum, 0x0001 ) ? ボーンの接続先表示方法.ボーンで指定 : ボーンの接続先表示方法.相対座標で指定;
            this.回転可能である = ParserHelper.isFlagEnabled( flagnum, 0x0002 );
            this.移動可能である = ParserHelper.isFlagEnabled( flagnum, 0x0004 );
            this.表示可能である = ParserHelper.isFlagEnabled( flagnum, 0x0008 );
            this.操作可能である = ParserHelper.isFlagEnabled( flagnum, 0x0010 );
            this.IKボーンである = ParserHelper.isFlagEnabled( flagnum, 0x0020 );
            this.ローカル付与対象 = ParserHelper.isFlagEnabled( flagnum, 0x0080 ) ? ローカル付与対象.親のローカル変形量 : ローカル付与対象.ユーザ変形値_IKリンク_多重付与;
            this.回転付与される = ParserHelper.isFlagEnabled( flagnum, 0x0100 );
            this.移動付与される = ParserHelper.isFlagEnabled( flagnum, 0x0200 );
            this.回転軸あり = ParserHelper.isFlagEnabled( flagnum, 0x0400 );
            this.ローカル軸あり = ParserHelper.isFlagEnabled( flagnum, 0x0800 );
            this.物理後変形である = ParserHelper.isFlagEnabled( flagnum, 0x1000 );
            this.外部親変形である = ParserHelper.isFlagEnabled( flagnum, 0x2000 );

            if( this.ボーンの接続先表示方法 == ボーンの接続先表示方法.相対座標で指定 )
            {
                this.ボーン位置からの相対位置 = ParserHelper.get_Float3( st );
            }
            else
            {
                this.接続先ボーンのボーンインデックス = ParserHelper.get_Index( st, header.ボーンインデックスサイズ );
            }

            if( this.回転付与される || this.移動付与される )
            {
                this.付与親ボーンインデックス = ParserHelper.get_Index( st, header.ボーンインデックスサイズ );
                this.付与率 = ParserHelper.get_Float( st );
            }

            if( this.回転軸あり )
                this.回転軸の方向ベクトル = ParserHelper.get_Float3( st );

            if( this.ローカル軸あり )
            {
                this.ローカル軸のX軸の方向ベクトル = ParserHelper.get_Float3( st );
                this.ローカル軸のZ軸の方向ベクトル = ParserHelper.get_Float3( st );
            }

            if( this.外部親変形である )
                this.親Key = ParserHelper.get_Int( st );

            if( this.IKボーンである )
            {
                this.IKターゲットボーンインデックス = ParserHelper.get_Index( st, header.ボーンインデックスサイズ );
                this.IKループ回数 = ParserHelper.get_Int( st );
                this.IK単位角rad = ParserHelper.get_Float( st );
                int IKリンク数 = ParserHelper.get_Int( st );
                for( int i = 0; i < IKリンク数; i++ )
                    this.IKリンクリスト.Add( new IKリンク( st, header ) );
            }
        }
    }
}
