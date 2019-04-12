using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX;  
using SharpDX.Direct3D11;

namespace MikuMikuFlex3
{
    public class シーン
    {
        public List<カメラ> カメラリスト { get; protected set; } = new List<カメラ>();

        public カメラ 選択中のカメラ { get; set; }

        public List<照明> 照明リスト { get; protected set; } = new List<照明>();

        public List<パス> パスリスト { get; protected set; } = new List<パス>();


        public シーン()
        {
            this.パスリスト = new List<パス>();
        }


        public void 描画する( double 現在時刻sec, DeviceContext d3ddc, GlobalParameters globalParameters )
        {
            // カメラを進行する。

            this.選択中のカメラ.更新する( 現在時刻sec );


            // GlobalParameters の設定（シーン単位）

            globalParameters.ViewMatrix = this.選択中のカメラ.ビュー行列を取得する();
            globalParameters.ViewMatrix.Transpose();
            globalParameters.ProjectionMatrix = this.選択中のカメラ.射影行列を取得する();
            globalParameters.ProjectionMatrix.Transpose();
            globalParameters.CameraPosition = new Vector4( this.選択中のカメラ.位置, 0f );
            globalParameters.Light1Direction = new Vector4( this.照明リスト[ 0 ].照射方向, 0f );


            // リストの先頭から順番にパスを描画。

            for( int i = 0; i < this.パスリスト.Count; i++ )
            {
                this.パスリスト[ i ].描画する( 現在時刻sec, d3ddc, globalParameters );
            }
        }
    }
}
