using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3.Utility
{
    public class FPS
    {
        public int CurrentFPS
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    return this._CurrentFPS;
                }
            }
        }

        public int CurrentVPS
        {
            get
            {
                lock( this._ThreadToThreadSynchronization )
                {
                    return this._CurrentVPS;
                }
            }
        }


        public FPS()
        {
            this._CurrentFPS = 0;
            this._CurrentVPS = 0;
            this._Stopwatch = Stopwatch.StartNew();
        }

        public bool FPSをカウントする()
        {
            lock( this._ThreadToThreadSynchronization )
            {
                this._fpsCounterFor++;

                return this._CheckForUpdates();
            }
        }

        public bool VPSをカウントする()
        {
            lock( this._ThreadToThreadSynchronization )
            {
                this._vpsCounterFor++;

                return this._CheckForUpdates();
            }
        }


        private int _CurrentFPS = 0;

        private int _CurrentVPS = 0;

        private int _fpsCounterFor = 0;

        private int _vpsCounterFor = 0;

        private Stopwatch _Stopwatch;
        
        private readonly object _ThreadToThreadSynchronization = new object();


        private bool _CheckForUpdates()
        {
            if( 1000 <= this._Stopwatch.ElapsedMilliseconds )
            {
                this._Stopwatch.Restart();

                this._CurrentFPS = this._fpsCounterFor;
                this._CurrentVPS = this._vpsCounterFor;
                this._fpsCounterFor = 0;
                this._vpsCounterFor = 0;

                return true;
            }

            return false;
        }
    }
}
