using System;
using System.IO;

namespace BveFileExplorer
{
    internal static class PathUtil
    {
        /// <summary>
        /// baseFilePath(ファイル)のあるフォルダを基準に、相対パス path を絶対パスへ変換する。
        /// パスが不正な場合は空文字を返す(例外を投げない)。
        /// </summary>
        public static string CombineAbs(string baseFilePath, string path)
        {
            if (string.IsNullOrWhiteSpace(baseFilePath) || path == null) return "";
            try
            {
                string dir = Path.GetDirectoryName(baseFilePath.Trim());
                if (dir == null) return "";
                return Path.GetFullPath(Path.Combine(dir, path.Trim()));
            }
            catch (ArgumentException) { return ""; }
            catch (NotSupportedException) { return ""; }
            catch (PathTooLongException) { return ""; }
            catch (System.Security.SecurityException) { return ""; }
        }

        /// <summary>例外を投げない File.Exists 相当(相対パス解決込み)</summary>
        public static bool ExistsRelative(string baseFilePath, string path)
        {
            string abs = CombineAbs(baseFilePath, path);
            return abs != "" && File.Exists(abs);
        }

        /// <summary>
        /// originalFilePath から見た targetPath の相対パスを生成する。失敗時は空文字。
        /// </summary>
        public static string GenerateRelativePath(string originalFilePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(originalFilePath) || string.IsNullOrWhiteSpace(targetPath)) return "";

            try
            {
                // % をエスケープ
                Uri u1 = new Uri(originalFilePath.Replace("%", "%25"));
                Uri u2 = new Uri(targetPath.Replace("%", "%25"));

                string relativePath = Uri.UnescapeDataString(u1.MakeRelativeUri(u2).ToString());
                return relativePath.Replace("%25", "%").Replace('/', Path.DirectorySeparatorChar);
            }
            catch (UriFormatException)
            {
                return "";
            }
            catch (InvalidOperationException)
            {
                return "";
            }
        }
    }
}
