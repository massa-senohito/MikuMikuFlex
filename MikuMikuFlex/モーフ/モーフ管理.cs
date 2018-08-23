using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.モーフ
{
    public interface モーフ管理 : モーフ
    {
        float モーフの進捗率を返す( string morphName );
    }
}
