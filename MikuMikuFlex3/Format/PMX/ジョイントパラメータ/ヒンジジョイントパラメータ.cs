using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMXFormat
{
    /// <summary>
    ///     軸回転ジョイント。
    /// </summary>
    public class ヒンジジョイントパラメータ : ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転rad { get; private set; }

        /// <summary>
        ///     回転制限-下限 - X
        /// </summary>
        public float Low { get; private set; }

        /// <summary>
        ///     回転制限-上限 - X
        /// </summary>
        public float High { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - X
        /// </summary>
        public float SoftNess { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - Y
        /// </summary>
        public float BiasFactor { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - Z
        /// </summary>
        public float RelaxationFactor { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - X  | 回転モーター有効 - 0:OFF 1:ON
        /// </summary>
        public bool MotorEnabled { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - Y
        ///     ※回転モーター有効時
        /// </summary>
        public float TargetVelocity { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - Z
        ///     ※回転モーター有効時
        /// </summary>
        public float MaxMotorImpulse { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void 読み込む( Stream fs, ヘッダ header )
        {
            this.関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.位置 = ParserHelper.get_Float3( fs );
            this.回転rad = ParserHelper.get_Float3( fs );

            Vector3 moveLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 moveLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 springMoveCoefficient = ParserHelper.get_Float3( fs );
            Vector3 springRotationCoefficient = ParserHelper.get_Float3( fs );

            this.Low = rotationLimitationMin.X;
            this.High = rotationLimitationMax.X;
            this.SoftNess = springMoveCoefficient.X;
            this.BiasFactor = springMoveCoefficient.Y;
            this.RelaxationFactor = springMoveCoefficient.Z;
            this.MotorEnabled = Math.Abs( springRotationCoefficient.X - 1 ) < 0.3f;   // floatなので誤差防止のため(こんなに大きくいらないけど。)
            this.TargetVelocity = springRotationCoefficient.Y;
            this.MaxMotorImpulse = springRotationCoefficient.Z;
        }
    }
}
