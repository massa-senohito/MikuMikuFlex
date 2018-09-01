using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.エフェクト
{
    public class エフェクト管理 : IDisposable
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
            _サブセットIDtoエフェクトマップ = new Dictionary<int, エフェクト>();
        }

        public void Dispose()
        {
            _サブセットIDtoエフェクトマップ.Clear();

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

        public void サブセットにエフェクトを割り当てる( int サブセットID, エフェクト effect )
        {
            // すでにマップに登録してあれば削除する。
            if( _サブセットIDtoエフェクトマップ.ContainsKey( サブセットID ) )
                _サブセットIDtoエフェクトマップ.Remove( サブセットID );

            // マップに登録する。
            _サブセットIDtoエフェクトマップ.Add( サブセットID, effect );
        }

        public エフェクト サブセットのエフェクトを取得する( int サブセットID )
        {
            return ( this._サブセットIDtoエフェクトマップ.TryGetValue( サブセットID, out var effect ) ) ? effect : this.既定のエフェクト;
        }


        private Dictionary<string, エフェクト> _エフェクトマスタリスト;

        /// <summary>
        ///     サブセットに適用されるエフェクトの対応表。
        ///     このマップにサブセットIDが存在しない場合は、既定のエフェクトを使うことを意味する。
        ///     [key: サブセットID, value: エフェクト]
        /// </summary>
        private Dictionary<int, エフェクト> _サブセットIDtoエフェクトマップ;
    }
}
