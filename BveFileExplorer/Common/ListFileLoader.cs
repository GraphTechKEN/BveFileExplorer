using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BveFileExplorer
{
    /// <summary>
    /// BVEのCSV形式リストファイル(ストラクチャ/サウンド/固定音源/停車場/信号)を DataTable に読み込む。
    /// いずれも1行目はヘッダ("BveTs Structure List 2.00" 等)として読み飛ばす。
    /// UIには依存しない。
    /// </summary>
    internal static class ListFileLoader
    {
        private static readonly char[] CommentChars = { '#', ';' };

        /// <summary>
        /// キー, ファイルパス[, 追加列...], 備考 の形式のリストを読み込む。
        /// 2列目以降に現れた '#' ';' 以降を備考列にまとめる。
        /// </summary>
        /// <param name="columns">列名。最後の列が備考列になる</param>
        public static DataTable LoadWithRemarks(string filePath, Encoding enc, string[] columns)
        {
            DataTable dt = CreateTable(columns);
            int dataColumnCount = columns.Length - 1; // 備考列を除く列数

            foreach (string[] fields in ReadDataRows(filePath, enc))
            {
                dt.Rows.Add(SplitRemarks(fields, columns.Length, dataColumnCount));
            }
            return dt;
        }

        /// <summary>
        /// 1行分のフィールドを [データ列..., 備考] の配列へ整形する。
        /// </summary>
        internal static object[] SplitRemarks(string[] fields, int totalColumns, int dataColumnCount)
        {
            object[] row = new object[totalColumns];

            // 先頭(キー)列はコメント記号を含んでいてもそのまま残す(コメント行として色付けするため)
            int commentAt = -1;
            string remark = null;
            for (int i = 1; i < fields.Length; i++)
            {
                int p = fields[i].IndexOfAny(CommentChars);
                if (p >= 0)
                {
                    commentAt = i;
                    // コメント記号以降 + 後続のフィールド(カンマで分割されてしまった部分)を備考とする
                    var tail = new List<string> { fields[i].Substring(p).TrimStart(CommentChars).Trim() };
                    tail.AddRange(fields.Skip(i + 1));
                    remark = string.Join(",", tail).Trim();
                    fields[i] = fields[i].Substring(0, p).Trim();
                    break;
                }
            }

            if (remark != null)
            {
                int count = Math.Min(commentAt + 1, dataColumnCount);
                for (int i = 0; i < count; i++) row[i] = fields[i];
                row[totalColumns - 1] = remark;
            }
            else
            {
                // 備考なし: 列数に収まればすべて、超える場合はデータ列のみ
                int count = fields.Length <= totalColumns ? fields.Length : dataColumnCount;
                for (int i = 0; i < count; i++) row[i] = fields[i];
            }
            return row;
        }

        /// <summary>
        /// 固定列のリストを読み込む(列数を超える分は切り捨て)。停車場リスト用。
        /// </summary>
        public static DataTable LoadFixedColumns(string filePath, Encoding enc, string[] columns)
        {
            DataTable dt = CreateTable(columns);
            foreach (string[] fields in ReadDataRows(filePath, enc))
            {
                if (fields.Length <= columns.Length)
                {
                    dt.Rows.Add(fields);
                }
                else
                {
                    dt.Rows.Add(fields.Take(columns.Length).Cast<object>().ToArray());
                }
            }
            return dt;
        }

        /// <summary>
        /// 列数可変のリストを読み込む。列名は "1","2",… 。信号現示リスト用。
        /// </summary>
        public static DataTable LoadVariableColumns(string filePath, Encoding enc)
        {
            List<string[]> rows = ReadDataRows(filePath, enc).ToList();
            DataTable dt = new DataTable();
            if (rows.Count == 0) return dt;

            int colnum = rows.Max(r => r.Length);
            for (int i = 0; i < colnum; i++)
            {
                dt.Columns.Add((i + 1).ToString());
            }
            foreach (string[] fields in rows)
            {
                dt.Rows.Add(fields);
            }
            return dt;
        }

        private static DataTable CreateTable(IEnumerable<string> columns)
        {
            DataTable dt = new DataTable();
            foreach (string c in columns) dt.Columns.Add(c);
            return dt;
        }

        /// <summary>1行目(ヘッダ)を読み飛ばし、カンマ区切りで各行を返す</summary>
        private static IEnumerable<string[]> ReadDataRows(string filePath, Encoding enc)
        {
            using (TextFieldParser parser = new TextFieldParser(filePath, enc))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                if (!parser.EndOfData)
                {
                    parser.ReadLine();
                }

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    if (fields != null) yield return fields;
                }
            }
        }
    }
}
