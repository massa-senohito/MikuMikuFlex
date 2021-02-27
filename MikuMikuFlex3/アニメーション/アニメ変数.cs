    using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public class AnimeVariables<T>
    {
        public T Value { get; set; }



        // 生成と終了


        public AnimeVariables( T InitialValue )
        {
            this.Value = InitialValue;
        }



        // リストを構築


        public void AddATransition( AnimeTransition<T> Transition )
        {
            this._TransitionList.Enqueue( Transition );
        }

        public void ClearTheTransition()
        {
            this._TransitionList = new ConcurrentQueue<AnimeTransition<T>>();
        }



        // リストを再生


        internal T Update( double CurrentTimesec )
        {
            bool CompletedAcquisitionOfCurrentValue = false;

            double StartTimeOfTheNextConsecutiveTransitionsec = CurrentTimesec;

            while( !CompletedAcquisitionOfCurrentValue )
            {
                if( this._TransitionList.TryPeek( out var Transition ) )
                {
                    // (A) リストに遷移がある

                    if( Transition.NotConfirmed )
                    {
                        Transition.Determine( StartTimeOfTheNextConsecutiveTransitionsec, this.Value );
                    }

                    if( Transition.Update( CurrentTimesec, out T CurrentValue ) )
                    {
                        this.Value = CurrentValue;

                        CompletedAcquisitionOfCurrentValue = true;
                    }
                    else
                    {
                        // 遷移リストに次の遷移が入っているなら、その遷移はこのあとすぐ開始されるが、現在時刻ではなく、この時刻から開始しなければならない。（ずれを抑えるため）
                        // なお、遷移リストに次の遷移が入ってないなら、このメソッドはいったん抜けるので、次に入ってくる遷移の開始時刻は現在時刻にリセットされる。

                        StartTimeOfTheNextConsecutiveTransitionsec = Transition.StartTimesec + Transition.Durationsec;


                        // この遷移は終了した → 次の遷移へ

                        this._TransitionList.TryDequeue( out _ );
                    }
                }
                else
                {
                    // (B) リストが空 → 現状維持

                    CompletedAcquisitionOfCurrentValue = true;
                }
            }

            return this.Value;
        }



        // private


        private ConcurrentQueue<AnimeTransition<T>> _TransitionList = new ConcurrentQueue<AnimeTransition<T>>();
    }
}
