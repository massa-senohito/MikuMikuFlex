using System;
using System.Collections.Generic;
using MMF.モデル;
using MMF.モデル.PMX;
using MMF.モーフ;
using SharpDX;

namespace MMF.ボーン
{
	public class 手動によるFK変形更新 : 変形更新
	{
		public 手動によるFK変形更新( WeakReference<PMXModel> wrefModel )
        {
			this._wrefModel = wrefModel;
		}

		public bool 変形を更新する()
		{
            if( !_wrefModel.TryGetTarget( out PMXModel model ) )
                throw new InvalidOperationException( "モデルが破棄されています。" );

            var boneDictionary = model.スキニング.ボーンマップ;
			var morphManager = model.モーフ管理;

			foreach( var boneTransformer in _名前toボーン変形マップ )
			{
				var bone = boneDictionary[ boneTransformer.Key ];

                bone.回転 *= boneTransformer.Value.回転;
				bone.移動 += boneTransformer.Value.平行移動;
			}

            foreach( var morphTransformer in _名前toモーフ変形マップ )
			{
				morphManager.進捗率を設定する( morphTransformer.Value.モーフ値, morphTransformer.Key );
			}

            return true;
		}

		public ボーン変形 ボーン変形を検索して返す( string boneName )
		{
            if( !_wrefModel.TryGetTarget( out PMXModel model ) )
                throw new InvalidOperationException( "モデルが破棄されています。" );

            if( !model.スキニング.ボーンマップ.ContainsKey( boneName ) )
                throw new InvalidOperationException( "そのような名前のボーンは存在しません。" );

			if( _名前toボーン変形マップ.ContainsKey( boneName ) )
                return _名前toボーン変形マップ[ boneName ];

			var transformer = new ボーン変形( boneName, Quaternion.Identity, Vector3.Zero );
			_名前toボーン変形マップ.Add( transformer.ボーン名, transformer );
			return transformer;
		}

		public モーフ変形 モーフ変形を検索して返す( string morphName )
		{
			if( _名前toモーフ変形マップ.ContainsKey( morphName ) )
                return _名前toモーフ変形マップ[ morphName ];

			var transformer = new モーフ変形( morphName );
			_名前toモーフ変形マップ.Add( transformer.モーフ名, transformer );
			return transformer;
		}


        private Dictionary<string, ボーン変形> _名前toボーン変形マップ = new Dictionary<string, ボーン変形>();

        private Dictionary<string, モーフ変形> _名前toモーフ変形マップ = new Dictionary<string, モーフ変形>();

        private WeakReference<PMXModel> _wrefModel;
    }
}
