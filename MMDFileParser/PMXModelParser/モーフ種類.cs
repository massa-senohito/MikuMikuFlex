using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMDFileParser.PMXModelParser
{
    public enum モーフ種類
    {
        // 整数演算で範囲選択してる箇所があるので、下記数値は変更しないこと。

        頂点 = 0,
        UV = 1,
        追加UV1 = 2,
        追加UV2 = 3,
        追加UV3 = 4,
        追加UV4 = 5,
        ボーン = 6,
        材質 = 7,
        グループ = 8,
        フリップ = 9,
        インパルス = 10,
    }
}
