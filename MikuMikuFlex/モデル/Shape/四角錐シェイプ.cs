using System.Collections.Generic;
using MikuMikuFlex.Utility;
using SharpDX;

namespace MikuMikuFlex.モデル.Shape
{
	public class 四角錐シェイプ : シェイプ
	{
		public 四角錐シェイプ( Vector4 color )
            : base( color )
		{
		}

		public override string ファイル名 => "@@@ConeShape@@@";

		public override int 頂点数 => 18;

		protected override void InitializeIndex( インデックスバッファBuilder builder )
		{
			builder.四角形を追加する( 4, 3, 2, 1 );
			builder.三角形を追加する( 0, 3, 4 );
			builder.三角形を追加する( 0, 2, 3 );
			builder.三角形を追加する( 0, 1, 2 );
			builder.三角形を追加する( 0, 4, 1 );
		}

		protected override void InitializePositions( List<Vector4> positions )
		{
			positions.Add( new Vector4( 0, 1, 0, 1 ) );
			positions.Add( new Vector4( -1, -1, 0, 1 ) );
			positions.Add( new Vector4( 0, -1, 1, 1 ) );
			positions.Add( new Vector4( 1, -1, 0, 1 ) );
			positions.Add( new Vector4( 0, -1, -1, 1 ) );
		}
	}
}