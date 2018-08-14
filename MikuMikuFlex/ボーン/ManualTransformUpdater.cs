using System;
using System.Collections.Generic;
using MMF.Model;
using MMF.Model.PMX;
using MMF.Morph;
using SharpDX;

namespace MMF.Bone
{
	public class ManualTransformUpdater : I変化量更新
	{
		private Dictionary<string, ボーン変換係数> updaters = new Dictionary<string, ボーン変換係数>();

		private Dictionary<string, モーフ変換係数> morphUpdaters = new Dictionary<string, モーフ変換係数>();

		private PMXModel model;

		public ManualTransformUpdater( PMXModel model )
		{
			this.model = model;
		}

		/// <summary>
		///  ITransformUpdaterメンバーの実装
		/// </summary>
		public bool 変化量を更新する()
		{
			var boneDictionary = model.Skinning.BoneDictionary;
			var morphManager = model.Morphmanager;
			foreach( var boneTransformer in updaters )
			{
				var bone = boneDictionary[ boneTransformer.Key ];
				bone.回転行列 *= boneTransformer.Value.回転行列;
				bone.平行移動位置ベクタ += boneTransformer.Value.平行移動位置ベクタ;
			}
			foreach( var morphTransformer in morphUpdaters )
			{
				morphManager.ApplyMorphProgress( morphTransformer.Value.モーフ値, morphTransformer.Key );
			}
			return true;
		}

		/// <summary>
		///		指定された名前に対応するボーン変換係数を辞書から検索して返す。
		///		辞書に未登録なら、新しく生成・登録して返す。
		/// </summary>
		/// <param name="ボーン名">
		///		取得するボーンの名前。
		///	</param>
		/// <returns>
		///		辞書から取得または新しく生成されたボーン変換係数。
		/// </returns>
		public ボーン変換係数 ボーン変換係数を取得する( string ボーン名 )
		{
			ボーン変換係数 transformer;

			if( !model.Skinning.BoneDictionary.ContainsKey( ボーン名 ) )
				throw new InvalidOperationException( "そのような名前のボーンは存在しません。" );

			if( updaters.ContainsKey( ボーン名 ) )
			{
				transformer = updaters[ ボーン名 ];
			}
			else
			{
				transformer = new ボーン変換係数( ボーン名, Quaternion.Identity, Vector3.Zero );
				this.updaters.Add( transformer.ボーン名, transformer );
			}

			return transformer;
		}

		/// <summary>
		///		指定された名前に対応するモーフ変換係数を辞書から検索して返す。
		///		辞書に未登録なら、新しく生成・登録して返す。
		/// </summary>
		/// <param name="モーフ名">
		///		取得するモーフの名前。
		///	</param>
		/// <returns>
		///		辞書から取得または新しく生成されたモーフ変換係数。
		/// </returns>
		public モーフ変換係数 モーフ変換係数を取得する( string モーフ名 )
		{
			モーフ変換係数 transformer;

			if( morphUpdaters.ContainsKey( モーフ名 ) )
			{
				transformer = morphUpdaters[ モーフ名 ];
			}
			else
			{
				// 存在しないモーフ名なら、新しく生成・登録して返す。

				transformer = new モーフ変換係数( モーフ名 );
				this.morphUpdaters.Add( transformer.モーフ名, transformer );
			}

			return transformer;
		}
	}
}
