using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SharpDX;

namespace MikuMikuFlex3.PMX
{
    /// <summary>
    ///     軸移動ジョイント。
    /// </summary>
    public class スライダージョイントパラメータ : ジョイントパラメータ
    {
        public int 関連剛体Aのインデックス { get; private set; }

        public int 関連剛体Bのインデックス { get; private set; }

        public Vector3 位置 { get; private set; }

        public Vector3 回転 { get; private set; }

        /// <summary>
        ///     移動制限-下限 - X
        /// </summary>
        public float LowerLinLimit { get; private set; }

        /// <summary>
        ///     移動制限-上限 - X
        /// </summary>
        public float UpperLinLimit { get; private set; }

        /// <summary>
        ///     回転制限-下限 - X
        /// </summary>
        public float LowerAngLimit { get; private set; }

        /// <summary>
        ///     回転制限-上限 - X
        /// </summary>
        public float UpperAngLimit { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - X  | 移動モーター有効 - 0:OFF 1:ON
        /// </summary>
        public bool IsPoweredLinMoter { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - Y
        ///     ※移動モーター有効時
        /// </summary>
        public float TargetLinMotorVelocity { get; private set; }

        /// <summary>
        ///     バネ定数-移動 - Z
        ///     ※移動モーター有効時
        /// </summary>
        public float MaxLinMotorForce { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - X  | 回転モーター有効 - 0:OFF 1:ON
        /// </summary>
        public bool IsPoweredAngMotor { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - Y
        ///     ※回転モーター有効時
        /// </summary>
        public float TargetAngMotorVelocity { get; private set; }

        /// <summary>
        ///     バネ定数-回転 - Z
        ///     ※回転モーター有効時
        /// </summary>
        public float MaxAngMotorForce { get; private set; }


        /// <summary>
        ///     指定されたストリームから読み込む。
        /// </summary>
        internal override void 読み込む( Stream fs, ヘッダ header )
        {
            this.関連剛体Aのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.関連剛体Bのインデックス = ParserHelper.get_Index( fs, header.剛体インデックスサイズ );
            this.位置 = ParserHelper.get_Float3( fs );
            this.回転 = ParserHelper.get_Float3( fs );

            Vector3 moveLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 moveLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMin = ParserHelper.get_Float3( fs );
            Vector3 rotationLimitationMax = ParserHelper.get_Float3( fs );
            Vector3 springMoveCoefficient = ParserHelper.get_Float3( fs );
            Vector3 springRotationCoefficient = ParserHelper.get_Float3( fs );

            this.LowerLinLimit = moveLimitationMin.X;
            this.UpperLinLimit = moveLimitationMax.X;
            this.LowerAngLimit = rotationLimitationMin.X;
            this.UpperAngLimit = rotationLimitationMax.X;

            this.IsPoweredLinMoter = Math.Abs( springMoveCoefficient.X - 1 ) < 0.3f;        // floatなので誤差防止のため(こんなに大きくいらないけど。)

            if( this.IsPoweredLinMoter )
            {
                this.TargetLinMotorVelocity = springMoveCoefficient.Y;
                this.MaxLinMotorForce = springMoveCoefficient.Z;
            }

            this.IsPoweredAngMotor = Math.Abs( springRotationCoefficient.X - 1 ) < 0.3f;    // 同上

            if( this.IsPoweredAngMotor )
            {
                this.TargetAngMotorVelocity = springRotationCoefficient.Y;
                this.MaxAngMotorForce = springRotationCoefficient.Z;
            }
        }
    }
}
