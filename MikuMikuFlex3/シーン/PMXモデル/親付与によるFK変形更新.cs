using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3
{
	class ByParentalGrantFKDeformationUpdate
    {

        // 生成と終了


		public ByParentalGrantFKDeformationUpdate( PMXBoneControl[] AllBones )
		{
            this._BoneArray = new HierarchicalList<PMXBoneControl>(
                AllBones,
                ( child ) => ( child.PMXFBourne.GrantedToMove || child.PMXFBourne.RotationIsGranted ) ? child.PMXFBourne.GrantedParentBoneIndex : -1,
                ( target ) => ( target.BoneIndex ) );
        }



        // 更新


		public void UpdateTransformation()
		{
			foreach( var pmxBone in _BoneArray )
			{
				if( pmxBone.PMXFBourne.GrantedToMove )
				{
					var pp = _BoneArray[ pmxBone.PMXFBourne.GrantedParentBoneIndex ];
					pmxBone.Move += Vector3.Lerp( Vector3.Zero, pp.Move, pmxBone.PMXFBourne.GrantRate );
				}

				if( pmxBone.PMXFBourne.RotationIsGranted )
				{
					var pp = _BoneArray[ pmxBone.PMXFBourne.GrantedParentBoneIndex ];
					pmxBone.Rotation *= Quaternion.Slerp( Quaternion.Identity, pp.Rotation, pmxBone.PMXFBourne.GrantRate );
				}
			}
		}



        // private


        private HierarchicalList<PMXBoneControl> _BoneArray;
	}
}
