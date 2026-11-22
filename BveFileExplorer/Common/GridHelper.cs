using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// DataGridView の共通設定・色付け
    /// </summary>
    internal static class GridHelper
    {
        /// <summary>DataTable をバインドし、ソートを禁止する</summary>
        public static void Bind(DataGridView dgv, DataTable dt, bool sortable = false)
        {
            dgv.DataSource = dt;
            if (!sortable) DisableSort(dgv);
        }

        public static void DisableSort(DataGridView dgv)
        {
            foreach (DataGridViewColumn c in dgv.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        /// <summary>列ヘッダのツールチップを先頭列から順に設定する</summary>
        public static void SetHeaderToolTips(DataGridView dgv, params string[] tips)
        {
            for (int i = 0; i < tips.Length && i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].HeaderCell.ToolTipText = tips[i];
            }
        }

        /// <summary>
        /// 行の色付け。
        /// <para>・pathBaseFile 指定時: pathColumn のファイルが存在しなければ黄色</para>
        /// <para>・キー列(0列目)にコメント記号(# ; //)を含む → 水色</para>
        /// <para>・キー列が空 → 灰色</para>
        /// <para>・sectionColor 指定時: キー列に "[" を含む → sectionColor</para>
        /// </summary>
        public static void ColorRows(DataGridView dgv, string pathBaseFile = null, int pathColumn = 1, Color? sectionColor = null)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                if (pathBaseFile != null && pathColumn < row.Cells.Count)
                {
                    string path = CellText(row, pathColumn);
                    if (path != "" && !PathUtil.ExistsRelative(pathBaseFile, path))
                    {
                        row.DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }

                string key = CellText(row, 0);
                if (IsCommentKey(key))
                {
                    row.DefaultCellStyle.BackColor = Color.LightBlue;
                }
                else if (key == "")
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }
                else if (sectionColor.HasValue && key.Contains("["))
                {
                    row.DefaultCellStyle.BackColor = sectionColor.Value;
                }
            }
        }

        public static bool IsCommentKey(string key)
        {
            return key.Contains("#") || key.Contains(";") || key.Contains("//");
        }

        public static string CellText(DataGridViewRow row, int column)
        {
            return row.Cells[column].Value?.ToString() ?? "";
        }

        /// <summary>
        /// グリッドのセル(相対パス)を baseFile 基準で絶対パスにして返す。
        /// 対象外のセル(ヘッダ、別の列)なら null。
        /// </summary>
        public static string ResolveCellPath(DataGridView dgv, int rowIndex, int columnIndex, int fileColumn, string baseFile)
        {
            if (rowIndex < 0 || columnIndex < 0 || columnIndex != fileColumn) return null;
            if (rowIndex >= dgv.Rows.Count || columnIndex >= dgv.Columns.Count) return null;

            string cellValue = dgv.Rows[rowIndex].Cells[columnIndex].Value?.ToString();
            if (string.IsNullOrWhiteSpace(cellValue)) return null;

            string path = PathUtil.CombineAbs(baseFile, cellValue);
            return File.Exists(path) ? path : null;
        }
    }
}
