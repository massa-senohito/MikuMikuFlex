using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMF.モデル.Shape;
using SharpDX;

namespace MMF.モデル.Controller.ControllerComponent
{
	class CenterCrossLine : IDrawable
	{
        public bool 表示中 { get; set; }

        public string ファイル名 { get; private set; }

        public int サブセット数 { get; private set; }

        public int 頂点数 { get; private set; }

        public モデル状態 モデル状態 { get; private set; }

        public Vector4 セルフシャドウ色 { get; set; }

        public Vector4 地面影色 { get; set; }


        public CenterCrossLine()
		{
			_xLine = new キューブシェイプ( new Vector4( 1, 0.55f, 0, 0.7f ) );
			_yLine = new キューブシェイプ( new Vector4( 1, 0.55f, 0, 0.7f ) );
			_zLine = new キューブシェイプ( new Vector4( 1, 0.55f, 0, 0.7f ) );

            _xLine.初期化する();
			_yLine.初期化する();
			_zLine.初期化する();

            _xLine.モデル状態.倍率 = new Vector3( 30f, _thickness, _thickness );
			_yLine.モデル状態.倍率 = new Vector3( _thickness, 30f, _thickness );
			_zLine.モデル状態.倍率 = new Vector3( _thickness, _thickness, 30f );
		}

        public void Dispose()
        {
            _xLine.Dispose();
            _yLine.Dispose();
            _zLine.Dispose();
        }

        public void AddTranslation( Vector3 trans )
		{
			_xLine.モデル状態.位置 += trans;
			_yLine.モデル状態.位置 += trans;
			_zLine.モデル状態.位置 += trans;
		}

		public void 描画する()
		{
			_xLine.描画する();
			_yLine.描画する();
			_zLine.描画する();
		}

		public void 更新する()
		{
		}


        private キューブシェイプ _xLine;

        private キューブシェイプ _yLine;

        private キューブシェイプ _zLine;

        private static float _thickness = 0.1f;
    }
}
