using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SharpDX.Direct3D11;

namespace MikuMikuFlex3.Utility.ShaderScript
{
    /// <summary>
    ///     スクリプトに渡される globals インスタンス。
    /// </summary>
    public class ScriptGlobals
    {
        public void SetVertexShader( string csoFilePath )
        {

        }

        /// <summary>
        ///     現在のパイプラインステートに従って描画を行う。
        /// </summary>
        public void Draw()
        {
            throw new NotImplementedException();
        }


        protected VertexShader VertexShader;
    }
}
