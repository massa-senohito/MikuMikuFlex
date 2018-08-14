using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using SharpDX.D3DCompiler;

namespace MMF.エフェクト.Includer
{
    /// <summary>
    ///     基本的な#includeのパス解決クラス
    /// </summary>
    public class BasicEffectIncluder : Include, IComparer<IncludeDirectory>
    {
        public ObservableCollection<IncludeDirectory> 登録されているディレクトリとその優先度のマップ { get; private set; }


        public BasicEffectIncluder()
        {
            登録されているディレクトリとその優先度のマップ = new ObservableCollection<IncludeDirectory>();
            登録されているディレクトリとその優先度のマップ.CollectionChanged += IncludeDirectories_CollectionChanged;
            登録されているディレクトリとその優先度のマップ.Add( new IncludeDirectory( "Shader\\include", 0 ) );
        }

        /// <summary>
        ///     コレクションが変更されたときはソートし直す。
        /// </summary>
        void IncludeDirectories_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            List<IncludeDirectory> sorted = 登録されているディレクトリとその優先度のマップ.ToList();

            登録されているディレクトリとその優先度のマップ = new ObservableCollection<IncludeDirectory>( sorted );
        }


        // Include の実装

        public Stream Open( IncludeType type, string fileName, Stream parentStream )
        {
            if( Path.IsPathRooted( fileName ) )
                return File.OpenRead( fileName );   // 絶対パスならファイルストリームを開いて返す

            foreach( var directory in 登録されているディレクトリとその優先度のマップ )
            {
                if( File.Exists( Path.Combine( directory.ディレクトリのパス, fileName ) ) )
                    return File.OpenRead( Path.Combine( directory.ディレクトリのパス, fileName ) );
            }

            return null;
        }

        public void Close( Stream stream )
        {
            stream.Close();
        }

        public void Dispose()
        {
        }

        public IDisposable Shadow { get; set; }


        // IComparer<IncludeDirectory> の実装

        public int Compare( IncludeDirectory x, IncludeDirectory y )
        {
            return x.優先度 - y.優先度;
        }
    }
}
