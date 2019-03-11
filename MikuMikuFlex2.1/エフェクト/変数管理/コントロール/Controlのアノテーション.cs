
namespace MikuMikuFlex.エフェクト変数管理
{
    /// <summary>
    ///     コントロールオブジェクトのアノテーションをまとめたもの
    /// </summary>
    class Controlのアノテーション
    {
        public TargetObject Target { get; private set; }

        public bool IsString { get; private set; }


        public Controlのアノテーション( TargetObject target, bool isString )
        {
            Target = target;
            IsString = isString;
        }
    }
}
