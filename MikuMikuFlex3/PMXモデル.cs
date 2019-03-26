using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SharpDX;

namespace MikuMikuFlex3
{
    public class PMXモデル : IDisposable
    {

        // 生成と終了


        public PMXモデル( SharpDX.Direct3D11.Device d3dDevice, string pmxファイルパス )
        {
            this.読み込む( pmxファイルパス );

            var assembly = Assembly.GetExecutingAssembly();
            using( var st = assembly.GetManifestResourceStream( "MikuMikuFlex3.Resources.Shaders.DefaultShader.cso" ) )
            {
                var effectByteCode = new byte[ st.Length ];
                st.Read( effectByteCode, 0, (int) st.Length );

                this._Effect = new SharpDX.Direct3D11.Effect( d3dDevice, effectByteCode );
            }
        }

        public virtual void Dispose()
        {
            this._Effect?.Dispose();
            this._PMXFモデル = null;
        }

        public void 読み込む( string pmxファイルパス )
        {
            using( var fs = new FileStream( pmxファイルパス, FileMode.Open, FileAccess.Read, FileShare.Read ) )
                this._PMXFモデル = new PMXFormat.モデル( fs );
        }



        // 進行と描画


        public void 進行する()
        {
        }

        public void 描画する( SharpDX.Direct3D11.DeviceContext d3ddc )
        {
            this._Effect.GetVariableBySemantic( "EDGECOLOR" ).AsVector().Set( new Vector4( 0f, 0f, 0f, 1f ) );
        }



        // private


        private PMXFormat.モデル _PMXFモデル;

        private SharpDX.Direct3D11.Effect   _Effect;
    }
}
