using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MMDFileParser.OpenMMDFormat;

namespace MikuMikuFlex
{
	/// <summary>
	/// VME用モーションプロバイダ
	/// </summary>
	public class MMDモーションforVME : モーション
	{
		/// <summary>
		/// VME用モーション構造体
		/// </summary>
		private VocaloidMotionEvolved vocaloidMotionEvolved;

		/// <summary>
		/// ボーン
		/// </summary>
		private PMXボーン[] bones;

		/// <summary>
		/// "全ての親"を無視するか否か
		/// </summary>
		private bool ignoreParent;

		/// <summary>
		/// ボーンのモーションの集合
		/// </summary>
		private readonly List<ボーンモーションforVME> boneMotions = new List<ボーンモーションforVME>();

		/// <summary>
		/// モーフのモーションの集合
		/// </summary>
		private readonly List<モーフモーションforVME> morphMotions = new List<モーフモーションforVME>();

        /// <summary>
        /// 再生中か否か
        /// </summary>
        private bool isPlaying = false;

		/// <summary>
		/// モーション再生終了後のアクション
		/// </summary>
		private モーション再生終了後の挙動 actionAfterMotion = モーション再生終了後の挙動.Nothing;

		/// <summary>
		/// コンストラクタの中身
		/// </summary>
		private void _MMDMotionFromVME( Stream fs, bool ignoreParent )
		{
			this.ignoreParent = ignoreParent;

			// VME用モーション構造体の取得
			vocaloidMotionEvolved = ProtoBuf.Serializer.Deserialize<VocaloidMotionEvolved>( fs );
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="filePath">VMEファイル名</param>
		/// <param name="ignoreParent">"全ての親"を無視するか否か</param>
		public MMDモーションforVME( string filePath, bool ignoreParent )
		{
			using( var fs = new FileStream( filePath, FileMode.Open ) )
			{
				_MMDMotionFromVME( fs, ignoreParent );
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="fs">VMEファイルのファイルストリーム</param>
		/// <param name="ignoreParent">"全ての親"を無視するか否か</param>
		public MMDモーションforVME( Stream fs, bool ignoreParent )
		{
			_MMDMotionFromVME( fs, ignoreParent );
		}

		/// <summary>
		/// IMotionProviderメンバーの実装
		/// </summary>
		public float 現在のフレーム { get; set; }
		public int このモーションにおける最終フレーム { get; private set; }
		public event EventHandler<EventArgs> FrameTicked;
		public event EventHandler<モーション再生終了後の挙動> モーションが終了した;

		/// <summary>
		/// IMotionProviderメンバーの実装
		/// </summary>
		public void モーションをアタッチする( PMXボーン[] bones )
		{
			this.bones = bones;

			// ボーンのモーションのセット
			var boneIDDictionary = new Dictionary<ulong, string>();
			foreach( var idTag in vocaloidMotionEvolved.boneIDTable ) boneIDDictionary[ idTag.id ] = idTag.name;
			foreach( var boneFrameTable in vocaloidMotionEvolved.boneFrameTables )
			{
				var boneName = boneIDDictionary[ boneFrameTable.id ];
				if( ( ignoreParent && boneName.Equals( "全ての親" ) ) || !bones.Any( b => b.ボーン名.Equals( boneName ) ) ) continue;
				boneMotions.Add( new ボーンモーションforVME( bones.Single( b => b.ボーン名.Equals( boneName ) ), boneFrameTable.frames ) );
			}

			// モーフのモーションのセット
			var morphIDDictionary = new Dictionary<ulong, string>();
			foreach( var idTag in vocaloidMotionEvolved.morphIDTable ) morphIDDictionary[ idTag.id ] = idTag.name;
			foreach( var morphFrameTable in vocaloidMotionEvolved.morphFrameTables )
			{
				var morphName = morphIDDictionary[ morphFrameTable.id ];
				morphMotions.Add( new モーフモーションforVME( morphName, morphFrameTable.frames ) );
			}

			// FinalFrameの検出
			foreach( var boneMotion in boneMotions )
			{
				このモーションにおける最終フレーム = Math.Max( (int) boneMotion.GetFinalFrame(), このモーションにおける最終フレーム );
			}

			// ロード完了
			アタッチ済み = true;
		}

		/// <summary>
		/// IMotionProviderメンバーの実装
		/// </summary>
		public void モーションを1フレーム進める( int fps, float elapsedTime, モーフ管理 morphManager )
		{
			// 行列の更新
			foreach( var boneMotion in boneMotions ) boneMotion.ReviseBone( (ulong) 現在のフレーム );
			foreach( var morphMotion in morphMotions ) morphManager.進捗率を設定する( morphMotion.指定したフレームにおけるモーフ値を取得する( (ulong) 現在のフレーム ), morphMotion.モーフ名 );

			// 停止中はフレームを進めない
			if( !isPlaying ) return;

			// フレームを進める
			現在のフレーム += elapsedTime * fps;
			FrameTicked?.Invoke( this, new EventArgs() );

			// 最終フレームに達した時の処理
			if( 現在のフレーム >= このモーションにおける最終フレーム )
			{
				現在のフレーム = ( actionAfterMotion == モーション再生終了後の挙動.Replay ) ? 1.0e-3f : このモーションにおける最終フレーム;
				モーションが終了した?.Invoke( this, actionAfterMotion );
			}
		}

		/// <summary>
		/// IMotionProviderメンバーの実装
		/// </summary>
		public void モーションを再生する( float frame, モーション再生終了後の挙動 actionAfterMotion )
		{
			if( frame > このモーションにおける最終フレーム )
			{
				throw new InvalidOperationException( "最終フレームを超えた場所から再生を求められました。" );
			}
			現在のフレーム = frame;
			this.actionAfterMotion = actionAfterMotion;
			isPlaying = true;
		}

		/// <summary>
		/// IMotionProviderメンバーの実装
		/// </summary>
		public void モーションを停止する()
		{
			isPlaying = false;
		}

        /// <summary>
        /// IMotionProviderメンバーの実装
        /// </summary>
        public bool アタッチ済み { get; private set; } = false;
    }
}
