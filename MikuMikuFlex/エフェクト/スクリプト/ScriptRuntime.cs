using System;
using System.Collections.Generic;
using MikuMikuFlex.エフェクト.Script.Function;
using MikuMikuFlex.モデル;

namespace MikuMikuFlex.エフェクト.Script
{
    /// <remarks>
    ///     string Script の構文: ファンクション[;ファンクション]*;
    ///     ファンクションの構文: ファンクション名[連番]=値
    /// </remarks>
    public class ScriptRuntime
    {
        /// <summary>
        ///     string Script の内容。
        /// </summary>
        public string ScriptCode { get; private set; }

        // static
        internal static Dictionary<string, 関数> 現在対応しているファンクションの一覧;

        internal List<関数> 実行するファンクションのリスト;

        /// <summary>
        ///     <see cref="実行するファンクションのリスト"/> のインデックス。
        /// </summary>
        public int 現在実行中のファンクションのインデックス;

        public Stack<int> LoopBegins = new Stack<int>();

        public Stack<int> LoopCounts = new Stack<int>();

        public Stack<int> LoopEndCount = new Stack<int>();


        static ScriptRuntime()
        {
            現在対応しているファンクションの一覧 = new Dictionary<string, 関数>();
            foreach( var func in new List<関数> {
                new RenderColorTarget関数(),
                new RenderDepthStencilTarget関数(),
                new ClearSetColor関数(),
                new ClearSetDepth関数(),
                new Clear関数(),
                new Pass関数(),
                new Draw関数(),
                new LoopByCount関数(),
                new LoopEnd関数(),
                new LoopGetIndex関数(),
                new ScriptExternal関数(),
            } )
            {
                現在対応しているファンクションの一覧.Add( func.名前, func );
            }
        }

        public ScriptRuntime( string script, エフェクト effect, テクニック technique = null, パス pass = null )
        {
            ScriptCode = script;

            _ScriptCodeを解析する( effect, technique, pass );
        }

        public void 実行する<T>( Action<T> drawAction, T ipmxSubset )
        {
            if( null == ( ipmxSubset as サブセット ) )
                return;

            for(
                現在実行中のファンクションのインデックス = 0; 
                現在実行中のファンクションのインデックス < 実行するファンクションのリスト.Count; 
                実行するファンクションのリスト[ 現在実行中のファンクションのインデックス ].次のファンクションへ遷移する( this ) )
            {
                実行するファンクションのリスト[ 現在実行中のファンクションのインデックス ].実行する( (サブセット) ipmxSubset, drawAction as Action<サブセット> );
            }
        }


        private void _ScriptCodeを解析する( エフェクト effect, テクニック technique = null, パス pass = null )
        {
            実行するファンクションのリスト = new List<関数>();

            if( string.IsNullOrWhiteSpace( ScriptCode ) )
                return;

            string[] splittedFunctions = ScriptCode.Split( ';' );   // ';' で分割し1命令ごとにする

            foreach( string function in splittedFunctions )
            {
                if( string.IsNullOrWhiteSpace( function ) )
                    continue;

                int 連番 = 0; // 省略時は 0 

                string[] segments = function.Split( '=' );  // '=' で分割し、関数名と引数を分ける

                if( segments.Length > 2 )
                    throw new InvalidMMEEffectShader例外( "スクリプト中の'='の数が多すぎます。" );

                char 関数名の末尾の文字 = segments[ 0 ][ segments[ 0 ].Length - 1 ];

                if( char.IsNumber( 関数名の末尾の文字 ) )
                {
                    segments[ 0 ] = segments[ 0 ].Remove( segments[ 0 ].Length - 1 );
                    連番 = int.Parse( 関数名の末尾の文字.ToString() );
                }

                if( 現在対応しているファンクションの一覧.ContainsKey( segments[ 0 ] ) )
                {
                    実行するファンクションのリスト.Add(
                        現在対応しているファンクションの一覧[ segments[ 0 ] ].ファンクションインスタンスを作成する( 連番, segments[ 1 ], this, effect, technique, pass )
                        );
                }
            }
        }
    }
}
