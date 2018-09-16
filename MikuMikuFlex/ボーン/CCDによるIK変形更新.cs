using System;
using System.Collections.Generic;
using System.Diagnostics;
using MikuMikuFlex.モデル;
using SharpDX;

namespace MikuMikuFlex
{
    /// <summary>
    ///     CCDIKでIKボーンを更新するクラス
    /// </summary>
    internal class CCDによるIK変形更新 : 変形更新
    {
        public CCDによるIK変形更新( WeakReference<List<PMXボーン>> wrefIKbones )
        {
            this._IKボーンリスト = wrefIKbones;
        }

        public bool 変形を更新する()
        {
            if( _IKボーンリスト.TryGetTarget( out List<PMXボーン> boneList ) )
            {
                foreach( PMXボーン IKbone in boneList )
                    _IKボーンを更新する( IKbone );
            }

            return false;
        }


        private WeakReference<List<PMXボーン>> _IKボーンリスト;


        private void _IKボーンを更新する( PMXボーン IKbone )
        {
            // 現在のループ回数のリセット
            foreach( var link in IKbone.IKリンクリスト )
                link.現在のループ回数 = 0;

            // 決められた回数、IK演算を反復する。
            for( int it = 0; it < IKbone.IK演算のLoop回数; it++ )
                _一回のIK演算を実行する( IKbone );
        }

        private bool _一回のIK演算を実行する( PMXボーン IKbone )
        {
            var エフェクタ = IKbone.IKターゲットボーン;
            var ターゲット = IKbone;

            // すべての IKリンク について……
            foreach( var ikLink in IKbone.IKリンクリスト )
            {
                var IKリンクのローカル座標へ変換する行列 = Matrix.Invert( ikLink.IKリンクボーン.モデルポーズ行列 );

                var IKリンクローカル座標でのエフェクタ位置 = Vector3.TransformCoordinate( エフェクタ.ローカル位置, エフェクタ.モデルポーズ行列 * IKリンクのローカル座標へ変換する行列 );
                var IKリンクローカル座標でのターゲット位置 = Vector3.TransformCoordinate( ターゲット.ローカル位置, ターゲット.モデルポーズ行列 * IKリンクのローカル座標へ変換する行列 );

                var V1 = Vector3.Normalize( IKリンクローカル座標でのエフェクタ位置 - ikLink.IKリンクボーン.ローカル位置 );
                var V2 = Vector3.Normalize( IKリンクローカル座標でのターゲット位置 - ikLink.IKリンクボーン.ローカル位置 );


                // IKリンクを更新する

                ikLink.現在のループ回数++;


                // 回転軸 Vaxis ＝ v1×v2 を計算する。

                var Vaxis = Vector3.Cross( V1, V2 );


                // 回転軸周りの回転角 θa ＝ Cos-1(V1・V2) を計算する。

                var 内積 = (double) Vector3.Dot( V1, V2 );
                内積 = Math.Max( Math.Min( 内積, 1.0 ), -1.0 ); // 誤差丸め

                double θa = Math.Acos( 内積 );        // 内積: -1 → 1  のとき、θa: π → 0
                θa = Math.Min( θa, IKbone.IK単位角 ); // θa は単位角を超えないこと
                if( θa <= 0.00001 ) continue;         // θa が小さすぎたら無視


                // Vaxis と θa から、回転クォータニオンを計算する。

                var 回転クォータニオン = Quaternion.RotationAxis( Vaxis, (float) θa );
                回転クォータニオン.Normalize();


                // IKリンクに回転クォータニオンを適用する。

                ikLink.IKリンクボーン.回転 *= 回転クォータニオン;


                // 回転量制限があれば適用する。

                if( ikLink.回転制限がある )
                {
                    #region " 回転量制限 "
                    //----------------
                    float X軸回転角, Y軸回転角, Z軸回転角;

                    // 回転にはクォータニオンを使っているが、PMXのボーンの最小回転量・最大回転量は、クォータニオンではなくオイラー角（X,Y,Z）で指定される。
                    // そのため、回転数制限の手順は
                    //  (1) クォータニオンをいったんオイラー角（X,Y,Z）に変換
                    //  (2) X,Y,Z に対して回転量制限チェック
                    //  (3) オイラー角を再びクォータニオンに戻す
                    // となる。
                    //
                    // また、オイラー角で回転を表す場合、各軸の回転の順番が重要となる。
                    // ここでは、
                    //  (A) X → Y → Z の順
                    //  (B) Y → Z → X の順
                    //  (C) Z → X → Y の順
                    // の３通りを調べて、ジンバルロックが発生しない最初の分解値を採用する。

                    if( CGHelper.クォータニオンをXYZ回転に分解する( ikLink.IKリンクボーン.回転, out X軸回転角, out Y軸回転角, out Z軸回転角 ) )    // ジンバルロックが発生しなければ true
                    {
                        // (A) XYZ 回転

                        var clamped = Vector3.Clamp(
                            new Vector3( X軸回転角, Y軸回転角, Z軸回転角 ).オイラー角の値域を正規化する(),
                            ikLink.最小回転量,
                            ikLink.最大回転量 );

                        X軸回転角 = clamped.X;
                        Y軸回転角 = clamped.Y;
                        Z軸回転角 = clamped.Z;

                        ikLink.IKリンクボーン.回転 = Quaternion.RotationMatrix( Matrix.RotationX( X軸回転角 ) * Matrix.RotationY( Y軸回転角 ) * Matrix.RotationZ( Z軸回転角 ) );    // X, Y, Z の順
                    }
                    else if( CGHelper.クォータニオンをYZX回転に分解する( ikLink.IKリンクボーン.回転, out Y軸回転角, out Z軸回転角, out X軸回転角 ) )    // ジンバルロックが発生しなければ true
                    {
                        // (B) YZX 回転

                        var clamped = Vector3.Clamp(
                            new Vector3( X軸回転角, Y軸回転角, Z軸回転角 ).オイラー角の値域を正規化する(),
                            ikLink.最小回転量,
                            ikLink.最大回転量 );

                        X軸回転角 = clamped.X;
                        Y軸回転角 = clamped.Y;
                        Z軸回転角 = clamped.Z;

                        ikLink.IKリンクボーン.回転 = Quaternion.RotationMatrix( Matrix.RotationY( Y軸回転角 ) * Matrix.RotationZ( Z軸回転角 ) * Matrix.RotationX( X軸回転角 ) );    // Y, Z, X の順
                    }
                    else if( CGHelper.クォータニオンをZXY回転に分解する( ikLink.IKリンクボーン.回転, out Z軸回転角, out X軸回転角, out Y軸回転角 ) )    // ジンバルロックが発生しなければ true
                    {
                        // (C) ZXY 回転

                        var clamped = Vector3.Clamp(
                            new Vector3( X軸回転角, Y軸回転角, Z軸回転角 ).オイラー角の値域を正規化する(),
                            ikLink.最小回転量,
                            ikLink.最大回転量 );

                        X軸回転角 = clamped.X;
                        Y軸回転角 = clamped.Y;
                        Z軸回転角 = clamped.Z;

                        //ikLink.ikLinkBone.回転行列 = Quaternion.RotationYawPitchRoll( Y軸回転角, X軸回転角, Z軸回転角 );
                        ikLink.IKリンクボーン.回転 = Quaternion.RotationMatrix( Matrix.RotationZ( Z軸回転角 ) * Matrix.RotationX( X軸回転角 ) * Matrix.RotationY( Y軸回転角 ) );    // Z, X, Y の順
                    }
                    else
                    {
                        // その他はエラー
                        continue;
                    }
                    //----------------
                    #endregion
                }

                // IKリンクの新しい回転行列を反映。
                ikLink.IKリンクボーン.モデルポーズを更新する();
            }

            return true;
        }
    }
}
