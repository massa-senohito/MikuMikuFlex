using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex
{
    public class シーン用エフェクト管理 : エフェクト管理, IDisposable
    {
        public IReadOnlyList<エフェクト> シーン用エフェクトリスト
            => _シーン用エフェクトリスト;


        public シーン用エフェクト管理()
            :base()
        {
            this._シーン用エフェクトリスト = new List<エフェクト>();
        }

        public new void Dispose()
        {
            this._シーン用エフェクトリスト.Clear();

            base.Dispose();
        }

        public void シーン用エフェクトを割り当てる( エフェクト effect )
        {
            if( !( effect.ScriptClass.HasFlag( ScriptClass.Scene ) ) )
                throw new InvalidOperationException( "Sceneフラグを持たないエフェクトはシーン用として登録できません。" );

            // すでにリストに登録してあれば削除する。
            if( _シーン用エフェクトリスト.Contains( effect ) )
                _シーン用エフェクトリスト.Remove( effect );

            // リストに登録する。
            _シーン用エフェクトリスト.Add( effect );
        }


        private List<エフェクト> _シーン用エフェクトリスト;
    }
}
