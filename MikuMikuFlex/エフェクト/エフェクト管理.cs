using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.エフェクト
{
    public class エフェクト管理 : IDisposable
    {
        /// <summary>
        ///     [key: エフェクト名, value: エフェクト]
        /// </summary>
        public Dictionary<string, エフェクト> エフェクトマスタリスト { get; protected set; }

        public エフェクト 既定のエフェクト { get; protected set; }

        /// <summary>
        ///     サブセットに適用されるエフェクトの対応表。
        ///     このマップにサブセットIDが存在しない場合は、既定のエフェクトを使うことを意味する。
        ///     [key: サブセットID, value: エフェクト名]
        /// </summary>
        protected Dictionary<int, string> サブセットIDtoエフェクト名マップ;


        public エフェクト管理()
        {
            エフェクトマスタリスト = new Dictionary<string, エフェクト>();
            既定のエフェクト = null;
            サブセットIDtoエフェクト名マップ = new Dictionary<int, string>();
        }

        public void Dispose()
        {
            サブセットIDtoエフェクト名マップ.Clear();

            foreach( KeyValuePair<string, エフェクト> kvp in エフェクトマスタリスト )
                kvp.Value.Dispose();

            エフェクトマスタリスト.Clear();

            既定のエフェクト = null;
        }

        public void エフェクトをマスタリストに登録する( string name, エフェクト effect, bool これを既定のエフェクトに指定する = false )
        {
            エフェクトマスタリスト.Add( name, effect );

            Debug.WriteLine( $"エフェクトを追加しました。[{effect.ファイル名}]" );

            if( これを既定のエフェクトに指定する )
                既定のエフェクト = effect;
        }

        public void サブセットにエフェクトを割り当てる( int サブセットID, string エフェクト名 )
        {
            // すでにマップに登録してあれば削除する。
            if( サブセットIDtoエフェクト名マップ.ContainsKey( サブセットID ) )
                サブセットIDtoエフェクト名マップ.Remove( サブセットID );

            // マップに登録する。
            サブセットIDtoエフェクト名マップ.Add( サブセットID, エフェクト名 );
        }

        public エフェクト 名前からエフェクトを取得する( string エフェクト名 )
        {
            if( string.IsNullOrEmpty( エフェクト名 ) )
                return null;

            return ( エフェクトマスタリスト.ContainsKey( エフェクト名 ) ) ?
                エフェクトマスタリスト[ エフェクト名 ] :
                null;   // なかったら null 。
        }

        public string サブセットに割り当てられているエフェクト名を取得する( int サブセットID )
        {
            return ( サブセットIDtoエフェクト名マップ.ContainsKey( サブセットID ) ) ?
                サブセットIDtoエフェクト名マップ[ サブセットID ] :
                null;   // 存在しないなら null。
        }
    }
}
