using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     CCDIKでIKボーンを更新するクラス
    /// </summary>
    static class CCDによるIKDeformationUpdate
    {
        public static void UpdateTransformation( IEnumerable<PMXBoneControl> IKBoneList )
        {
            foreach( var IKbone in IKBoneList )
            {
                // 決められた回数、IK演算を反復する。
                for( int it = 0; it < IKbone.PMXFBourne.IKNumberOfLoops; it++ )
                {
                    var Effector = IKbone.IKTargetBone;
                    var Target = IKbone;

                    // すべての IKLink について……
                    foreach( var ikLink in IKbone.IKLinkList )
                    {
                        var IKMatrixToConvertToLocalCoordinatesOfTheLink = Matrix.Invert( ikLink.IKLinkBone.ModelPoseMatrix );

                        var IKEffectorPositionInLinkLocalCoordinates = Vector3.TransformCoordinate( Effector.LocalLocation, Effector.ModelPoseMatrix * IKMatrixToConvertToLocalCoordinatesOfTheLink );
                        var IKTargetPositionInLinkLocalCoordinates = Vector3.TransformCoordinate( Target.LocalLocation, Target.ModelPoseMatrix * IKMatrixToConvertToLocalCoordinatesOfTheLink );

                        var V1 = Vector3.Normalize( IKEffectorPositionInLinkLocalCoordinates - ikLink.IKLinkBone.LocalLocation );
                        var V2 = Vector3.Normalize( IKTargetPositionInLinkLocalCoordinates - ikLink.IKLinkBone.LocalLocation );


                        // 回転軸 Vaxis ＝ v1×v2 を計算する。

                        var Vaxis = Vector3.Cross( V1, V2 );


                        // 回転軸周りの回転角 θa ＝ Cos-1(V1・V2) を計算する。

                        var InnerProduct = (double) Vector3.Dot( V1, V2 );
                        InnerProduct = Math.Max( Math.Min( InnerProduct, 1.0 ), -1.0 ); // 誤差丸め

                        double θa = Math.Acos( InnerProduct );                        // InnerProduct: -1 → 1  のとき、θa: π → 0
                        θa = Math.Min( θa, IKbone.PMXFBourne.IKUnitAnglerad );  // θa は単位角を超えないこと
                        if( θa <= 0.00001 ) continue;                         // θa が小さすぎたら無視


                        // Vaxis と θa から、回転クォータニオンを計算する。

                        var RotatingQuaternion = Quaternion.RotationAxis( Vaxis, (float) θa );
                        RotatingQuaternion.Normalize();


                        // IKリンクに回転クォータニオンを適用する。

                        ikLink.IKLinkBone.Rotation *= RotatingQuaternion;


                        // 回転量制限があれば適用する。

                        if( ikLink.ThereIsARotationLimit )
                        {
                            #region " RotationAmountLimit "
                            //----------------
                            float XAxisRotationAngle, YAxisRotationAngle, ZAxisRotationAngle;

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

                            if( CGHelper.QuaternionXYZDisassembleIntoRotation( ikLink.IKLinkBone.Rotation, out XAxisRotationAngle, out YAxisRotationAngle, out ZAxisRotationAngle ) )    // ジンバルロックが発生しなければ true
                            {
                                // (A) XYZ Rotation

                                var clamped = Vector3.Clamp(
                                    new Vector3( XAxisRotationAngle, YAxisRotationAngle, ZAxisRotationAngle ).NormalizeTheRangeOfEulerAngles(),
                                    ikLink.MinimumRotationAmount,
                                    ikLink.MaximumRotationAmount );

                                XAxisRotationAngle = clamped.X;
                                YAxisRotationAngle = clamped.Y;
                                ZAxisRotationAngle = clamped.Z;

                                ikLink.IKLinkBone.Rotation = Quaternion.RotationMatrix( Matrix.RotationX( XAxisRotationAngle ) * Matrix.RotationY( YAxisRotationAngle ) * Matrix.RotationZ( ZAxisRotationAngle ) );    // X, Y, Z の順
                            }
                            else if( CGHelper.QuaternionYZXDisassembleIntoRotation( ikLink.IKLinkBone.Rotation, out YAxisRotationAngle, out ZAxisRotationAngle, out XAxisRotationAngle ) )    // ジンバルロックが発生しなければ true
                            {
                                // (B) YZX Rotation

                                var clamped = Vector3.Clamp(
                                    new Vector3( XAxisRotationAngle, YAxisRotationAngle, ZAxisRotationAngle ).NormalizeTheRangeOfEulerAngles(),
                                    ikLink.MinimumRotationAmount,
                                    ikLink.MaximumRotationAmount );

                                XAxisRotationAngle = clamped.X;
                                YAxisRotationAngle = clamped.Y;
                                ZAxisRotationAngle = clamped.Z;

                                ikLink.IKLinkBone.Rotation = Quaternion.RotationMatrix( Matrix.RotationY( YAxisRotationAngle ) * Matrix.RotationZ( ZAxisRotationAngle ) * Matrix.RotationX( XAxisRotationAngle ) );    // Y, Z, X の順
                            }
                            else if( CGHelper.QuaternionZXYDisassembleIntoRotation( ikLink.IKLinkBone.Rotation, out ZAxisRotationAngle, out XAxisRotationAngle, out YAxisRotationAngle ) )    // ジンバルロックが発生しなければ true
                            {
                                // (C) ZXY Rotation

                                var clamped = Vector3.Clamp(
                                    new Vector3( XAxisRotationAngle, YAxisRotationAngle, ZAxisRotationAngle ).NormalizeTheRangeOfEulerAngles(),
                                    ikLink.MinimumRotationAmount,
                                    ikLink.MaximumRotationAmount );

                                XAxisRotationAngle = clamped.X;
                                YAxisRotationAngle = clamped.Y;
                                ZAxisRotationAngle = clamped.Z;

                                //ikLink.ikLinkBone.RotationMatrix = Quaternion.RotationYawPitchRoll( YAxisRotationAngle, XAxisRotationAngle, ZAxisRotationAngle );
                                ikLink.IKLinkBone.Rotation = Quaternion.RotationMatrix( Matrix.RotationZ( ZAxisRotationAngle ) * Matrix.RotationX( XAxisRotationAngle ) * Matrix.RotationY( YAxisRotationAngle ) );    // Z, X, Y の順
                            }
                            else
                            {
                                // その他はエラー
                                continue;
                            }
                            //----------------
                            #endregion
                        }


                        // IKリンクの新しい回転行列を反映する。

                        ikLink.IKLinkBone.CalculateModelPose();
                    }
                }
            }
        }
    }
}
