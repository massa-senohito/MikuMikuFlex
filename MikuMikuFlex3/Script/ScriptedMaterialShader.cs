using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace MikuMikuFlex3.Script
{
    /// <summary>
    ///     スクリプトを使った材質シェーダー。
    /// </summary>
    public class ScriptedMaterialShader : IMaterialShader
    {
        public ScriptedMaterialShader( string scriptPath, Device d3dDevice )
        {
            var scriptFolder = Path.GetDirectoryName( scriptPath );

            string code;
            using( var fs = new FileStream( scriptPath, FileMode.Open, FileAccess.Read, FileShare.Read ) )
            using( var sr = new StreamReader( fs ) )
            {
                code = sr.ReadToEnd() + @"
switch( Reason )
{
    case MikuMikuFlex3.Script.Reason.Initialize: Initialize(); break;
    case MikuMikuFlex3.Script.Reason.Run: Run(); break;
}
";
            }

            var scriptSourceResolver = string.IsNullOrEmpty( scriptFolder ) ? ScriptSourceResolver.Default : ScriptSourceResolver.Default.WithBaseDirectory( scriptFolder );
            var scriptOptions = ScriptOptions.Default
                .WithSourceResolver( scriptSourceResolver )
                .WithImports( "System", "MikuMikuFlex3", "MikuMikuFlex3.Script" )
                .WithReferences( System.Reflection.Assembly.GetExecutingAssembly() );

            this._ShaderScript = CSharpScript.Create( code, scriptOptions, typeof( PipelineState ) );
            var imms = this._ShaderScript.Compile();

            if( 0 != imms.Length )
                throw new Exception( $"スクリプトのコンパイルに失敗しました。- {imms[ 0 ].ToString()}" );

            this._PipelineState = new PipelineState( d3dDevice );

            this._PipelineState.Reason = Reason.Initialize;
            this._ShaderScript.RunAsync( this._PipelineState );
        }

        public void Dispose()
        {
            this._PipelineState?.Dispose();
            this._ShaderScript = null;
        }

        public void Draw( int 頂点数, int 頂点の開始インデックス, MMDPass pass種別, DeviceContext d3ddc )
        {
            this._PipelineState.SetDrawState( 頂点数, 頂点の開始インデックス, pass種別, d3ddc );
            this._PipelineState.Reason = Reason.Run;
            this._ShaderScript.RunAsync( this._PipelineState );
        }


        protected Microsoft.CodeAnalysis.Scripting.Script _ShaderScript;

        protected PipelineState _PipelineState;
    }
}
