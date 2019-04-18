using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public static class VMDアニメーションビルダ
    {
        public static void アニメーションを追加する( string vmdFilePath, PMXモデル PMXモデル, bool すべての親を無視する = true )
        {
            var vmd = new VMDFormat.モーション( vmdFilePath );
            ボーンモーションを追加する( vmd.ボーンフレームリスト, PMXモデル, すべての親を無視する );
            モーフを追加する( vmd.モーフフレームリスト, PMXモデル );
        }

        /// <summary>
        ///     VMDのボーンフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFボーンフレームリスト">入力となるボーンフレームリスト。</param>
        /// <param name="PMXモデル">対象となるPMXモデル。</param>
        public static void ボーンモーションを追加する( VMDFormat.ボーンフレームリスト VMDFボーンフレームリスト, PMXモデル PMXモデル, bool 全ての親を無視する = true )
        {
            // すべてのPMXボーンについて……
            for( int i = 0; i < PMXモデル.ボーンリスト.Length; i++ )
            {
                var pmxBone = PMXモデル.ボーンリスト[ i ];

                if( 全ての親を無視する && pmxBone.PMXFボーン.ボーン名 == "全ての親" )
                    continue;
                

                // 同じボーン名のフレームを列挙する。

                var boneFrames = VMDFボーンフレームリスト
                    .Where( ( frame ) => ( frame.ボーン名 == pmxBone.PMXFボーン.ボーン名 ) )  // 同じボーン名のフレームを、
                    .OrderBy( ( frame ) => frame.フレーム番号 );                              // フレーム番号昇順に。


                // 列挙されたすべてのフレームについて……

                uint 前のフレーム番号 = 0;

                foreach( var frame in boneFrames )
                {
                    var 持続時間sec = ( frame.フレーム番号 - 前のフレーム番号 ) / 30.0;   // 1frame = 1/30sec

                    pmxBone.アニメ変数_移動.遷移を追加する( 
                        new ベジェ移動アニメ遷移( frame.ボーンの位置, 持続時間sec, frame.ベジェ曲線[ 0 ], frame.ベジェ曲線[ 1 ], frame.ベジェ曲線[ 2 ] ) );

                    pmxBone.アニメ変数_回転.遷移を追加する( 
                        new ベジェ回転アニメ遷移( frame.ボーンの回転, 持続時間sec, frame.ベジェ曲線[ 3 ] ) );

                    前のフレーム番号 = frame.フレーム番号;
                }
            }
        }

        /// <summary>
        ///     VMDのモーフフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFモーフフレームリスト">入力となるモーフフレームリスト。</param>
        /// <param name="PMXモデル">対象となるPMXモデル。</param>
        public static void モーフを追加する( VMDFormat.モーフフレームリスト VMDFモーフフレームリスト, PMXモデル PMXモデル )
        {
            // すべてのモーフについて……
            for( int i = 0; i < PMXモデル.モーフリスト.Length; i++ )
            {
                var pmxMorph = PMXモデル.モーフリスト[ i ];

                
                // 同じモーフ名のフレームを列挙する。

                var morphFrames = VMDFモーフフレームリスト
                    .Where( ( frame ) => ( frame.モーフ名 == pmxMorph.PMXFモーフ.モーフ名 ) ) // 同じ名前のフレームを、
                    .OrderBy( ( frame ) => frame.フレーム番号 );                              // フレーム番号昇順に。


                // 列挙されたすべてのフレームについて……

                uint 前のフレーム番号 = 0;

                foreach( var frame in morphFrames )
                {
                    var 持続時間sec = ( frame.フレーム番号 - 前のフレーム番号 ) / 30.0;   // 1frame = 1/30sec

                    pmxMorph.アニメ変数_モーフ.遷移を追加する( new リニア実数アニメ遷移( frame.モーフ値, 持続時間sec ) );

                    前のフレーム番号 = frame.フレーム番号;
                }
            }
        }

        /// <summary>
        ///     VMDのボーンフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFカメラフレームリスト">入力となるカメラフレームリスト。</param>
        /// <param name="カメラ">対象となるカメラ。</param>
        public static void カメラモーションを追加する( VMDFormat.カメラフレームリスト VMDFカメラフレームリスト, モーションカメラMMD カメラ )
        {
            var cameraFrames = VMDFカメラフレームリスト
                .OrderBy( ( frame ) => frame.フレーム番号 );  // フレーム番号昇順に。

            uint 前のフレーム番号 = 0;

            foreach( var frame in cameraFrames )
            {
                var 持続時間sec = ( frame.フレーム番号 - 前のフレーム番号 ) / 30.0;   // 1frame = 1/30sec

                カメラ.アニメ変数.注視点からの距離.遷移を追加する( new ベジェ実数アニメ遷移( frame.距離, 持続時間sec, frame.ベジェ曲線[ 4 ] ) );
                カメラ.アニメ変数.注視点の位置.遷移を追加する( new ベジェ移動アニメ遷移( frame.位置, 持続時間sec, frame.ベジェ曲線[ 0 ], frame.ベジェ曲線[ 1 ], frame.ベジェ曲線[ 2 ] ) );
                カメラ.アニメ変数.回転rad.遷移を追加する( new ベジェ移動アニメ遷移( frame.回転, 持続時間sec, frame.ベジェ曲線[ 3 ], frame.ベジェ曲線[ 3 ], frame.ベジェ曲線[ 3 ] ) );
                カメラ.アニメ変数.視野角deg.遷移を追加する( new ベジェ実数アニメ遷移( frame.視野角, 持続時間sec, frame.ベジェ曲線[ 5 ] ) );
                // todo: VMDカメラモーションのパースペクティブには未対応

                前のフレーム番号 = frame.フレーム番号;
            }
        }
    }
}
