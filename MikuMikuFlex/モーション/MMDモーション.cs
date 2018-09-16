using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MikuMikuFlex.モーフ;

namespace MikuMikuFlex
{
	/// <summary>
	/// モーションを管理するクラス
	/// </summary>
	public class MMDモーション : モーション
	{
        public event EventHandler<EventArgs> FrameTicked;

        public event EventHandler<モーション再生終了後の挙動> モーションが終了した;

        public float 現在のフレーム { get; set; }

        public int このモーションにおける最終フレーム { get; private set; }

        public bool アタッチ済み { get; private set; }


        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">VMEファイル名</param>
        /// <param name="ignoreParent">"全ての親"を無視するか否か</param>        
        public MMDモーション( string filePath, bool ignoreParent )
		{
			using( var fs = new FileStream( filePath, FileMode.Open ) )
			{
				_初期化する( fs, ignoreParent );
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="fs">VMEファイルのファイルストリーム</param>
		/// <param name="ignoreParent">"全ての親"を無視するか否か</param>
		public MMDモーション( Stream fs, bool ignoreParent )
		{
			_初期化する( fs, ignoreParent );
		}

		public void モーションをアタッチする( PMXボーン[] bones )
		{
			this._ボーン配列 = bones;

			// データのアタッチ
			_ボーンモーションにボーンフレームデータをアタッチする();
			_モーフモーションにモーフフレームデータをアタッチする();

			//フレームのソートと最終フレームの検出
			foreach( var boneMotion in _ボーンモーションのリスト )
			{
				boneMotion.ボーンフレームリストをソートする();
				このモーションにおける最終フレーム = Math.Max( (int) boneMotion.ボーンフレームリストの最後のフレーム番号, このモーションにおける最終フレーム );
			}
			foreach( var morphMotion in _モーフモーションのリスト )
			{
				morphMotion.モーフフレームデータリストをソートする();
			}

			アタッチ済み = true;
		}

		public void モーションを1フレーム進める( int fps, float elapsedTime, モーフ管理 morphManager )
		{
			// 行列の更新
			foreach( var boneMotion in _ボーンモーションのリスト )
                boneMotion.ボーンを指定したフレームの姿勢に更新する( 現在のフレーム );

			foreach( var morphMotion in _モーフモーションのリスト )
                morphManager.進捗率を設定する( morphMotion.指定したフレームにおけるモーフ値を取得する( (ulong) 現在のフレーム ), morphMotion.モーフ名 );

			if( !_再生中である )
                return;

			現在のフレーム += (float) elapsedTime * fps;

            if( 現在のフレーム >= このモーションにおける最終フレーム )
                現在のフレーム = このモーションにおける最終フレーム;

            FrameTicked?.Invoke( this, new EventArgs() );

			if( 現在のフレーム >= このモーションにおける最終フレーム )
			{
				モーションが終了した?.Invoke( this, _モーション再生終了後の挙動 );

                if( _モーション再生終了後の挙動 == モーション再生終了後の挙動.Replay )
                    現在のフレーム = 1.0e-3f;
			}
		}

		public void モーションを再生する( float frame, モーション再生終了後の挙動 action )
		{
			if( frame > このモーションにおける最終フレーム ) throw new InvalidOperationException( "最終フレームを超えた場所から再生を求められました。" );
			現在のフレーム = frame;
			_再生中である = true;
			_モーション再生終了後の挙動 = action;
		}

        public void モーションを停止する()
        {
            _再生中である = false;
        }


        private PMXボーン[] _ボーン配列;

        private readonly List<ボーンモーション> _ボーンモーションのリスト = new List<ボーンモーション>();

        private readonly List<モーフモーション> _モーフモーションのリスト = new List<モーフモーション>();

        private MMDFileParser.MotionParser.モーション _モーション;

        private モーション再生終了後の挙動 _モーション再生終了後の挙動 = モーション再生終了後の挙動.Nothing;

        private bool _再生中である;

        private bool _すべての親を無視する;


        private void _初期化する( Stream fs, bool ignoreParent )
        {
            this._すべての親を無視する = ignoreParent;

            this._モーション = MMDFileParser.MotionParser.モーション.読み込む( fs );
        }

        private void _ボーンモーションにボーンフレームデータをアタッチする()
        {
            foreach( var boneFrameData in _モーション.ボーンフレームリスト )
            {
                if( _すべての親を無視する && boneFrameData.ボーン名.Equals( "全ての親" ) )
                    continue;

                if( !_ボーン配列.Any( b => b.ボーン名.Equals( boneFrameData.ボーン名 ) ) )
                    continue;

                var bone = _ボーン配列.Single( b => b.ボーン名.Equals( boneFrameData.ボーン名 ) );

                if( !_ボーンモーションのリスト.Any( bm => bm.ボーン名.Equals( boneFrameData.ボーン名 ) ) )
                {
                    var boneMotion = new ボーンモーション( bone );
                    boneMotion.ボーンフレームを追加する( boneFrameData );
                    _ボーンモーションのリスト.Add( boneMotion );
                    continue;
                }
                _ボーンモーションのリスト.Single( bm => bm.ボーン名.Equals( boneFrameData.ボーン名 ) ).ボーンフレームを追加する( boneFrameData );
            }
        }

        private void _モーフモーションにモーフフレームデータをアタッチする()
        {
            foreach( var morphFrameData in _モーション.モーフフレームリスト )
            {
                if( !_モーフモーションのリスト.Any( mm => mm.モーフ名.Equals( morphFrameData.モーフ名 ) ) )
                {
                    var morphMotion = new モーフモーション( morphFrameData.モーフ名 );
                    morphMotion.モーフフレームデータを付け加える( morphFrameData );
                    _モーフモーションのリスト.Add( morphMotion );
                }
                _モーフモーションのリスト.Single( mm => mm.モーフ名.Equals( morphFrameData.モーフ名 ) ).モーフフレームデータを付け加える( morphFrameData );
            }
        }
    }
}