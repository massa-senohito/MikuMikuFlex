using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex
{
    public class オブジェクト用エフェクト管理 : エフェクト管理, IDisposable
    {
        public オブジェクト用エフェクト管理()
            : base()
        {
            _サブセットIDtoエフェクトマップ = new Dictionary<int, エフェクト>();
        }

        public new void Dispose()
        {
            _サブセットIDtoエフェクトマップ.Clear();

            base.Dispose();
        }

        public void サブセットにエフェクトを割り当てる( int サブセットID, エフェクト effect )
        {
            if( !( effect.ScriptClass.HasFlag( ScriptClass.Object ) ) )
                throw new InvalidOperationException( "Objectフラグを持たないエフェクトはサブセットに登録できません。" );

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

        /// <summary>
        ///     サブセットに適用されるエフェクトの対応表。
        ///     このマップにサブセットIDが存在しない場合は、既定のエフェクトを使うことを意味する。
        ///     [key: サブセットID, value: エフェクト]
        /// </summary>
        /// <remarks>
        ///     このマップに登録されているエフェクトは、<see cref="エフェクトマスタリスト"/>にも登録されている。
        /// </remarks>
        private Dictionary<int, エフェクト> _サブセットIDtoエフェクトマップ;
    }
}
