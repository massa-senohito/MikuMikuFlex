using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MikuMikuFlex3.Utility
{
    /// <summary>
    ///		パフォーマンスカウンタを使用した高精度タイマ。
    /// </summary>
    /// <remarks>
    ///		以下の2種類の使い方を想定する。
    ///		(A) 正確に同一の時刻を複数の処理で共有できるように、現在時刻をキャプチャしてから取得する方法。
    ///		   1. 最初に「CaptureTheCurrentCount()」を呼び出し、その時点での時刻を内部に保存する。
    ///		   2. キャプチャされたカウントを、「現在のキャプチャカウントを……取得する()」を呼び出して、希望する単位で取得する。
    ///		     （次に1.を行うまで、2.はずっと同じ時刻を返し続ける。）
    ///		(B) 常に現時刻（メソッド呼び出し時点の時刻）を取得する方法。
    ///		   a. 「現在のリアルタイムカウントを……取得する()」を呼び出して、希望する単位で取得する。
    ///		   または、
    ///		   b. 「生カウントを取得する()」を呼び出して、生カウンタを取得する。
    ///	
    ///		時刻の単位としては、[カウント], [Seconds], [100ナノ秒] を用意する。
    /// 
    ///		用語：
    ///		"カウント" …………………… タイマインスタンスの生成時（または前回のリセット時）から「前回キャプチャされた時点」までの、パフォーマンスカウンタの差分（相対値）。
    ///		"リアルタイムカウント" …… タイマインスタンスの生成時（または前回のリセット時）から「現時点」までの、パフォーマンスカウンタの差分（相対値）。
    ///		"RawCount" ………………… パフォーマンスカウンタの生の値。::QueryPerformanceCounter() で取得できる値に等しい。システム依存の絶対値。
    /// </remarks>
    public class QPCTimer
    {
        /// <summary>
        ///		カウントが無効であることを示す定数。
        /// </summary>
        public const long Unused = -1;


        public static long Frequency
            => Stopwatch.Frequency;

        public static long RawCount
            => Stopwatch.GetTimestamp();


        public static double RawCountRelativeValueConvertedToSecondsAndReturned( long RawCountRelativeValue )
            => (double) RawCountRelativeValue / QPCTimer.Frequency;

        public static long ConvertSecondsToCountAndReturn( double Seconds )
            => (long) ( Seconds * QPCTimer.Frequency );


        public long CurrentCaptureCount
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    if( 0 != this._NumberOfPauses )
                    {
                        // 停止中。
                        return ( this._CaptureCountAtTheTimeOfPauseDuringOperation - this._RawCountAtTheTimeOfTheLastReset );
                    }
                    else
                    {
                        // 稼働中。
                        return ( this._LastCapturedCount - this._RawCountAtTheTimeOfTheLastReset );
                    }
                }
            }
        }

        public long CurrentCaptureCount100ns
            => (long) ( this.CurrentCaptureCountsec * 10_000_000.0 + 0.5 ); // +0.5 で四捨五入できる。

        public double CurrentCaptureCountsec
            => (double) this.CurrentCaptureCount / QPCTimer.Frequency;

        public long CurrentRealTimeCount
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    if( 0 != this._NumberOfPauses )
                    {
                        // 停止中。
                        return ( this._CaptureCountAtTheTimeOfPauseDuringOperation - this._RawCountAtTheTimeOfTheLastReset );
                    }
                    else
                    {
                        // 稼働中。
                        return ( QPCTimer.RawCount - this._RawCountAtTheTimeOfTheLastReset );
                    }
                }
            }
        }

        public long CurrentRealTimeCount100ns
            => (long) ( this.CurrentRealTimeCountsec * 10_000_000.0 + 0.5 ); // +0.5 で四捨五入できる。

        public double CurrentRealTimeCountsec
            => (double) this.CurrentRealTimeCount / QPCTimer.Frequency;

        public long RawCountAtTheTimeOfTheLastReset
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    return this._RawCountAtTheTimeOfTheLastReset;
                }
            }
        }

        public bool IsStopped
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    return ( 0 != this._NumberOfPauses );
                }
            }
        }

        public bool InOperation
            => !( this.IsStopped );


        public QPCTimer()
        {
            var CurrentRawCount = QPCTimer.RawCount;

            this._RawCountAtTheTimeOfTheLastReset = CurrentRawCount;
            this._LastCapturedCount = CurrentRawCount;
            this._CaptureCountAtTheTimeOfPauseDuringOperation = CurrentRawCount;

            this._NumberOfPauses = 0;
        }

        public long CaptureTheCurrentCount()
        {
            lock( this._ThreadToThreadSynchronization )
            {
                this._LastCapturedCount = QPCTimer.RawCount;
                return this._LastCapturedCount;
            }
        }

        public void Reset( long NewCount = 0 )
        {
            lock( this._ThreadToThreadSynchronization )
            {
                this._RawCountAtTheTimeOfTheLastReset = QPCTimer.RawCount - NewCount;
            }
        }

        public void Pause()
        {
            lock( this._ThreadToThreadSynchronization )
            {
                if( this.InOperation )
                {
                    this._CaptureCountAtTheTimeOfPauseDuringOperation = QPCTimer.RawCount;
                }

                this._NumberOfPauses++;
            }
        }

        public void Resume()
        {
            lock( this._ThreadToThreadSynchronization )
            {
                if( this.IsStopped )
                {
                    this._NumberOfPauses--;

                    if( 0 >= this._NumberOfPauses )
                    {
                        this._LastCapturedCount = QPCTimer.RawCount;
                        this._RawCountAtTheTimeOfTheLastReset += this._LastCapturedCount - this._CaptureCountAtTheTimeOfPauseDuringOperation;
                    }
                }
            }
        }


        private long _RawCountAtTheTimeOfTheLastReset = 0;
        private long _LastCapturedCount = 0;
        private long _CaptureCountAtTheTimeOfPauseDuringOperation = 0;
        private int _NumberOfPauses = 0;
        private readonly object _ThreadToThreadSynchronization = new object();
    }
}
