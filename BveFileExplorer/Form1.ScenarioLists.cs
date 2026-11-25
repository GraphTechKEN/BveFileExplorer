using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// 車両ファイル(Vehicle)タブ・路線ファイル(Route)タブ:
    /// シナリオの Vehicle 行 / Route 行(出現するファイルと重み)の編集・別名保存。
    /// <para>シナリオファイルの仕様: Vehicle = ファイルパス1 * 重み1 | ファイルパス2 * 重み2 | ...(重み省略時は 1)。Route も同じ書式</para>
    /// <para>出現確率 = そのファイルの重み ÷ 重みの合計</para>
    /// </summary>
    public partial class Form1
    {
        private const string EntryColPath = "FilePath";
        private const string EntryColWeight = "Ratio";
        private const string EntryColProbability = "Probability";

        /// <summary>シナリオの Vehicle / Route 行を編集する一覧1つ分</summary>
        private sealed class ScenarioEntryList
        {
            /// <summary>シナリオファイルのキー("Vehicle" / "Route")</summary>
            public string Key;
            /// <summary>メッセージ用の名前("車両" / "マップ")</summary>
            public string Name;
            public DataGridView Grid;
            public Button AddButton;
            public Button RemoveButton;
            public Button SaveAsButton;
            public Label Info;
            public string InfoNormal;
            public string InfoDirty;
            /// <summary>ファイル選択ダイアログのフィルタ</summary>
            public string FileFilter;
            /// <summary>追加するファイルが正しい形式か(ヘッダーを確認)</summary>
            public Func<string, bool> LooksValid;
            /// <summary>行に対応するファイルを表示するための選択肢(cbxVehicle / cbxMapFilePath)</summary>
            public ComboBox Selector;

            /// <summary>未保存の変更があるか</summary>
            public bool Dirty;
            /// <summary>表示更新中は CellValueChanged を変更扱いにしない</summary>
            public bool Updating;
        }

        private ScenarioEntryList _vehicleList;
        private ScenarioEntryList _routeList;

        private IEnumerable<ScenarioEntryList> EntryLists => new[] { _vehicleList, _routeList };

        private void InitScenarioEntryLists()
        {
            _vehicleList = new ScenarioEntryList
            {
                Key = "Vehicle",
                Name = "車両",
                Grid = dgvVehicle,
                AddButton = btnVehicleAdd,
                RemoveButton = btnVehicleRemove,
                SaveAsButton = btnVehicleSaveAs,
                Info = lblVehicleListInfo,
                InfoNormal = "追加・削除・重み(Ratio)の編集ができます。出現確率 = 重み ÷ 重みの合計",
                InfoDirty = "未保存の変更があります(「別名で保存」で新しいシナリオファイルとして保存します。元のファイルは変更しません)",
                FileFilter = "車両ファイル(*.txt)|*.txt",
                LooksValid = path => new Vehicle(path, true).FileVersion > 0,
                Selector = cbxVehicle,
            };
            _routeList = new ScenarioEntryList
            {
                Key = "Route",
                Name = "マップ",
                Grid = dgvRoute,
                AddButton = btnRouteAdd,
                RemoveButton = btnRouteRemove,
                SaveAsButton = btnRouteSaveAs,
                Info = lblRouteListInfo,
                // 路線タブは幅が狭いので短く
                InfoNormal = "重み(Ratio)を編集できます",
                InfoDirty = "未保存の変更があります",
                FileFilter = "マップファイル(*.txt)|*.txt",
                LooksValid = path => new Map(path, true).FileVersion > 0,
                Selector = cbxMapFilePath,
            };

            foreach (ScenarioEntryList list in EntryLists)
            {
                new ToolTip().SetToolTip(list.Info,
                    "出現確率 = 重み ÷ 重みの合計。「別名で保存」で新しいシナリオファイルとして保存します(元のファイルは変更しません)");
            }
        }

        #region 表示

        /// <summary>シナリオの Vehicle / Route 行を編集可能な一覧として表示する</summary>
        private void ShowEntryList(ScenarioEntryList list, List<(string FileName, double Ratio)> entries)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(EntryColPath, typeof(string));
            dt.Columns.Add(EntryColWeight, typeof(double));
            dt.Columns.Add(EntryColProbability, typeof(string));

            if (entries != null)
            {
                foreach (var v in entries)
                {
                    dt.Rows.Add(v.FileName, v.Ratio, "");
                }
            }

            DataGridView dgv = list.Grid;
            list.Updating = true;
            try
            {
                GridHelper.Bind(dgv, dt);
                dgv.Columns[EntryColPath].HeaderCell.ToolTipText = "シナリオファイルからの相対パス(ダブルクリックで開きます)";
                dgv.Columns[EntryColWeight].HeaderCell.ToolTipText = "重み(未指定の場合は1)。セルを編集して変更できます";
                dgv.Columns[EntryColWeight].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv.Columns[EntryColProbability].HeaderText = "出現確率";
                dgv.Columns[EntryColProbability].HeaderCell.ToolTipText = "重み ÷ 重みの合計";
                dgv.Columns[EntryColProbability].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                dgv.Columns[EntryColProbability].ReadOnly = true;
            }
            finally
            {
                list.Updating = false;
            }

            SetEntryListDirty(list, false);
            list.AddButton.Enabled = scenario?.FilePath != null;
            RefreshEntryList(list);
        }

        /// <summary>出現確率・リンク切れ表示・削除ボタンを更新する</summary>
        private void RefreshEntryList(ScenarioEntryList list)
        {
            DataGridView dgv = list.Grid;
            if (dgv.DataSource == null || !dgv.Columns.Contains(EntryColProbability)) return;

            List<DataGridViewRow> rows = EntryRows(list).ToList();
            double total = rows.Sum(r => WeightOf(r));

            list.Updating = true;
            try
            {
                foreach (DataGridViewRow row in rows)
                {
                    row.Cells[EntryColProbability].Value = total > 0
                        ? (WeightOf(row) / total).ToString("0.0%", CultureInfo.InvariantCulture)
                        : "-";

                    // リンク切れは黄色
                    string path = PathOf(row);
                    bool exists = path != "" && PathUtil.ExistsRelative(scenario?.FilePath, path);
                    row.DefaultCellStyle.BackColor = exists ? Color.Empty : Color.Yellow;
                }
            }
            finally
            {
                list.Updating = false;
            }

            list.RemoveButton.Enabled = rows.Count > 0;
        }

        private static IEnumerable<DataGridViewRow> EntryRows(ScenarioEntryList list)
        {
            return list.Grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow);
        }

        private static string PathOf(DataGridViewRow row)
        {
            return row.Cells[EntryColPath].Value?.ToString().Trim() ?? "";
        }

        private static double WeightOf(DataGridViewRow row)
        {
            object v = row.Cells[EntryColWeight].Value;
            return (v is double d && d > 0) ? d : 0;
        }

        private void SetEntryListDirty(ScenarioEntryList list, bool dirty)
        {
            list.Dirty = dirty;
            list.Info.ForeColor = dirty ? Color.Red : SystemColors.ControlText;
            list.Info.Text = dirty ? list.InfoDirty : list.InfoNormal;

            // 別名保存は両方の変更をまとめて保存するので、どちらかに変更があれば両方のボタンを有効にする
            bool anyDirty = EntryLists.Any(l => l.Dirty);
            foreach (ScenarioEntryList l in EntryLists)
            {
                l.SaveAsButton.Enabled = anyDirty;
            }
        }

        /// <summary>
        /// 未保存の変更があれば破棄してよいか確認する。
        /// </summary>
        /// <returns>続行してよい場合 true</returns>
        private bool ConfirmDiscardEntryListChanges()
        {
            List<ScenarioEntryList> dirty = EntryLists.Where(l => l.Dirty).ToList();
            if (dirty.Count == 0) return true;

            string names = string.Join("・", dirty.Select(l => l.Name));
            DialogResult r = MessageBox.Show(
                names + "の一覧(シナリオの " + string.Join(" / ", dirty.Select(l => l.Key)) + " 行)の変更が保存されていません。変更を破棄しますか？",
                "未保存の変更", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return false;

            foreach (ScenarioEntryList l in dirty)
            {
                SetEntryListDirty(l, false);
            }
            return true;
        }

        /// <summary>
        /// 一覧の行をクリックすると、そのファイルを表示する。
        /// 追加・削除で行番号がずれるため、パスで選択肢を探す(追加した未保存のファイルは保存後に表示できる)。
        /// </summary>
        private void SelectEntryFile(ScenarioEntryList list, int rowIndex)
        {
            if (rowIndex < 0 || scenario?.FilePath == null || !list.Grid.Columns.Contains(EntryColPath)) return;

            DataGridViewRow row = list.Grid.Rows[rowIndex];
            if (row.IsNewRow) return;

            string abs = NormalizePath(PathUtil.CombineAbs(scenario.FilePath, PathOf(row)));
            ComboBox selector = list.Selector;
            for (int i = 0; i < selector.Items.Count; i++)
            {
                if (string.Equals(NormalizePath(selector.Items[i].ToString()), abs, StringComparison.OrdinalIgnoreCase))
                {
                    if (selector.SelectedIndex != i) selector.SelectedIndex = i;
                    return;
                }
            }
        }

        #endregion

        #region 編集

        private void btnVehicleAdd_Click(object sender, EventArgs e) => AddEntry(_vehicleList);
        private void btnRouteAdd_Click(object sender, EventArgs e) => AddEntry(_routeList);
        private void btnVehicleRemove_Click(object sender, EventArgs e) => RemoveEntries(_vehicleList);
        private void btnRouteRemove_Click(object sender, EventArgs e) => RemoveEntries(_routeList);
        private void dgvVehicle_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) => ValidateEntryCell(_vehicleList, e);
        private void dgvRoute_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) => ValidateEntryCell(_routeList, e);
        private void dgvVehicle_CellValueChanged(object sender, DataGridViewCellEventArgs e) => OnEntryCellChanged(_vehicleList, e);
        private void dgvRoute_CellValueChanged(object sender, DataGridViewCellEventArgs e) => OnEntryCellChanged(_routeList, e);

        /// <summary>数値列への変換エラーで既定のエラーダイアログを出さない(CellValidating で案内済み)</summary>
        private void dgvVehicle_DataError(object sender, DataGridViewDataErrorEventArgs e) => e.ThrowException = false;
        private void dgvRoute_DataError(object sender, DataGridViewDataErrorEventArgs e) => e.ThrowException = false;

        private void AddEntry(ScenarioEntryList list)
        {
            if (scenario?.FilePath == null || !(list.Grid.DataSource is DataTable dt)) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = list.FileFilter;
                ofd.Title = "追加する" + list.Name + "ファイルを選択してください";
                ofd.InitialDirectory = Path.GetDirectoryName(scenario.FilePath);

                if (ofd.ShowDialog() != DialogResult.OK) return;

                // ファイルヘッダー(BveTs Vehicle / BveTs Map)を確認
                if (!list.LooksValid(ofd.FileName))
                {
                    DialogResult r = MessageBox.Show(
                        list.Name + "ファイルではない可能性があります(ファイルヘッダーを確認できません)。追加しますか？\r\n" + ofd.FileName,
                        "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                }

                // 重複チェック
                string newAbs = NormalizePath(ofd.FileName);
                foreach (DataGridViewRow row in EntryRows(list))
                {
                    string abs = NormalizePath(PathUtil.CombineAbs(scenario.FilePath, PathOf(row)));
                    if (string.Equals(abs, newAbs, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("既に追加されています。重みを変更する場合は Ratio を編集してください。");
                        return;
                    }
                }

                string relative = PathUtil.GenerateRelativePath(scenario.FilePath, ofd.FileName);
                if (relative == "")
                {
                    MessageBox.Show("相対パスを生成できませんでした。シナリオと同じドライブのファイルを選択してください。");
                    return;
                }

                dt.Rows.Add(relative, 1.0, "");
            }

            SetEntryListDirty(list, true);
            RefreshEntryList(list);

            // 追加した行の重みを編集できるようにする
            DataGridViewRow last = EntryRows(list).LastOrDefault();
            if (last != null)
            {
                list.Grid.CurrentCell = last.Cells[EntryColWeight];
            }
        }

        private void RemoveEntries(ScenarioEntryList list)
        {
            DataGridView dgv = list.Grid;
            List<DataGridViewRow> targets = dgv.SelectedCells.Cast<DataGridViewCell>()
                .Select(c => c.OwningRow)
                .Concat(dgv.SelectedRows.Cast<DataGridViewRow>())
                .Where(r => r != null && !r.IsNewRow)
                .Distinct()
                .ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show("削除する行を選択してください。");
                return;
            }

            foreach (DataGridViewRow row in targets)
            {
                dgv.Rows.Remove(row);
            }
            (dgv.DataSource as DataTable)?.AcceptChanges();

            SetEntryListDirty(list, true);
            RefreshEntryList(list);
        }

        /// <summary>入力チェック: パスは空・区切り記号(| *)不可、重みは0以上の数値</summary>
        private void ValidateEntryCell(ScenarioEntryList list, DataGridViewCellValidatingEventArgs e)
        {
            if (list.Updating || e.RowIndex < 0) return;

            string column = list.Grid.Columns[e.ColumnIndex].Name;
            string input = e.FormattedValue?.ToString().Trim() ?? "";

            if (column == EntryColPath)
            {
                if (input == "" || input.IndexOfAny(new[] { '|', '*' }) >= 0)
                {
                    MessageBox.Show("ファイルパスを入力してください(「|」「*」は使用できません)。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
            else if (column == EntryColWeight)
            {
                if (!TryParseWeight(input, out double w) || w < 0)
                {
                    MessageBox.Show("重みには 0 以上の数値を入力してください(例: 1, 2.5)。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }

        private void OnEntryCellChanged(ScenarioEntryList list, DataGridViewCellEventArgs e)
        {
            if (list.Updating || e.RowIndex < 0) return;

            string column = list.Grid.Columns[e.ColumnIndex].Name;
            if (column == EntryColPath || column == EntryColWeight)
            {
                SetEntryListDirty(list, true);
                RefreshEntryList(list);
            }
        }

        private static bool TryParseWeight(string s, out double weight)
        {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out weight)
                || double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out weight);
        }

        #endregion

        #region 別名で保存

        private void btnVehicleSaveAs_Click(object sender, EventArgs e) => SaveScenarioAs();
        private void btnRouteSaveAs_Click(object sender, EventArgs e) => SaveScenarioAs();

        /// <summary>
        /// 変更した一覧(Vehicle / Route の両方)を反映したシナリオファイルを別名で保存する
        /// </summary>
        private void SaveScenarioAs()
        {
            if (scenario?.FilePath == null) return;

            // 編集中のセルを確定し、変更のある一覧を集める
            var changes = new Dictionary<string, string>();
            foreach (ScenarioEntryList list in EntryLists)
            {
                list.Grid.EndEdit();
                if (!list.Dirty) continue;

                List<(string Path, double Weight)> entries = EntryRows(list)
                    .Select(r => (PathOf(r), WeightOf(r)))
                    .Where(x => x.Item1 != "")
                    .ToList();

                if (entries.Count == 0)
                {
                    MessageBox.Show(list.Name + "を1つ以上指定してください。");
                    return;
                }
                if (entries.Sum(x => x.Weight) <= 0)
                {
                    MessageBox.Show(list.Name + "の重みの合計が 0 です。1つ以上に 0 より大きい重みを指定してください。");
                    return;
                }
                changes[list.Key] = string.Join(" | ", entries.Select(x => FormatScenarioEntry(x.Path, x.Weight)));
            }
            if (changes.Count == 0) return;

            string scenarioDir = Path.GetDirectoryName(scenario.FilePath);
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "シナリオファイル(*.txt)|*.txt";
                sfd.Title = "シナリオファイルを別名で保存";
                sfd.InitialDirectory = scenarioDir;
                sfd.FileName = Path.GetFileNameWithoutExtension(scenario.FilePath) + "_edit.txt";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                // 相対パス(Route, Vehicle, Image)がずれないよう、同じフォルダに限定する
                if (!string.Equals(NormalizePath(Path.GetDirectoryName(sfd.FileName)), NormalizePath(scenarioDir), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("ファイル内の相対パスがずれるため、元のシナリオと同じフォルダに保存してください。\r\n" + scenarioDir);
                    return;
                }
                if (string.Equals(NormalizePath(sfd.FileName), NormalizePath(scenario.FilePath), StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("元のシナリオファイルとは別の名前を指定してください。");
                    return;
                }

                try
                {
                    WriteScenarioWithEntries(sfd.FileName, changes);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("保存できませんでした: " + ex.Message);
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("保存できませんでした: " + ex.Message);
                    return;
                }

                foreach (ScenarioEntryList list in EntryLists)
                {
                    SetEntryListDirty(list, false);
                }
                string saved = sfd.FileName;

                // シナリオ一覧が同じフォルダを表示していれば更新
                if (string.Equals(NormalizePath(tbScenarioDirectory.Text), NormalizePath(scenarioDir), StringComparison.OrdinalIgnoreCase))
                {
                    OpenSenaroDirectory(tbScenarioDirectory.Text, true);
                }

                if (MessageBox.Show("保存しました。\r\n" + saved + "\r\n\r\n保存したシナリオを開きますか？",
                        "保存", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    LoadScenario(saved);
                }
            }
        }

        /// <summary>
        /// 元のシナリオファイルの指定キー(Vehicle / Route)の行だけを差し替えて outFile に書き出す。
        /// エンコード(BOM の有無を含む)は元ファイルに合わせ、他の行はそのまま残す。
        /// </summary>
        private void WriteScenarioWithEntries(string outFile, Dictionary<string, string> changes)
        {
            Encoding enc = ScenarioEncodingForWrite();
            string[] lines = File.ReadAllLines(scenario.FilePath, enc);
            FixScenarioHeader(lines, enc);

            var output = new List<string>();
            var replaced = new HashSet<string>();
            foreach (string line in lines)
            {
                string key = changes.Keys.FirstOrDefault(k => IsScenarioKeyLine(line, k));
                if (key != null)
                {
                    output.Add(key + " = " + changes[key]);
                    replaced.Add(key);
                }
                else
                {
                    output.Add(line);
                }
            }

            // 行がない場合はヘッダーの次に追加
            bool hasHeader = output.Count > 0 && output[0].TrimStart().StartsWith("BveTs", StringComparison.OrdinalIgnoreCase);
            int insertAt = hasHeader ? 1 : 0;
            foreach (var change in changes.Where(c => !replaced.Contains(c.Key)))
            {
                output.Insert(insertAt++, change.Key + " = " + change.Value);
            }

            File.WriteAllLines(outFile, output, enc);
        }

        /// <summary>"a.txt * 2.5"(重み1は省略)</summary>
        private static string FormatScenarioEntry(string path, double weight)
        {
            return weight == 1.0 ? path : path + " * " + weight.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            try
            {
                return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            }
            catch (ArgumentException) { return path; }
            catch (NotSupportedException) { return path; }
            catch (PathTooLongException) { return path; }
            catch (System.Security.SecurityException) { return path; }
        }

        #endregion
    }
}
