using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MMDFileParser.PMXModelParser.JointParam
{
    public class 錐回転ジョイントパラメータ: ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }

        /// <summary>
        ///     回転制限 - 下限 - Z
        /// </summary>
        public float SwingSpan1 { get; private set; }

        /// <summary>
        ///     回転制限 - 下限 - Y
        /// </summary>
        public float SwingSpan2 { get; private set; }

        /// <summary>
        ///     回転制限 - 下限 - X
        /// </summary>
        public float TwistSpan { get; private set; }

        /// <summary>
        ///     バネ定数 - 移動 - X
        /// </summary>
        public float Softness { get; private set; }

        /// <summary>
        ///     バネ定数 - 移動 - Y
        /// </summary>
        public float BiasFactor { get; private set; }

        /// <summary>
        ///     バネ定数 - 移動 - Z
        /// </summary>
        public float RelaxationFactor { get; private set; }

        /// <summary>
        ///     移動制限 - 下限 - X
        /// </summary>
        public float Damping { get; private set; }

        /// <summary>
        ///     移動制限 - 上限 - X
        /// </summary>
        public float FixThresh { get; private set; }

        /// <summary>
        ///     移動制限-下限 - Z  | モーター有効 - 0:OFF 1:ON
        /// </summary>
        public bool MoterEnabled { get;private set; }

        /// <summary>
        ///     移動制限 - 上限 - Z
        ///     ※ モーター有効の場合。
        /// </summary>
        public float MaxMotorImpluse { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void 読み込む( Stream fs, PMXヘッダ header )
        {
            関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            位置 = ParserHelper.get_Float3( fs );
            回転rad = ParserHelper.get_Float3( fs );

            Vector3 moveLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 moveLimitationMax = ParserHelper.get_Float3( fs );
            Damping = moveLimitationMin.X;
            FixThresh = moveLimitationMax.X;

            MoterEnabled = Math.Abs( moveLimitationMin.Z - 1 ) < 0.3f;   // floatなので誤差防止のため(こんなに大きくいらないけど。)

            if( MoterEnabled )
                MaxMotorImpluse = moveLimitationMax.Z;

            Vector3 rotationLimitationMin = ParserHelper.get_Float3( fs );
            SwingSpan1 = rotationLimitationMin.Z;
            SwingSpan2 = rotationLimitationMin.Y;
            TwistSpan = rotationLimitationMin.X;
            ParserHelper.get_Float3( fs );   // スキップ

            Vector3 springMoveCoefficient = ParserHelper.get_Float3( fs );
            Softness = springMoveCoefficient.X;
            BiasFactor = springMoveCoefficient.Y;
            RelaxationFactor = springMoveCoefficient.Z;
            ParserHelper.get_Float3( fs );   // スキップ
        }
    }
}
