using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BveFileExplorer
{
    /// <summary>
    /// "Key = Value ;備考" 形式(車両パラメータ、サウンド、他列車等)のファイルを DataTable に読み込む。
    /// UIには依存しない。
    /// </summary>
    internal static class IniTableLoader
    {
        private static readonly char[] CommentChars = { ';', '#' };

        /// <summary>
        /// Key / Value / Remarks の3列で読み込む。
        /// </summary>
        /// <param name="removeFirstRow">先頭行(ファイルヘッダ)を除くか</param>
        public static DataTable LoadKeyValue(string filePath, Encoding enc, bool removeFirstRow)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value");
            dt.Columns.Add("Remarks");

            foreach (string line in File.ReadAllLines(filePath, enc))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // '=' で分割する(値に'='が入っても問題ない)
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length >= 2)
                {
                    string[] valueAndRemark = parts[1].Split(CommentChars, 2);
                    if (valueAndRemark.Length < 2)
                    {
                        dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                    }
                    else
                    {
                        dt.Rows.Add(parts[0].Trim(), valueAndRemark[0].Trim(), valueAndRemark[1].Trim());
                    }
                }
                else
                {
                    // '='がない行(セクション、コメント等)
                    dt.Rows.Add(parts[0].Trim(), "");
                }
            }

            RemoveFirstRowIf(dt, removeFirstRow);
            return dt;
        }

        /// <summary>
        /// パネルファイル用。Key / Value1 / Value2 / Remarks の4列で読み込む。
        /// </summary>
        public static DataTable LoadPanel(string filePath, Encoding enc, bool removeFirstRow)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value1");
            dt.Columns.Add("Value2");
            dt.Columns.Add("Remarks");

            foreach (string line in File.ReadAllLines(filePath, enc))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length >= 2)
                {
                    // 値を ',' で分割する
                    string[] values = parts[1].Split(',');
                    if (values.Length < 2)
                    {
                        // color 指定は "#RRGGBB" を含むため '#' をコメント扱いしない
                        bool isColor = parts[1].IndexOf("color", StringComparison.OrdinalIgnoreCase) >= 0;
                        char[] del = isColor ? new[] { ';' } : CommentChars;
                        string[] valueAndRemark = parts[1].Split(del, 2);

                        if (valueAndRemark.Length < 2)
                        {
                            dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                        }
                        else
                        {
                            dt.Rows.Add(parts[0].Trim(), valueAndRemark[0].Trim(), "", valueAndRemark[1].Trim());
                        }
                    }
                    else
                    {
                        string[] valueAndRemark = values[1].Split(CommentChars, 2);
                        if (valueAndRemark.Length < 2)
                        {
                            dt.Rows.Add(parts[0].Trim(), values[0].Trim(), values[1].Trim());
                        }
                        else
                        {
                            dt.Rows.Add(parts[0].Trim(), values[0].Trim(), valueAndRemark[0].Trim(), valueAndRemark[1].Trim());
                        }
                    }
                }
                else
                {
                    dt.Rows.Add(line.Trim());
                }
            }

            RemoveFirstRowIf(dt, removeFirstRow);
            return dt;
        }

        private static readonly Regex IncludeKeyword = new Regex("include", RegexOptions.IgnoreCase);

        /// <summary>
        /// マップファイル中の include 文を FilePath / Remarks の2列で抽出する。
        /// </summary>
        public static DataTable LoadIncludes(string mapFilePath, Encoding enc)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("FilePath");
            dt.Columns.Add("Remarks");

            foreach (string line in File.ReadAllLines(mapFilePath, enc))
            {
                string trimmedLine = line.Trim();
                if (trimmedLine.Length == 0) continue;
                if (trimmedLine.IndexOf("include", StringComparison.OrdinalIgnoreCase) < 0) continue;

                // ';','#' 以降を備考として分割
                string[] parts = trimmedLine.Split(new[] { ';', '#' }, 2);

                // include キーワード(大文字小文字問わず)と引用符を除去
                string filePath = IncludeKeyword.Replace(parts[0], "").Replace("'", "").Trim();
                string remarks = (parts.Length >= 2) ? parts[1].Trim() : "";

                dt.Rows.Add(filePath, remarks);
            }
            return dt;
        }

        private static void RemoveFirstRowIf(DataTable dt, bool remove)
        {
            if (remove && dt.Rows.Count > 0) dt.Rows.RemoveAt(0);
        }
    }
}
