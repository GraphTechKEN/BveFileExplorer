namespace BveFileExplorer
{
    /// <summary>
    /// 車両ファイル・マップファイルから参照されるファイル1件分の情報
    /// </summary>
    public interface IFileReference
    {
        /// <summary>絶対パス</summary>
        string AbsolutePath { get; }

        /// <summary>1:存在する / -1:見つからない・未対応 / 0:未宣言</summary>
        int Ret { get; }
    }
}
