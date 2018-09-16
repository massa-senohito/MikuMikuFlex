using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MMDFileParser.PMXModelParser;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex
{
	/// <summary>
	///     モーション管理クラス
	/// </summary>
	public class MMDモーション管理 : モーション管理
	{
        public モーション 現在再生中のモーション { get; set; }

        public float 現在再生中のモーションのフレーム位置sec
            => ( 現在再生中のモーション == null ) ? float.NaN : ( 現在再生中のモーション.現在のフレーム / RenderContext.Instance.Timer.秒間フレーム数 );

        public float 前回のフレームからの経過時間sec { get; private set; }

        /// <summary>
        ///     [Key: ファイル名, Value: モーション] のリスト。
        /// </summary>
        public List<KeyValuePair<string, モーション>> モーションリスト { get; private set; }


        public event EventHandler<モーション再生終了後の挙動> モーションが再生終了した;

        public event EventHandler モーションリストが更新された;


        public MMDモーション管理()
        {
        }

        public void 初期化する( PMXモデル model, モーフ管理 morph, スキニング skinning, バッファ管理 bufferManager )
		{
			_スキニング = skinning;

            _Stopwatch = new Stopwatch();
			_Stopwatch.Start();

            _モーフ管理 = morph;

            モーションリスト = new List<KeyValuePair<string, モーション>>();
		}

		/// <summary>
		///     モーションをファイルから生成して返す。
        ///     生成されたモーションは、このインスタンスの <see cref="モーションリスト"/> に追加される。
		/// </summary>
		public モーション ファイルからモーションを生成し追加する( string ファイルパス, bool すべての親を無視する )
		{
			モーション motion;

            
            // 拡張子に基づいてモーションを生成。

            var 拡張子 = Path.GetExtension( ファイルパス );

            if( String.Compare( 拡張子, ".vmd", true ) == 0 )
            {
                // VMD ファイルから
                motion = new MMDモーション( ファイルパス, すべての親を無視する );
            }
            else if( String.Compare( 拡張子, ".vme", true ) == 0 )
            {
                // VME ファイルから
                motion = new MMDモーションforVME( ファイルパス, すべての親を無視する );
            }
            else
            {
                throw new Exception( "ファイルが不適切です！" );
            }

            // モーションをスキニングに割り当てる
			motion.モーションをアタッチする( _スキニング.ボーン配列 );

            // 終了イベントを登録
            motion.モーションが終了した += motion_MotionFinished;

            // モーションリストに追加。
			モーションリスト.Add( new KeyValuePair<string, モーション>( ファイルパス, motion ) );
			モーションリストが更新された?.Invoke( this, new EventArgs() );

			return motion;
		}

        /// <param name="fileName">ストリームを表すファイル名。マップへの登録用。</param>
        /// <param name="stream">ストリーム。</param>
		/// <param name="ignoreParent">すべての親を無視するか否か</param>
        /// <returns>モーションプロバイダ</returns>
        public モーション ストリームからモーションを追加する( string fileName, Stream stream, bool ignoreParent )
        {
            // MMDモーションを生成
            モーション motion = new MMDモーション( stream, ignoreParent );

            // 終了イベントを登録
            motion.モーションが終了した += motion_MotionFinished;

            // モーションリストに追加。
            モーションリスト.Add( new KeyValuePair<string, モーション>( fileName, motion ) );
            モーションリストが更新された?.Invoke( this, new EventArgs() );

            return motion;
        }

        /// <summary>
        ///     指定したモーションを再生する
        /// </summary>
        /// <param name="id">モーションのid</param>
        /// <param name="startFrame">最初のフレーム</param>
        /// <param name="setting">終了後の挙動</param>
        public void モーションを適用する( モーション motionProvider, int startFrame = 0, モーション再生終了後の挙動 setting = モーション再生終了後の挙動.Nothing )
		{
            // TODO: モーションが同時には１つしか使えない
			if( 現在再生中のモーション != null )
                現在再生中のモーション.モーションを停止する();

			motionProvider.モーションを再生する( startFrame, setting );

			現在再生中のモーション = motionProvider;
		}

		public void モーションを停止する( bool クリアする = false )
		{
            現在再生中のモーション?.モーションを停止する();

			if( クリアする )
				現在再生中のモーション = null;
		}

        public bool 変形を更新する()
        {
            if( _前回の時刻 == 0 )
            {
                _前回の時刻 = _Stopwatch.ElapsedMilliseconds;
            }
            else
            {
                long 現在時刻 = _Stopwatch.ElapsedMilliseconds;

                前回のフレームからの経過時間sec = 現在時刻 - _前回の時刻;

                現在再生中のモーション?.モーションを1フレーム進める( RenderContext.Instance.Timer.秒間フレーム数, 前回のフレームからの経過時間sec / 1000f, _モーフ管理 );

                _前回の時刻 = 現在時刻;
            }
            return true;
        }


        private Stopwatch _Stopwatch;

        private long _前回の時刻 { get; set; }

        private スキニング _スキニング;

        private モーフ管理 _モーフ管理;


        private void motion_MotionFinished( object owner, モーション再生終了後の挙動 obj )
		{
			モーションが再生終了した?.Invoke( this, obj );
		}
	}
}
