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


        // 方向（回転、"＞" の表示先）

        public ボーンの接続先表示方法 ボーンの接続先表示方法 { get; private set; }

        /// <summary>
        ///     ボーンの方向を、ボーンの位置からの相対位置で指定する。
        ///     <see cref="ボーンの接続先表示方法"/> が <see cref="ボーンの接続先表示方法.相対座標で指定"/> である場合に有効。
        /// </summary>
        public Vector3 ボーン位置からの相対位置 { get; private set; }

        /// <summary>
        ///     ボーンの方向を、別のボーンの位置で指定する。
        ///     <see cref="ボーンの接続先表示方法"/> が <see cref="ボーンの接続先表示方法.ボーンで指定"/> である場合に有効。
        /// </summary>
        public int 接続先ボーンのボーンインデックス { get; private set; }


        // ボーンの性能（属性）

        /// <summary>
        ///     true なら、ボーンの位置は変化せず、方向（回転）だけが変化する。
        /// </summary>
        public bool 回転可能である { get; private set; }

        /// <summary>
        ///     true なら、ボーンの方向（回転）は変化せず、位置だけが変化する。
        /// </summary>
        public bool 移動可能である { get; private set; }

        /// <summary>
        ///     IKの目標点である？
        /// </summary>
        /// <remarks>
        ///     true の場合、このボーンは、モデルの一部を構成する通常のボーンとは異なり、IKのターゲット（目標点）となる専用のボーンである。
        /// </remarks>
        public bool IKボーンである { get; private set; }

        /// <summary>
        ///     ボーンの表示・選択が可能？
        /// </summary>
        public bool 表示可能である { get; private set; }

        /// <summary>
        ///     選択時に操作可能？
        /// </summary>
        public bool 操作可能である { get; private set; }

        
        // 変形階層
        // 　変形は、初めにボーン番号の順番に行い、その後、IKが順番に行われる。
        // 　変形階層を使うと、この順番をある程度変えることができる。
        // 　変形階層を考慮すると、変換の順番は以下のようになる。
        // 　　１．物理変形前（変形階層順）
        //   　２．物理演算
        // 　　３．物理変形後（変形階層順）
        // 　従って、「物理変形の前か後か」と「変形階層番号」の２つで変形順を設定する。

        public bool 物理後変形である { get; private set; }

        public int 変形階層 { get; private set; }


        // 付与
        // 　ボーンの方向や位置に、付与親ボーンの回転量や移動量を加算すること。
        // 　付与される回転量と移動量は 付与率 により増減することができる。（負数も指定可能。）
        // 　注意：付与親と付与率は、回転付与と移動付与の両方で共通で、１つだけ指定可能。

        /// <summary>
        ///     true の場合、付与親の方向の変化量（回転の変化量）に <see cref="付与率"/> を乗じた値が、このボーンの方向にも付与される。
        /// </summary>
        /// <remarks>
        ///     例：ボーンの方向 (10,20,30)
        ///     　　付与親の方向 (100,200,300)
        ///     　　付与率 0.2
        ///     　　のとき、付与親の方向が (+1, +10, -6) 変化すると、
        ///     　　ボーンの方向は (10 + (+1×0.2), 20 + (+10×0.2), 30 + (-6×0.2)) = (10.2, 22, 28.8) となる。
        /// </remarks>
        public bool 回転付与される { get; private set; }

        /// <summary>
        ///     true の場合、付与親の位置の変化量（移動量）に <see cref="付与率"/> を乗じた値が、このボーンの位置にも付与される。
        /// </summary>
        /// <remarks>
        ///     例：ボーンの位置 (10,20,30)
        ///     　　付与親の位置 (100,200,300)
        ///     　　付与率 0.2
        ///     　　のとき、付与親の位置が (+1, +10, -6) 変化すると、
        ///     　　ボーンの位置は (10 + (+1×0.2), 20 + (+10×0.2), 30 + (-6×0.2)) = (10.2, 22, 28.8) となる。
        /// </remarks>
        public bool 移動付与される { get; private set; }

        /// <summary>
        ///     回転・移動の付与親となるボーン。
        ///     <see cref="回転付与される"/> または <see cref="移動付与される"/> が true の場合に有効。
        /// </summary>
        public int 付与親ボーンインデックス { get; private set; }

        /// <summary>
        ///     <see cref="回転付与される"/> または <see cref="移動付与される"/> が true の場合に有効。
        /// </summary>
        public float 付与率 { get; private set; }


        // 軸制限
        // 　通常、ボーンは X, Y, Z の３軸にそって回転することができるが、
        // 　これを、指定した１軸（回転軸）にそってのみ回転できるように制限すること。

        /// <summary>
        ///     true なら、軸制限を行う。
        /// </summary>
        public bool 軸制限あり { get; private set; }

        /// <summary>
        ///     回転軸を表すベクトル。
        ///     <see cref="軸制限あり"/> が true のときのみ有効。
        /// </summary>
        public Vector3 回転軸の方向ベクトル { get; private set; }


        // ローカル軸
        // 　ボーン操作時のX,Y,Z軸を指定する。
        // 　軸の指定は X, Z しかなく、この２つから Y 軸は自動的に決定される。

        public bool ローカル軸あり { get; private set; }

        public ローカル付与対象 ローカル付与対象 { get; private set; }

        public Vector3 ローカル軸のX軸の方向ベクトル { get; private set; }  // ローカル軸あり である場合

        public Vector3 ローカル軸のZ軸の方向ベクトル { get; private set; }  // ローカル軸あり である場合


        // 外部親 (PMX2.1)
        // 　このボーンを、別のモデル（外部親；具体的には番号 0 のボーン）に追従するよう設定することができる。

        /// <summary>
        ///     true の場合、このボーンは外部親に追従する。
        /// </summary>
        public bool 外部親変形である { get; private set; }  // Todo: 外部親に対応する

        /// <summary>
        ///     未使用。
        ///     <see cref="外部親変形である"/> が true のときに指定することができるが、どのソフトもまだ対応していない。
        /// </summary>
        public int 親Key { get; private set; }


        //--------------- IK 関連 -------------------

        /// <summary>
        ///     ターゲットボーン。
        ///     <see cref="IKリンクリスト"/> の先端のボーンを近づける目標となるボーン。
        /// </summary>
        public int IKターゲットボーンインデックス { get; private set; }

        /// <summary>
        ///     IK の反復演算回数。
        ///     PMD及びMMD環境では255回が最大になるようです。
        /// </summary>
        public int IKループ回数 { get; private set; }

        /// <summary>
        ///     IKループ計算時の1回あたりの制限角度[ラジアン]。
        ///     PMDのIK値とは4倍異なるので注意。
        /// </summary>
        public float IK単位角rad { get; private set; }

        /// <summary>
        ///     IKの影響を受けるボーンのリスト。
        /// </summary>
        public List<IKリンク> IKリンクリスト { get; private set; }

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
            this.軸制限あり = ParserHelper.isFlagEnabled( flagnum, 0x0400 );
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

            if( this.軸制限あり )
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

            if( this.付与親ボーンインデックス == -1 )
            {
                this.回転付与される = false;
                this.移動付与される = false;
            }

            if( !this.移動付与される && !this.回転付与される )
                this.付与親ボーンインデックス = -1;
        }
    }
}
