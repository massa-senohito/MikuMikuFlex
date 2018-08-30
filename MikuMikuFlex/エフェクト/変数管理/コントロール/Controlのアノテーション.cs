
namespace MikuMikuFlex.エフェクト.変数管理.コントロール
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
