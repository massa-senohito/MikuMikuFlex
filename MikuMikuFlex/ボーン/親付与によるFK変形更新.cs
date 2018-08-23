using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikuMikuFlex.モーフ;
using MikuMikuFlex.Utility;
using SharpDX;

namespace MikuMikuFlex.ボーン
{
	class 親付与によるFK変形更新 : 変形更新
	{
		public 親付与によるFK変形更新( PMXボーン[] bones )
		{
            this._ボーン配列 = new 階層順リスト<PMXボーン>(
                bones,
                ( child ) => ( !child.移動付与される && child.回転付与される ) ? -1 : child.付与親ボーンインデックス,
                ( target ) => ( target.ボーンインデックス ) );
        }

		public bool 変形を更新する()
		{
			foreach( var pmxBone in _ボーン配列 )
			{
				if( pmxBone.移動付与される )
				{
					var pp = _ボーン配列[ pmxBone.付与親ボーンインデックス ];
					pmxBone.移動 += Vector3.Lerp( Vector3.Zero, pp.移動, pmxBone.付与率 );
				}

				if( pmxBone.回転付与される )
				{
					var pp = _ボーン配列[ pmxBone.付与親ボーンインデックス ];
					pmxBone.回転 *= Quaternion.Slerp( Quaternion.Identity, pp.回転, pmxBone.付与率 );
				}
			}
			return true;
		}


        private 階層順リスト<PMXボーン> _ボーン配列;
	}
}
