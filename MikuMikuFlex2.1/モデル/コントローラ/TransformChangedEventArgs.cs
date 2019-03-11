using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MikuMikuFlex.モデル.コントローラ
{
    public class TransformChangedEventArgs : EventArgs
    {
        public IDrawable TargetModel { get; }

        public TransformType Type { get; }


        public TransformChangedEventArgs( IDrawable targetModel, TransformType type )
        {
            this.TargetModel = targetModel;
            this.Type = type;
        }
    }

}
