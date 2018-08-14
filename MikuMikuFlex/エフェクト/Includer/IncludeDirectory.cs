
namespace MMF.エフェクト.Includer
{
	/// <summary>
	///     Includeファイルの検索に利用するディレクトリ
	/// </summary>
	public class IncludeDirectory
	{
        public string ディレクトリのパス { get; private set; }

        public int 優先度 { get; private set; }


        public IncludeDirectory( string ディレクトリパス, int 優先度 )
		{
			ディレクトリのパス = ディレクトリパス;
			this.優先度 = 優先度;
		}
	}
}
