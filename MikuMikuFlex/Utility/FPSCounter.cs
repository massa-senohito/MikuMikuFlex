using System.Collections.Generic;
using System.Timers;

namespace MMF.Utility
{
	/// <summary>
	///     FPSカウンタ。
	/// </summary>
	/// <remarks>
	///		「平均する秒数」を変更することで、FPS の計算対象時間を変更することができる（既定値は10秒）。
	/// </remarks>
	public class FPSCounter
	{
		public Timer タイマ { get; private set; }

		public int 平均する秒数 { get; set; }

		public float 現在のFPS
		{
			get
			{
				if( !this._キャッシュ値を使う )
				{
					int sum = 0;
					foreach( int i in _FPSの履歴 )
					{
						sum += i;
					}
					this._現在のキャッシュ値 = sum / (float) this._FPSの履歴.Count;
					this._キャッシュ値を使う = true;
					return this._現在のキャッシュ値;
				}
				return this._現在のキャッシュ値;
			}
		}


        public FPSCounter()
		{
			this._FPSの履歴 = new Queue<int>();
			this.平均する秒数 = 10;
			this.タイマ = new Timer( 1000d );
			this.タイマ.Elapsed += this._秒ごとの処理;
		}

		public void カウントを開始する()
		{
			this._計測中のカウンタ = 0;
			this.タイマ.Start();
		}

		public void フレームを進める()
		{
			this._計測中のカウンタ++;
		}


        private Queue<int> _FPSの履歴 { get; set; }

		private int _計測中のカウンタ { get; set; }

		/// <summary>
		///		これを false にすると、FPS プロパティで FPS の計算を行う。
		///		計算後は自動的に true になる。
		/// </summary>
		private bool _キャッシュ値を使う;

		/// <summary>
		///		現在の FPS 値。
		/// </summary>
		private float _現在のキャッシュ値;


		private void _秒ごとの処理( object sender, ElapsedEventArgs args )
		{
			// 履歴に現在のカウンタを追加する。履歴がいっぱいなら、古いものから削除する。
			if( this._FPSの履歴.Count > this.平均する秒数 )
				this._FPSの履歴.Dequeue();
			this._FPSの履歴.Enqueue( this._計測中のカウンタ );

			// 現在のカウンタをリセットする。
			this._計測中のカウンタ = 0;
			this._キャッシュ値を使う = false;
		}
	}
}
