using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.Utility
{
    public class FPS
    {
        public int 現在のFPS
        {
            get
            {
                lock( this._スレッド間同期 )
                {
                    return this._現在のFPS;
                }
            }
        }

        public int 現在のVPS
        {
            get
            {
                lock( this._スレッド間同期 )
                {
                    return this._現在のVPS;
                }
            }
        }


        public FPS()
        {
            this._現在のFPS = 0;
            this._現在のVPS = 0;
            this._Stopwatch = Stopwatch.StartNew();
        }

        public bool FPSをカウントする()
        {
            lock( this._スレッド間同期 )
            {
                this._fps用カウンタ++;

                return this._更新チェックする();
            }
        }

        public bool VPSをカウントする()
        {
            lock( this._スレッド間同期 )
            {
                this._vps用カウンタ++;

                return this._更新チェックする();
            }
        }


        private int _現在のFPS = 0;

        private int _現在のVPS = 0;

        private int _fps用カウンタ = 0;

        private int _vps用カウンタ = 0;

        private Stopwatch _Stopwatch;
        
        private readonly object _スレッド間同期 = new object();


        private bool _更新チェックする()
        {
            if( 1000 <= this._Stopwatch.ElapsedMilliseconds )
            {
                this._Stopwatch.Restart();

                this._現在のFPS = this._fps用カウンタ;
                this._現在のVPS = this._vps用カウンタ;
                this._fps用カウンタ = 0;
                this._vps用カウンタ = 0;

                return true;
            }

            return false;
        }
    }
}
