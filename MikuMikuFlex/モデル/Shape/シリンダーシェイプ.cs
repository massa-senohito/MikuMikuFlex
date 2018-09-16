using System;
using System.Collections.Generic;
using SharpDX;

namespace MikuMikuFlex.モデル.Shape
{
	public class シリンダーシェイプ : シェイプ
	{
        public class SilinderShapeDescription
        {
            public float Thickness { get; }

            public uint DivideCount { get; }

            public SilinderShapeDescription( float thickness, uint divideCount )
            {
                this.Thickness = thickness;
                this.DivideCount = divideCount;
            }
        }


        public override string ファイル名 => "@@@SilinderShape@@@";

        public override int 頂点数 => (int) ( _desc.DivideCount * 6 * 4 );


        public シリンダーシェイプ( Vector4 color, SilinderShapeDescription desc ) 
            : base( color )
		{
			_desc = desc;
		}

		protected override void InitializeIndex( インデックスバッファBuilder builder )
		{
			uint n = _desc.DivideCount + 1;

            // 上のリング
            for( uint i = 0; i < _desc.DivideCount; i++ )
				builder.四角形を追加する( i, i + 1, ( i + 1 ) + n, i + n );

            // 下のリング
            for( uint i = 0; i < _desc.DivideCount; i++ )
				builder.四角形を追加する( i + n * 2, i + n * 3, ( i + 1 ) + n * 3, ( i + 1 ) + n * 2 );

            // 内面
            for( uint i = 0; i < _desc.DivideCount; i++ )
				builder.四角形を追加する( i, i + n * 2, ( i + 1 ) + n * 2, ( i + 1 ) );

            // 内面
            for( uint i = 0; i < _desc.DivideCount; i++ )
				builder.四角形を追加する( i + n, i + 1 + n, ( i + 1 ) + n * 3, i + n * 3 );
		}

		protected override void InitializePositions( List<Vector4> positions )
		{
			_リングを追加する( positions, 1, 1 );
			_リングを追加する( positions, 1, _desc.Thickness + 1f );
			_リングを追加する( positions, -1, 1 );
			_リングを追加する( positions, -1, _desc.Thickness + 1f );
		}


        private readonly SilinderShapeDescription _desc;

        private void _リングを追加する( List<Vector4> positions, float y, float r )
		{
			float stride = (float) ( 2 * Math.PI / _desc.DivideCount );

			for( int i = 0; i <= _desc.DivideCount; i++ )
				positions.Add( new Vector4( (float) ( Math.Cos( i * stride ) * r ), y, (float) ( Math.Sin( i * stride ) * r ), 1f ) );
		}
	}
}