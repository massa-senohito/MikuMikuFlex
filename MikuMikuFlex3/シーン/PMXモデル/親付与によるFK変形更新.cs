using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex3
{
	class 親付与によるFK変形更新
    {

        // 生成と終了


		public 親付与によるFK変形更新( PMXボーン制御[] 全ボーン )
		{
            this._ボーン配列 = new 階層順リスト<PMXボーン制御>(
                全ボーン,
                ( child ) => ( child.PMXFボーン.移動付与される || child.PMXFボーン.回転付与される ) ? child.PMXFボーン.付与親ボーンインデックス : -1,
                ( target ) => ( target.ボーンインデックス ) );
        }



        // 更新


		public void 変形を更新する()
		{
			foreach( var pmxBone in _ボーン配列 )
			{
				if( pmxBone.PMXFボーン.移動付与される )
				{
					var pp = _ボーン配列[ pmxBone.PMXFボーン.付与親ボーンインデックス ];
					pmxBone.移動 += Vector3.Lerp( Vector3.Zero, pp.移動, pmxBone.PMXFボーン.付与率 );
				}

				if( pmxBone.PMXFボーン.回転付与される )
				{
					var pp = _ボーン配列[ pmxBone.PMXFボーン.付与親ボーンインデックス ];
					pmxBone.回転 *= Quaternion.Slerp( Quaternion.Identity, pp.回転, pmxBone.PMXFボーン.付与率 );
				}
			}
		}



        // private


        private 階層順リスト<PMXボーン制御> _ボーン配列;
	}
}
