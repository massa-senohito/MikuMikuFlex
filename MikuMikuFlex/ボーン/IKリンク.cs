using System;
using MikuMikuFlex.Utility;
using SharpDX;

namespace MikuMikuFlex.ボーン
{
	public class IKリンク
	{
		public bool 回転制限がある = false;

		public Vector3 最大回転量;

		public Vector3 最小回転量;

		public int 現在のループ回数;

        public PMXボーン IKリンクボーン
            => ( _wrefスキニング.TryGetTarget( out スキニング skinnig ) ) ? skinnig.ボーン配列[ _IKリンクに対応するボーンのインデックス ] : null;


        public IKリンク( WeakReference<スキニング> wrefSkinning, MMDFileParser.PMXModelParser.ボーン.IKリンク linkData )
		{
			_wrefスキニング = wrefSkinning;
			_IKリンクに対応するボーンのインデックス = linkData.リンクボーンのボーンインデックス;

            回転制限がある = linkData.角度制限あり;

            // minとmaxを正しく読み込む
            Vector3 maxVec = linkData.角度制限の下限rad;
            Vector3 minVec = linkData.角度制限の上限rad;
            最小回転量 = new Vector3( Math.Min( maxVec.X, minVec.X ), Math.Min( maxVec.Y, minVec.Y ), Math.Min( maxVec.Z, minVec.Z ) );
            最大回転量 = new Vector3( Math.Max( maxVec.X, minVec.X ), Math.Max( maxVec.Y, minVec.Y ), Math.Max( maxVec.Z, minVec.Z ) );
			最小回転量 = Vector3.Clamp( 最小回転量, CGHelper.オイラー角の最小値, CGHelper.オイラー角の最大値 );
            最大回転量 = Vector3.Clamp( 最大回転量, CGHelper.オイラー角の最小値, CGHelper.オイラー角の最大値 );
		}


        private readonly int _IKリンクに対応するボーンのインデックス;

        private WeakReference<スキニング> _wrefスキニング;
    }
}
