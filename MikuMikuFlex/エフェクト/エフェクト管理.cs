using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMF.エフェクト
{
    public class エフェクト管理 : IDisposable
    {
        public List<エフェクト> エフェクトリスト { get; protected set; }

        public エフェクト 既定のエフェクト { get; protected set; }


        public エフェクト管理()
        {
            エフェクトリスト = new List<エフェクト>();
            既定のエフェクト = null;
        }

        public void Dispose()
        {
            foreach( var effect in エフェクトリスト )
                effect.Dispose();

            エフェクトリスト.Clear();

            既定のエフェクト = null;
        }

        public void エフェクトを登録する( エフェクト effect, bool 既定にする = false )
        {
            エフェクトリスト.Add( effect );

            Debug.WriteLine( $"エフェクトを追加しました。[{effect.ファイル名}]" );

            if( 既定にする )
                既定のエフェクト = effect;
        }
    }
}
