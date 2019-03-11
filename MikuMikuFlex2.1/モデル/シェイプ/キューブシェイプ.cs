
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MikuMikuFlex.モデル.シェイプ
{
	public class キューブシェイプ : シェイプ
	{
		public キューブシェイプ( Vector4 色 )
            : base( 色 )
		{
		}

		public override string ファイル名 => "@@@CubeShape@@@";

		public override int 頂点数 => 36;

		protected override void InitializeIndex( インデックスバッファBuilder builder )
		{
			builder.四角形を追加する( 0, 1, 2, 3 );
			builder.四角形を追加する( 6, 5, 4, 7 );
			builder.四角形を追加する( 0, 3, 7, 4 );
			builder.四角形を追加する( 2, 1, 5, 6 );
			builder.四角形を追加する( 3, 2, 6, 7 );
			builder.四角形を追加する( 1, 0, 4, 5 );
		}

		protected override void InitializePositions( List<Vector4> positions )
		{
			positions.Add( new Vector4( -1, 1, -1, 1 ) );
			positions.Add( new Vector4( -1, 1, 1, 1 ) );
			positions.Add( new Vector4( 1, 1, 1, 1 ) );
			positions.Add( new Vector4( 1, 1, -1, 1 ) );
			positions.Add( new Vector4( -1, -1, -1, 1 ) );
			positions.Add( new Vector4( -1, -1, 1, 1 ) );
			positions.Add( new Vector4( 1, -1, 1, 1 ) );
			positions.Add( new Vector4( 1, -1, -1, 1 ) );
		}
	}
}
