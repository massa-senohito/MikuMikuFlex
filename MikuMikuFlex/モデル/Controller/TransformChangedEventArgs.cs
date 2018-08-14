using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MMF.モデル.Controller
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
