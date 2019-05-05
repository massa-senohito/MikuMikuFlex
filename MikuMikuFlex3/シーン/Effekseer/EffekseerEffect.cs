using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex3
{
    public class EffekseerEffect : IDisposable
    {

        public Effekseer Effekseer { get; protected set; }

        /// <summary>
        ///     Manager の Update 時に呼び出されるアクション。null 可。
        /// </summary>
        public Action<float> UpdateAction { get; protected set; }



        // 生成と終了


        public EffekseerEffect( Effekseer effekseer, string efkPath, float magnification = 10.0f, string materialPath = null )
        {
            this.Effekseer = effekseer;
            this._Effect = EffekseerRendererDX11NET.Effect.Create( this.Effekseer.Manager, efkPath, magnification, materialPath );

            effekseer.EffectList.Add( this );
        }

        public void Dispose()
        {
            if( this.Effekseer.EffectList.Contains( this ) )
                this.Effekseer.EffectList.Remove( this );

            this._Effect.Dispose();

            this.Effekseer = null; // Disposeしない
        }



        // 再生と停止


        /// <summary>
        ///     エフェクトの再生を開始する。
        /// </summary>
        /// <param name="x">初期位置のX。</param>
        /// <param name="y">初期位置のY。</param>
        /// <param name="z">初期位置のZ。</param>
        public void Play( float x, float y, float z, Action<float> update )
        {
            this._EffectHandle = this.Effekseer.Manager.Play( this._Effect, x, y, z );
            this.UpdateAction = update;
        }

        /// <summary>
        ///     エフェクトの再生を停止する。
        /// </summary>
        public void Stop()
        {
            this.Effekseer.Manager.StopEffect( this._EffectHandle );
        }



        // ローカル


        private EffekseerRendererDX11NET.Effect _Effect;

        private int _EffectHandle;
    }
}
