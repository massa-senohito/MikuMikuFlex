using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3
{
    /// <summary>
    ///     <see cref="PMXFormat.Bourne"/> に追加情報を付与するクラス。
    /// </summary>
    public class PMXBoneControl : IDisposable
    {

        // 基本情報


        public string GivenNames => this.PMXFBourne.BoneName;

        public string GivenNames_English => this.PMXFBourne.BoneName_English;

        public PMXBoneControl ParentBone { get; set; }

        public List<PMXBoneControl> ChildBoneList { get; protected set; }

        internal PMXFormat.Bourne PMXFBourne { get; private protected set; }

        internal int BoneIndex { get; private protected set; }

        internal PMXBoneControl IKTargetBone { get; private protected set; }

        internal IKLink[] IKLinkList { get; private protected set; }

        internal int TransformationHierarchy { get; set; }



        // 動的情報（入力）


        public Vector3 LocalLocation { get; set; }

        public Vector3 Move { get; set; }

        public Quaternion Rotation
        {
            get { return _Rotation; }
            set
            {
                _Rotation = value;
                _Rotation.Normalize();
            }
        }

        public AnimeVariables<Vector3> AnimeVariables_Move { get; protected set; }

        public AnimeVariables<Quaternion> AnimeVariables_Rotation { get; protected set; }



        // 動的情報（出力）


        internal Matrix ModelPoseMatrix { get; private protected set; }

        internal Matrix LocalPoseMatrix { get; private protected set; }



        // 生成と終了


        public PMXBoneControl( PMXFormat.Bourne bone, int index )
        {
            this.PMXFBourne = bone;
            this.BoneIndex = index;
            this.ParentBone = null;
            this.ChildBoneList = new List<PMXBoneControl>();
            this.LocalLocation = bone.Position;
            this.Move = Vector3.Zero;
            this.Rotation = Quaternion.Identity;
            this.ModelPoseMatrix = Matrix.Identity;
            this.LocalPoseMatrix = Matrix.Identity;
            this.AnimeVariables_Move = new AnimeVariables<Vector3>( Vector3.Zero );
            this.AnimeVariables_Rotation = new AnimeVariables<Quaternion>( Quaternion.Identity );
        }

        internal void PerformPostReadingProcessing( PMXBoneControl[] AllBones )
        {
            // 子ボーンとの階層化

            for( int i = 0; i < AllBones.Length; i++ )
            {
                if( AllBones[ i ].PMXFBourne.ParentBoneIndex == this.BoneIndex )
                {
                    AllBones[ i ].ParentBone = this;
                    this.ChildBoneList.Add( AllBones[ i ] );
                }
            }


            // IK

            if( this.PMXFBourne.IKBone )
            {
                this.IKTargetBone = AllBones[ this.PMXFBourne.IKTargetBoneIndex ];

                this.IKLinkList = new IKLink[ this.PMXFBourne.IKLinkList.Count ];
                for( int i = 0; i < this.PMXFBourne.IKLinkList.Count; i++ )
                {
                    this.IKLinkList[ i ] = new IKLink( this.PMXFBourne.IKLinkList[ i ] ) {
                        IKLinkBone = AllBones[ this.PMXFBourne.IKLinkList[ i ].LinkBoneBoneIndex ],
                    };
                }
            }
        }

        public virtual void Dispose()
        {
            this.PMXFBourne = null;
            this.ParentBone = null;
            this.ChildBoneList = null;
        }



        // 更新と出力


        internal void ApplyBoneMotion( double CurrentTimesec )
        {
            this.Move += this.AnimeVariables_Move.Update( CurrentTimesec );
            this.Rotation *= this.AnimeVariables_Rotation.Update( CurrentTimesec );
        }

        internal void CalculateModelPose()
        {
            // ポーズ計算。

            this.LocalPoseMatrix =
                Matrix.Translation( -this.LocalLocation ) *    // 原点に戻って
                Matrix.RotationQuaternion( this.Rotation ) *      // 回転して
                Matrix.Translation( this.Move ) *             // 平行移動したのち
                Matrix.Translation( this.LocalLocation );      // 元の位置に戻す

            this.ModelPoseMatrix =
                this.LocalPoseMatrix *
                ( this.ParentBone?.ModelPoseMatrix ?? Matrix.Identity );    // 親ボーンがあるなら親ボーンのモデルポーズを反映。


            // すべての子ボーンについても更新。

            foreach( var ChildBone in this.ChildBoneList )
                ChildBone.CalculateModelPose();
        }

        internal void ConfirmTheState( Matrix[] ModelPoseArray, Vector4[] LocalLocationArray, Vector4[] RotationalArray )
        {
            ModelPoseArray[ this.BoneIndex ] = this.ModelPoseMatrix;
            ModelPoseArray[ this.BoneIndex ].Transpose(); // エフェクトを介さない場合は自分で転置する必要がある。
            LocalLocationArray[ this.BoneIndex ] = new Vector4( this.LocalLocation, 0f );
            RotationalArray[ this.BoneIndex ] = new Vector4( this.Rotation.ToArray() );  // Quaternion → Vector4

            // すべての子ボーンについても更新。

            foreach( var ChildBone in this.ChildBoneList )
                ChildBone.ConfirmTheState( ModelPoseArray, LocalLocationArray, RotationalArray );
        }



        // private


        private Quaternion _Rotation;


        public class IKLink
        {
            public PMXBoneControl IKLinkBone;

            public bool ThereIsARotationLimit => this._ikLink.ThereIsAnAngleLimit;

            public Vector3 MaximumRotationAmount { get; }

            public Vector3 MinimumRotationAmount { get; }


            public IKLink( PMXFormat.Bourne.IKLink ikLink )
            {
                this._ikLink = ikLink;

                // minとmaxを正しく読み込む
                Vector3 maxVec = ikLink.UpperLimitOfAngleLimitrad;
                Vector3 minVec = ikLink.LowerLimitOfAngleLimitrad; 
                this.MinimumRotationAmount = new Vector3( Math.Min( maxVec.X, minVec.X ), Math.Min( maxVec.Y, minVec.Y ), Math.Min( maxVec.Z, minVec.Z ) );
                this.MaximumRotationAmount = new Vector3( Math.Max( maxVec.X, minVec.X ), Math.Max( maxVec.Y, minVec.Y ), Math.Max( maxVec.Z, minVec.Z ) );
                this.MinimumRotationAmount = Vector3.Clamp( MinimumRotationAmount, CGHelper.MinimumEulerAngles, CGHelper.MaximumEulerAngles );
                this.MaximumRotationAmount = Vector3.Clamp( MaximumRotationAmount, CGHelper.MinimumEulerAngles, CGHelper.MaximumEulerAngles );
            }


            private PMXFormat.Bourne.IKLink _ikLink;
        }
    }
}
