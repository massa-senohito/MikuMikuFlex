using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public static class VMDAnimationBuilder
    {
        public static void AddAnimation( string vmdFilePath, PMXModel PMXModel, bool IgnoreAllParents = true )
        {
            var vmd = new VMDFormat.Motion( vmdFilePath );
            AddBoneMotion( vmd.BoneFrameList, PMXModel, IgnoreAllParents );
            AddAMorph( vmd.MorphFrameList, PMXModel );
        }

        /// <summary>
        ///     VMDのボーンフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFBoneFrameList">入力となるボーンフレームリスト。</param>
        /// <param name="PMXModel">対象となるPMXModel。</param>
        public static void AddBoneMotion( VMDFormat.BoneFrameList VMDFBoneFrameList, PMXModel PMXModel, bool IgnoreAllParents = true )
        {
            // すべてのPMXボーンについて……
            for( int i = 0; i < PMXModel.BoneList.Length; i++ )
            {
                var pmxBone = PMXModel.BoneList[ i ];

                if( IgnoreAllParents && pmxBone.PMXFBourne.BoneName == "AllParents" )
                    continue;
                

                // 同じボーン名のフレームを列挙する。

                var boneFrames = VMDFBoneFrameList
                    .Where( ( frame ) => ( frame.BoneName == pmxBone.PMXFBourne.BoneName ) )  // 同じボーン名のフレームを、
                    .OrderBy( ( frame ) => frame.FrameNumber );                              // フレーム番号昇順に。


                // 列挙されたすべてのフレームについて……

                uint PreviousFrameNumber = 0;

                foreach( var frame in boneFrames )
                {
                    var Durationsec = ( frame.FrameNumber - PreviousFrameNumber ) / 30.0;   // 1frame = 1/30sec

                    pmxBone.AnimeVariables_Move.AddATransition( 
                        new BezierMovingAnimationTransition( frame.BonePosition, Durationsec, frame.BezierCurve[ 0 ], frame.BezierCurve[ 1 ], frame.BezierCurve[ 2 ] ) );

                    pmxBone.AnimeVariables_Rotation.AddATransition( 
                        new BezierRotationAnimationTransition( frame.BoneRotation, Durationsec, frame.BezierCurve[ 3 ] ) );

                    PreviousFrameNumber = frame.FrameNumber;
                }
            }
        }

        /// <summary>
        ///     VMDのモーフフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFMorphFrameList">入力となるモーフフレームリスト。</param>
        /// <param name="PMXModel">対象となるPMXModel。</param>
        public static void AddAMorph( VMDFormat.MorphFrameList VMDFMorphFrameList, PMXModel PMXModel )
        {
            // すべてのモーフについて……
            for( int i = 0; i < PMXModel.MorphList.Length; i++ )
            {
                var pmxMorph = PMXModel.MorphList[ i ];

                
                // 同じモーフ名のフレームを列挙する。

                var morphFrames = VMDFMorphFrameList
                    .Where( ( frame ) => ( frame.MorphName == pmxMorph.PMXFMorph.MorphName ) ) // 同じ名前のフレームを、
                    .OrderBy( ( frame ) => frame.FrameNumber );                              // フレーム番号昇順に。


                // 列挙されたすべてのフレームについて……

                uint PreviousFrameNumber = 0;

                foreach( var frame in morphFrames )
                {
                    var Durationsec = ( frame.FrameNumber - PreviousFrameNumber ) / 30.0;   // 1frame = 1/30sec

                    pmxMorph.AnimeVariables_Morph.AddATransition( new LinearRealAnimationTransition( frame.MorphValue, Durationsec ) );

                    PreviousFrameNumber = frame.FrameNumber;
                }
            }
        }

        /// <summary>
        ///     VMDのボーンフレームリストからアニメ変数を構築する。
        /// </summary>
        /// <param name="VMDFCameraFrameList">入力となるカメラフレームリスト。</param>
        /// <param name="Camera">対象となるカメラ。</param>
        public static void AddCameraMotion( VMDFormat.CameraFrameList VMDFCameraFrameList, MotionCameraMMD Camera )
        {
            var cameraFrames = VMDFCameraFrameList
                .OrderBy( ( frame ) => frame.FrameNumber );  // フレーム番号昇順に。

            uint PreviousFrameNumber = 0;

            foreach( var frame in cameraFrames )
            {
                var Durationsec = ( frame.FrameNumber - PreviousFrameNumber ) / 30.0;   // 1frame = 1/30sec

                Camera.AnimeVariables.DistanceFromTheGazingPoint.AddATransition( new BezierRealNumberAnimationTransition( frame.Distance, Durationsec, frame.BezierCurve[ 4 ] ) );
                Camera.AnimeVariables.PositionOfGazingPoint.AddATransition( new BezierMovingAnimationTransition( frame.Position, Durationsec, frame.BezierCurve[ 0 ], frame.BezierCurve[ 1 ], frame.BezierCurve[ 2 ] ) );
                Camera.AnimeVariables.Rotationrad.AddATransition( new BezierMovingAnimationTransition( frame.Rotation, Durationsec, frame.BezierCurve[ 3 ], frame.BezierCurve[ 3 ], frame.BezierCurve[ 3 ] ) );
                Camera.AnimeVariables.ViewingAngledeg.AddATransition( new BezierRealNumberAnimationTransition( frame.ViewingAngle, Durationsec, frame.BezierCurve[ 5 ] ) );
                // todo: VMDカメラモーションのパースペクティブには未対応

                PreviousFrameNumber = frame.FrameNumber;
            }
        }
    }
}
