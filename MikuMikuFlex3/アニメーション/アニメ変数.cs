using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public class アニメ変数<T>
    {
        public T 値 { get; protected set; }



        // 生成と終了


        public アニメ変数( T 初期値 )
        {
            this.値 = 初期値;
        }



        // リストを構築


        public void 遷移を追加する( アニメ遷移<T> 遷移 )
        {
            this._遷移リスト.Enqueue( 遷移 );
        }

        public void 遷移をクリアする()
        {
            this._遷移リスト = new ConcurrentQueue<アニメ遷移<T>>();
        }



        // リストを再生


        public T 更新する( double 現在時刻sec )
        {
            bool 現在値を取得完了 = false;

            while( !現在値を取得完了 )
            {
                if( this._遷移リスト.TryPeek( out var 遷移 ) )
                {
                    // (A) リストに遷移がある

                    if( 遷移.確定されていない )
                    {
                        遷移.確定する( 現在時刻sec, this.値 );
                    }

                    if( 遷移.更新する( 現在時刻sec, out T 現在の値 ) )
                    {
                        this.値 = 現在の値;

                        現在値を取得完了 = true;
                    }
                    else
                    {
                        // この遷移は終了した → 次の遷移へ

                        this._遷移リスト.TryDequeue( out _ );
                    }
                }
                else
                {
                    // (B) リストが空 → 現状維持

                    現在値を取得完了 = true;
                }
            }

            return this.値;
        }



        // private


        private ConcurrentQueue<アニメ遷移<T>> _遷移リスト = new ConcurrentQueue<アニメ遷移<T>>();
    }
}
