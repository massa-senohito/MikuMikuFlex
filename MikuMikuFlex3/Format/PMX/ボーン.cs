using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    public class Bourne
    {
        public string BoneName { get; private set; }

        public string BoneName_English { get; private set; }


        // 接続元（Parent）

        public int ParentBoneIndex { get; private set; }


        // Position

        public Vector3 Position { get; private set; }


        // 方向（Rotation、"＞" の表示先）

        public HowToDisplayTheConnectionDestinationOfBones HowToDisplayTheConnectionDestinationOfBones { get; private set; }

        /// <summary>
        ///     ボーンの方向を、ボーンの位置からの相対位置で指定する。
        ///     <see cref="HowToDisplayTheConnectionDestinationOfBones"/> が <see cref="HowToDisplayTheConnectionDestinationOfBones.SpecifiedByRelativeCoordinates"/> である場合に有効。
        /// </summary>
        public Vector3 RelativePositionFromBonePosition { get; private set; }

        /// <summary>
        ///     ボーンの方向を、別のボーンの位置で指定する。
        ///     <see cref="HowToDisplayTheConnectionDestinationOfBones"/> が <see cref="HowToDisplayTheConnectionDestinationOfBones.SpecifiedByBone"/> である場合に有効。
        /// </summary>
        public int BoneIndexOfTheConnectedBone { get; private set; }


        // ボーンの性能（属性）

        /// <summary>
        ///     true なら、ボーンの位置は変化せず、方向（Rotation）だけが変化する。
        /// </summary>
        public bool ItIsRotatable { get; private set; }

        /// <summary>
        ///     true なら、ボーンの方向（Rotation）は変化せず、位置だけが変化する。
        /// </summary>
        public bool Movable { get; private set; }

        /// <summary>
        ///     IKの目標点である？
        /// </summary>
        /// <remarks>
        ///     true の場合、このボーンは、モデルの一部を構成する通常のボーンとは異なり、IKのターゲット（目標点）となる専用のボーンである。
        /// </remarks>
        public bool IKBone { get; private set; }

        /// <summary>
        ///     ボーンの表示・選択が可能？
        /// </summary>
        public bool CanBeDisplayed { get; private set; }

        /// <summary>
        ///     選択時に操作可能？
        /// </summary>
        public bool CanBeOperated { get; private set; }

        
        // TransformationHierarchy
        // 　変形は、初めにボーン番号の順番に行い、その後、IKが順番に行われる。
        // 　変形階層を使うと、この順番をある程度変えることができる。
        // 　変形階層を考慮すると、変換の順番は以下のようになる。
        // 　　１．物理変形前（変形階層順）
        //   　２．Physics
        // 　　３．物理変形後（変形階層順）
        // 　従って、「物理変形の前か後か」と「変形階層番号」の２つで変形順を設定する。

        public bool PostPhysicalDeformation { get; private set; }

        public int TransformationHierarchy { get; private set; }


        // 付与
        // 　ボーンの方向や位置に、付与親ボーンの回転量や移動量を加算すること。
        // 　付与される回転量と移動量は GrantRate により増減することができる。（負数も指定可能。）
        // 　注意：付与親と付与率は、回転付与と移動付与の両方で共通で、１つだけ指定可能。

        /// <summary>
        ///     true の場合、付与親の方向の変化量（回転の変化量）に <see cref="GrantRate"/> を乗じた値が、このボーンの方向にも付与される。
        /// </summary>
        /// <remarks>
        ///     例：ボーンの方向 (10,20,30)
        ///     　　付与親の方向 (100,200,300)
        ///     　　GrantRate 0.2
        ///     　　のとき、付与親の方向が (+1, +10, -6) 変化すると、
        ///     　　ボーンの方向は (10 + (+1×0.2), 20 + (+10×0.2), 30 + (-6×0.2)) = (10.2, 22, 28.8) となる。
        /// </remarks>
        public bool RotationIsGranted { get; private set; }

        /// <summary>
        ///     true の場合、付与親の位置の変化量（AmountOfMovement）に <see cref="GrantRate"/> を乗じた値が、このボーンの位置にも付与される。
        /// </summary>
        /// <remarks>
        ///     例：BonePosition (10,20,30)
        ///     　　付与親の位置 (100,200,300)
        ///     　　GrantRate 0.2
        ///     　　のとき、付与親の位置が (+1, +10, -6) 変化すると、
        ///     　　ボーンの位置は (10 + (+1×0.2), 20 + (+10×0.2), 30 + (-6×0.2)) = (10.2, 22, 28.8) となる。
        /// </remarks>
        public bool GrantedToMove { get; private set; }

        /// <summary>
        ///     回転・移動の付与親となるボーン。
        ///     <see cref="RotationIsGranted"/> または <see cref="GrantedToMove"/> が true の場合に有効。
        /// </summary>
        public int GrantedParentBoneIndex { get; private set; }

        /// <summary>
        ///     <see cref="RotationIsGranted"/> または <see cref="GrantedToMove"/> が true の場合に有効。
        /// </summary>
        public float GrantRate { get; private set; }


        // 軸制限
        // 　通常、ボーンは X, Y, Z の３軸にそって回転することができるが、
        // 　これを、指定した１Axis（回転軸）にそってのみ回転できるように制限すること。

        /// <summary>
        ///     true なら、軸制限を行う。
        /// </summary>
        public bool ThereIsAnAxisLimit { get; private set; }

        /// <summary>
        ///     回転軸を表すベクトル。
        ///     <see cref="ThereIsAnAxisLimit"/> が true のときのみ有効。
        /// </summary>
        public Vector3 DirectionVectorOfRotationAxis { get; private set; }


        // ローカル軸
        // 　ボーン操作時のX,Y,Z軸を指定する。
        // 　軸の指定は X, Z しかなく、この２つから Y 軸は自動的に決定される。

        public bool WithLocalAxis { get; private set; }

        public LocalGrantTarget LocalGrantTarget { get; private set; }

        public Vector3 OnTheLocalAxisXAxisDirectionVector { get; private set; }  // WithLocalAxis である場合

        public Vector3 OnTheLocalAxisZAxisDirectionVector { get; private set; }  // WithLocalAxis である場合


        // 外部親 (PMX2.1)
        // 　このボーンを、別のモデル（外部親；具体的には番号 0 のボーン）に追従するよう設定することができる。

        /// <summary>
        ///     true の場合、このボーンは外部親に追従する。
        /// </summary>
        public bool ExternalParentTransformation { get; private set; }  // Todo: 外部親に対応する

        /// <summary>
        ///     Unused。
        ///     <see cref="ExternalParentTransformation"/> が true のときに指定することができるが、どのソフトもまだ対応していない。
        /// </summary>
        public int ParentKey { get; private set; }


        //--------------- IK 関連 -------------------

        /// <summary>
        ///     TargetBone。
        ///     <see cref="IKLinkList"/> の先端のボーンを近づける目標となるボーン。
        /// </summary>
        public int IKTargetBoneIndex { get; private set; }

        /// <summary>
        ///     IK の反復演算回数。
        ///     PMD及びMMD環境では255回が最大になるようです。
        /// </summary>
        public int IKNumberOfLoops { get; private set; }

        /// <summary>
        ///     IKループ計算時の1回あたりの制限角度[ラジアン]。
        ///     PMDのIK値とは4倍異なるので注意。
        /// </summary>
        public float IKUnitAnglerad { get; private set; }

        /// <summary>
        ///     IKの影響を受けるボーンのリスト。
        /// </summary>
        public List<IKLink> IKLinkList { get; private set; }

        public class IKLink
        {
            public int LinkBoneBoneIndex { get; private set; }

            public bool ThereIsAnAngleLimit { get; private set; }

            public Vector3 LowerLimitOfAngleLimitrad { get; private set; }     // ThereIsAnAngleLimit の場合

            public Vector3 UpperLimitOfAngleLimitrad { get; private set; }     // ThereIsAnAngleLimit の場合


            /// <summary>
            ///     指定されたストリームから読み込む。
            /// </summary>
            internal IKLink( Stream st, Header header )
            {
                this.LinkBoneBoneIndex = ParserHelper.get_Index( st, header.BoneIndexSize );
                this.ThereIsAnAngleLimit = ParserHelper.get_Byte( st ) == 1 ? true : false;

                if( this.ThereIsAnAngleLimit )
                {
                    this.LowerLimitOfAngleLimitrad = ParserHelper.get_Float3( st );
                    this.UpperLimitOfAngleLimitrad = ParserHelper.get_Float3( st );
                }
            }
        }


        public Bourne()
        {
        }

        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal Bourne( Stream st, Header header )
        {
            this.IKLinkList = new List<IKLink>();
            this.BoneName = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.BoneName_English = ParserHelper.get_TextBuf( st, header.EncodingMethod );
            this.Position = ParserHelper.get_Float3( st );
            this.ParentBoneIndex = ParserHelper.get_Index( st, header.BoneIndexSize );
            this.TransformationHierarchy = ParserHelper.get_Int( st );

            var flag = new byte[ 2 ];
            flag[ 0 ] = ParserHelper.get_Byte( st );
            flag[ 1 ] = ParserHelper.get_Byte( st );
            Int16 flagnum = BitConverter.ToInt16( flag, 0 );
            this.HowToDisplayTheConnectionDestinationOfBones = ParserHelper.isFlagEnabled( flagnum, 0x0001 ) ? HowToDisplayTheConnectionDestinationOfBones.SpecifiedByBone : HowToDisplayTheConnectionDestinationOfBones.SpecifiedByRelativeCoordinates;
            this.ItIsRotatable = ParserHelper.isFlagEnabled( flagnum, 0x0002 );
            this.Movable = ParserHelper.isFlagEnabled( flagnum, 0x0004 );
            this.CanBeDisplayed = ParserHelper.isFlagEnabled( flagnum, 0x0008 );
            this.CanBeOperated = ParserHelper.isFlagEnabled( flagnum, 0x0010 );
            this.IKBone = ParserHelper.isFlagEnabled( flagnum, 0x0020 );
            this.LocalGrantTarget = ParserHelper.isFlagEnabled( flagnum, 0x0080 ) ? LocalGrantTarget.ParentsLocalDeformation : LocalGrantTarget.UserTransformationValue_IKLink_MultipleGrant;
            this.RotationIsGranted = ParserHelper.isFlagEnabled( flagnum, 0x0100 );
            this.GrantedToMove = ParserHelper.isFlagEnabled( flagnum, 0x0200 );
            this.ThereIsAnAxisLimit = ParserHelper.isFlagEnabled( flagnum, 0x0400 );
            this.WithLocalAxis = ParserHelper.isFlagEnabled( flagnum, 0x0800 );
            this.PostPhysicalDeformation = ParserHelper.isFlagEnabled( flagnum, 0x1000 );
            this.ExternalParentTransformation = ParserHelper.isFlagEnabled( flagnum, 0x2000 );

            if( this.HowToDisplayTheConnectionDestinationOfBones == HowToDisplayTheConnectionDestinationOfBones.SpecifiedByRelativeCoordinates )
            {
                this.RelativePositionFromBonePosition = ParserHelper.get_Float3( st );
            }
            else
            {
                this.BoneIndexOfTheConnectedBone = ParserHelper.get_Index( st, header.BoneIndexSize );
            }

            if( this.RotationIsGranted || this.GrantedToMove )
            {
                this.GrantedParentBoneIndex = ParserHelper.get_Index( st, header.BoneIndexSize );
                this.GrantRate = ParserHelper.get_Float( st );
            }

            if( this.ThereIsAnAxisLimit )
                this.DirectionVectorOfRotationAxis = ParserHelper.get_Float3( st );

            if( this.WithLocalAxis )
            {
                this.OnTheLocalAxisXAxisDirectionVector = ParserHelper.get_Float3( st );
                this.OnTheLocalAxisZAxisDirectionVector = ParserHelper.get_Float3( st );
            }

            if( this.ExternalParentTransformation )
                this.ParentKey = ParserHelper.get_Int( st );

            if( this.IKBone )
            {
                this.IKTargetBoneIndex = ParserHelper.get_Index( st, header.BoneIndexSize );
                this.IKNumberOfLoops = ParserHelper.get_Int( st );
                this.IKUnitAnglerad = ParserHelper.get_Float( st );
                int IKNumberOfLinks = ParserHelper.get_Int( st );
                for( int i = 0; i < IKNumberOfLinks; i++ )
                    this.IKLinkList.Add( new IKLink( st, header ) );
            }

            if( this.GrantedParentBoneIndex == -1 )
            {
                this.RotationIsGranted = false;
                this.GrantedToMove = false;
            }

            if( !this.GrantedToMove && !this.RotationIsGranted )
                this.GrantedParentBoneIndex = -1;
        }
    }
}
