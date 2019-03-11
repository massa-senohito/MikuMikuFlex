using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;

namespace MikuMikuFlex.物理演算
{
	internal class 物理演算を超越した特性
	{
		public bool 物理演算の影響を受けないKinematic剛体である { private set; get; }

        public CollisionFilterGroups 自身の衝突グループ番号 { private set; get; }

        public CollisionFilterGroups 自身と衝突する他の衝突グループ番号 { private set; get; }


		public 物理演算を超越した特性(
            bool 物理演算の影響を受けないKinematic剛体である = false, 
            CollisionFilterGroups 自身の衝突グループ番号 = CollisionFilterGroups.DefaultFilter,
            CollisionFilterGroups 自身と衝突する他の衝突グループ番号 = CollisionFilterGroups.AllFilter )
		{
			this.物理演算の影響を受けないKinematic剛体である = 物理演算の影響を受けないKinematic剛体である;
			this.自身の衝突グループ番号 = 自身の衝突グループ番号;
			this.自身と衝突する他の衝突グループ番号 = 自身と衝突する他の衝突グループ番号;
		}
	}
}
