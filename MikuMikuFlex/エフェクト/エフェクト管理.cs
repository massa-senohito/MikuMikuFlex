using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex
{
    public abstract class エフェクト管理 : IDisposable
    {
        public エフェクト 既定のエフェクト { get; protected set; }

        /// <summary>
        ///     管理するすべてのエフェクトのリスト。
        ///     [key: エフェクトID（一意の文字列）, value: エフェクト]
        /// </summary>
        public IReadOnlyDictionary<string, エフェクト> エフェクトマスタリスト
            => this._エフェクトマスタリスト;


        public エフェクト管理()
        {
            既定のエフェクト = null;
            _エフェクトマスタリスト = new Dictionary<string, エフェクト>();
        }

        public void Dispose()
        {
            foreach( KeyValuePair<string, エフェクト> kvp in _エフェクトマスタリスト )
                kvp.Value.Dispose();

            _エフェクトマスタリスト.Clear();

            既定のエフェクト = null;
        }

        public void エフェクトをマスタリストに登録する( string エフェクトID, エフェクト effect, bool これを既定のエフェクトに指定する = false )
        {
            _エフェクトマスタリスト.Add( エフェクトID, effect );

            Debug.WriteLine( $"エフェクトを追加しました。[{effect.ファイル名}]" );

            if( これを既定のエフェクトに指定する )
                既定のエフェクト = effect;
        }


        private Dictionary<string, エフェクト> _エフェクトマスタリスト;
    }
}
