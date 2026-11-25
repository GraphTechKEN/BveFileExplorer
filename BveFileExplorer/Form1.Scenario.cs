using BveFileExplorer.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// シナリオファイルの表示・シナリオ一覧
    /// </summary>
    public partial class Form1
    {
        /// <summary>シナリオファイルのエンコード手動指定(Reset ではクリアしない。別のシナリオを開くとクリア)</summary>
        private Encoding _scenarioEncOverride;
        private string _scenarioEncOverridePath;

        /// <summary>シナリオ一覧で最後にクリックした行(-1:なし)</summary>
        private int indexDgvFiles = -1;

        private bool flgListBVE5 = false;
        private bool flgListBVE6 = false;
        private bool flgListNoVehicle = false;
        private bool flgListNoMap = false;

        #region シナリオファイル

        private void btnOpenScenario_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "路線ファイル(*.txt)|*.txt";
                ofd.InitialDirectory = Settings.Default.RouteFileDirectory;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    TempFileChecker();
                    Settings.Default.RouteFileDirectory = Path.GetDirectoryName(ofd.FileName);
                    Settings.Default.Save();

                    LoadScenario(ofd.FileName);
                }
            }
        }

        private void btnScenarioReload_Click(object sender, EventArgs e)
        {
            LoadScenario(tbSeinarioFileName.Text);
        }

        private void btnScenarioClear_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardEntryListChanges()) return;

            Reset();
            indexDgvFiles = -1;
            _scenarioEncOverride = null;
            _scenarioEncOverridePath = null;
            btnScenarioClear.Enabled = false;
            btnScenarioReload.Enabled = false;
        }

        /// <summary>
        /// シナリオファイルを読み込む。
        /// </summary>
        /// <param name="enc">null の場合、同じシナリオに手動指定があればそれを、なければヘッダーから判定</param>
        /// <returns>読み込んだ場合 true(未保存の変更の破棄をキャンセルした場合 false)</returns>
        private bool LoadScenario(string scenarioFilePath, Encoding enc = null)
        {
            if (!ConfirmDiscardEntryListChanges()) return false;

            // シナリオのエンコード手動指定は、同じシナリオの読み直し(リロード等)の間だけ維持する
            if (enc == null && string.Equals(scenarioFilePath, _scenarioEncOverridePath, StringComparison.OrdinalIgnoreCase))
            {
                enc = _scenarioEncOverride;
            }

            Reset();
            _scenarioEncOverride = enc;
            _scenarioEncOverridePath = (enc != null) ? scenarioFilePath : null;

            scenario = new Scenario(scenarioFilePath, enc);
            OpenScenario();
            return true;
        }

        /// <summary>
        /// シナリオファイルのエンコードを手動で切り替える(ヘッダーの shift_jis 記載漏れ・保存ミス対策)。
        /// シナリオを読み直すため、車両・マップの表示も読み直しになる。
        /// </summary>
        private void cbxScenarioEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsUserEncodingChange(cbxScenarioEnc)) return;

            string path = scenario?.FilePath;
            if (!File.Exists(path)) return;

            if (!LoadScenario(path, EncodingDetector.FromMode(cbxScenarioEnc.SelectedIndex)))
            {
                // キャンセルされたら表示を元に戻す
                SetEncIndex(cbxScenarioEnc, scenario.encMode);
            }
        }

        private void OpenScenario()
        {
            btnOpenScenarioFile.Enabled = true;
            btnScenarioReload.Visible = true;
            btnAtsPluginSelect.Enabled = true;
            btnBootBVE5.Enabled = true;
            tbSeinarioFileName.Enabled = true;
            tbSeinarioFileName.Text = scenario.FilePath;
            btnScenarioClear.Enabled = true;
            btnScenarioReload.Enabled = true;
            SetEncIndex(cbxScenarioEnc, scenario.encMode);
            cbxScenarioEnc.Enabled = scenario.FilePath != null;
            if (scenario.HeaderWarning != "")
            {
                strDisp += "シナリオファイル: " + scenario.HeaderWarning + "\n";
            }

            if (!string.IsNullOrEmpty(scenario.ImagePath))
            {
                pbVehicleImage.Visible = true;
                pbVehicleImage.ImageLocation = scenario.ImagePath;
            }
            else
            {
                pbVehicleImage.Visible = false;
            }

            dgvScenario.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvScenario.Rows.Add("タイトル", scenario.Title);
            dgvScenario.Rows.Add("路　線", scenario.RouteTitle);
            dgvScenario.Rows.Add("車　両", scenario.VehicleTitle);
            dgvScenario.Rows.Add("作　者", scenario.Author);
            dgvScenario.Rows.Add("コメント", scenario.Comment);
            dgvScenario[1, 4].Style.WrapMode = DataGridViewTriState.True;
            dgvScenario.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            ShowScenarioVehicles();
            ShowEntryList(_vehicleList, scenario.VehicleFilesList);
            ShowEntryList(_routeList, scenario.MapFilesList);
            ShowScenarioMaps();
            FlushMessages();
        }

        private void ShowScenarioVehicles()
        {
            if (scenario.VehicleFilesExistsCount <= 0)
            {
                // 車両ファイルが空欄のとき
                btnBootBVE5.Enabled = false;
                btnBootBVE6.Enabled = false;
                btnBve5BootChooseVehicle.Enabled = false;
                btnBve6BootChooseVehicle.Enabled = false;
                cbxVehicle.Text = "Not defined";
                cbxVehicle.BackColor = Color.LightYellow;
                strDisp += "車両ファイルが指定されていません。\n";
                return;
            }

            cbxVehicle.BackColor = SystemColors.Window;
            cbxVehicle.Text = scenario.VehicleFilesAbs[0];
            OpenNewVehicleFile(scenario.VehicleFilesAbs[0]);
            cbxVehicle.Items.AddRange(scenario.VehicleFilesAbs.ToArray());

            if (scenario.VehicleFilesCount > 1)
            {
                btnBve5BootChooseVehicle.Enabled = btnBootBVE5.Enabled;
                btnBve6BootChooseVehicle.Enabled = btnBootBVE6.Enabled;
                strDisp += "車両ファイルが複数あります。データ数：" + scenario.VehicleFilesCount + "\n";
                cbxVehicle.SelectedIndex = 0;
                if (scenario.VehicleFilesNotExistsCount > 0)
                {
                    btnOpenVehicleFile.BackColor = Color.Yellow;
                    strDisp += "車両ファイルのリンク切れがあります。データ数：" + scenario.VehicleFilesNotExistsCount + "\n";
                }
            }
            else
            {
                btnBve5BootChooseVehicle.Enabled = false;
                btnBve6BootChooseVehicle.Enabled = false;
            }

            if (tbAtsPluginFile.Text != "" && File.Exists(tbAtsPluginFile.Text))
            {
                AddAtsPlugin(tbAtsPluginFile.Text);
            }
        }

        private void ShowScenarioMaps()
        {
            if (scenario.MapFilesCount <= 0)
            {
                // マップファイルが空欄のとき
                btnBootBVE5.Enabled = false;
                btnBootBVE6.Enabled = false;
                btnBve5BootChooseMap.Enabled = false;
                btnBve6BootChooseMap.Enabled = false;
                cbxMapFilePath.Text = "Not defined";
                cbxMapFilePath.BackColor = Color.LightYellow;
                strDisp += "マップファイルが指定されていません。\n";
                return;
            }

            if (scenario.MapFilesAbs.Any(File.Exists))
            {
                listMapFilePath.AddRange(scenario.MapFilesAbs);

                if (scenario.MapFilesCount > 1)
                {
                    btnBve5BootChooseMap.Enabled = btnBootBVE5.Enabled;
                    btnBve6BootChooseMap.Enabled = btnBootBVE6.Enabled;
                    strDisp += "マップファイルが複数あります。データ数：" + scenario.MapFilesCount + "\n";
                }
                else
                {
                    btnBve5BootChooseMap.Enabled = false;
                    btnBve6BootChooseMap.Enabled = false;
                }
                cbxMapFilePath.Items.AddRange(listMapFilePath.ToArray());
                cbxMapFilePath.BackColor = SystemColors.Window;

                OpenNewMapFile(listMapFilePath[0]);
                IncludeFileExtract();
            }
            else
            {
                // マップファイルが見つからない場合
                btnBootBVE5.Enabled = false;
                btnBootBVE6.Enabled = false;
                btnBve5BootChooseMap.Enabled = false;
                btnBve6BootChooseMap.Enabled = false;
                listMapFilePath.AddRange(scenario.MapFilesAbs.Select(p => "Not found or supported : " + p));
                cbxMapFilePath.Text = "Not found or supported : " + scenario.MapFilesAbs[0];
                cbxMapFilePath.BackColor = Color.LightYellow;
                strDisp += "マップファイルが見つかりません。\n";
            }
        }

        #endregion

        #region 選択車両/マップで起動するための一時シナリオファイル

        private string TempScenarioPath() => scenario.FilePath + ".tmp.txt";

        /// <summary>
        /// シナリオファイルの key(Vehicle または Route)だけを targetFilePath に差し替えた
        /// 一時シナリオファイルを作成し、そのパスを返す。元のシナリオファイルは変更しない。
        /// </summary>
        private string CreateTempScenario(string key, string targetFilePath)
        {
            if (scenario?.FilePath == null || string.IsNullOrEmpty(targetFilePath)) return null;

            string source = scenario.FilePath;
            string tempFile = TempScenarioPath();

            // 読み込んだときと同じエンコード(手動指定を含む)で読み書きする
            Encoding enc = ScenarioEncodingForWrite();
            string[] lines = File.ReadAllLines(source, enc);
            FixScenarioHeader(lines, enc);

            for (int i = 0; i < lines.Length; i++)
            {
                if (IsScenarioKeyLine(lines[i], key))
                {
                    lines[i] = key + " = " + PathUtil.GenerateRelativePath(source, targetFilePath);
                }
            }

            File.WriteAllLines(tempFile, lines, enc);
            return tempFile;
        }

        /// <summary>
        /// シナリオファイルを書き出すときのエンコード。読込時のエンコード(手動指定を含む)に合わせ、
        /// UTF-8 の場合は元ファイルの BOM の有無も合わせる。
        /// </summary>
        private Encoding ScenarioEncodingForWrite()
        {
            Encoding enc = scenario.FileEncoding ?? EncodingDetector.Detect(scenario.FilePath, out _);
            if (enc.CodePage != 65001) return enc;

            bool hasBom = false;
            using (FileStream fs = new FileStream(scenario.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] head = new byte[3];
                hasBom = fs.Read(head, 0, 3) == 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF;
            }
            return new UTF8Encoding(hasBom);
        }

        /// <summary>
        /// ヘッダーの記載漏れ等で手動指定している場合、BVE が正しく読めるようヘッダーを実際のエンコードに合わせる
        /// </summary>
        private static void FixScenarioHeader(string[] lines, Encoding enc)
        {
            if (lines.Length == 0) return;

            EncodingDetector.HeaderInfo header = EncodingDetector.ParseHeader(lines[0]);
            if (header.Encoding.CodePage != enc.CodePage || header.Warning != "")
            {
                lines[0] = EncodingDetector.RewriteHeader(lines[0], enc);
            }
        }

        /// <summary>
        /// "key = ..." の行か(コメント行は除く)。"VehicleTitle" や "RouteTitle" は "Vehicle"/"Route" と一致しない
        /// </summary>
        private static bool IsScenarioKeyLine(string rawLine, string key)
        {
            string line = rawLine.TrimStart();
            if (line.StartsWith(";") || line.StartsWith("#")) return false;

            int eq = line.IndexOf('=');
            return eq >= 0 && string.Equals(line.Substring(0, eq).Trim(), key, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region シナリオ一覧

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = Settings.Default.RouteFileDirectory;
                folderBrowserDialog.ShowNewFolderButton = true;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    OpenSenaroDirectory(folderBrowserDialog.SelectedPath, true);
                }
            }
        }

        private async void OpenSenaroDirectory(string selectedFolderPath, bool IsReadIndexOnly = false)
        {
            if (!Directory.Exists(selectedFolderPath))
            {
                btnOpenScenarioDirectory.Enabled = false;
                return;
            }

            lblLegend.Text = "読込中";
            pgBarList.Visible = true;
            SetListLegendVisible(false);
            btnOpenScenarioDirectory.Enabled = true;
            dgvFiles.Rows.Clear();
            indexDgvFiles = -1;

            try
            {
                string[] files = Directory.GetFiles(selectedFolderPath, "*.txt");
                tbScenarioDirectory.Text = selectedFolderPath;
                Settings.Default.RouteFileDirectory = selectedFolderPath;
                Settings.Default.Save();

                pgBarList.Minimum = 0;
                pgBarList.Maximum = files.Length;
                pgBarList.Value = 0;
                lblPgBarStatus.Visible = true;
                lblPgBarStatus.Text = "";

                // UIスレッドで実行されるプログレスハンドラ
                int loadedCount = 0;
                var progress = new Progress<ScenarioRow>(row =>
                {
                    loadedCount++;
                    pgBarList.Value = Math.Min(loadedCount, pgBarList.Maximum);
                    lblPgBarStatus.Text = loadedCount + "/" + files.Length;
                    dgvFiles.Rows.Add(row.FileName, row.Author, row.VehicleCount, row.MapCount, row.VehicleVersion, row.MapVersion);
                });

                try
                {
                    await Task.Run(() => LoadFilesAsync(files, progress, IsReadIndexOnly));
                }
                finally
                {
                    DgvFilesViewColoring();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ファイルの取得中にエラーが発生しました: " + ex.Message);
                btnOpenScenarioDirectory.Enabled = false;
            }

            lblLegend.Text = "凡例";
            pgBarList.Visible = false;
            lblPgBarStatus.Visible = false;
            SetListLegendVisible(true);
        }

        private void SetListLegendVisible(bool visible)
        {
            btnListBVE5.Visible = visible;
            btnListBVE6.Visible = visible;
            btnListNoVehicle.Visible = visible;
            btnListNoMap.Visible = visible;
            btnListOther.Visible = visible;
        }

        /// <summary>シナリオファイルを順に解析する(バックグラウンドスレッドで実行)</summary>
        private static void LoadFilesAsync(string[] files, IProgress<ScenarioRow> progress, bool IsReadIndexOnly)
        {
            foreach (string file in files)
            {
                Scenario sn = new Scenario(file);
                var row = new ScenarioRow
                {
                    FileName = Path.GetFileName(file),
                    Author = sn.Author ?? "",
                    VehicleCount = sn.VehicleFilesExistsCount,
                    MapCount = sn.MapFilesCount,
                    VehicleVersion = "",
                    MapVersion = ""
                };

                if (sn.VehicleFilesExistsCount > 0)
                {
                    row.VehicleVersion = new Vehicle(sn.VehicleFilesAbs[0], IsReadIndexOnly).FileVersion.ToString("0.00");
                }
                if (sn.MapFilesCount > 0)
                {
                    row.MapVersion = new Map(sn.MapFilesAbs[0], IsReadIndexOnly).FileVersion.ToString("0.00");
                }

                progress.Report(row);
            }
        }

        private void DgvFilesViewColoring()
        {
            foreach (DataGridViewRow row in dgvFiles.Rows)
            {
                if (row.IsNewRow) continue;

                int.TryParse(GridHelper.CellText(row, 2), out int vehicleFilesCount);
                int.TryParse(GridHelper.CellText(row, 3), out int mapFilesCount);
                double.TryParse(GridHelper.CellText(row, 4), out double vehicleFilesVersion);

                Color color;
                if (vehicleFilesCount == 0 && mapFilesCount == 0)
                {
                    color = Color.DarkGray;
                }
                else if (vehicleFilesCount == 0 && flgListNoVehicle)
                {
                    color = Color.LightYellow;
                }
                else if (mapFilesCount == 0 && flgListNoMap)
                {
                    color = Color.LightGray;
                }
                else if (vehicleFilesVersion == 2.0 && flgListBVE6 && mapFilesCount > 0)
                {
                    color = Color.Thistle;
                }
                else if (vehicleFilesVersion > 0.0 && vehicleFilesVersion <= 2.0 && flgListBVE5 && mapFilesCount > 0)
                {
                    color = Color.PaleGreen;
                }
                else
                {
                    color = SystemColors.Window;
                }
                row.DefaultCellStyle.BackColor = color;
            }
        }

        private void btnListBVE5_Click(object sender, EventArgs e)
        {
            flgListBVE5 = !flgListBVE5;
            btnListBVE5.BackColor = flgListBVE5 ? Color.Green : Color.PaleGreen;
            btnListBVE5.ForeColor = flgListBVE5 ? Color.White : Color.Black;
            DgvFilesViewColoring();
        }

        private void btnListBVE6_Click(object sender, EventArgs e)
        {
            flgListBVE6 = !flgListBVE6;
            btnListBVE6.BackColor = flgListBVE6 ? Color.Purple : Color.Thistle;
            btnListBVE6.ForeColor = flgListBVE6 ? Color.White : Color.Black;
            DgvFilesViewColoring();
        }

        private void btnListNoVehicle_Click(object sender, EventArgs e)
        {
            flgListNoVehicle = !flgListNoVehicle;
            btnListNoVehicle.BackColor = flgListNoVehicle ? Color.Yellow : Color.LightYellow;
            DgvFilesViewColoring();
        }

        private void btnListNoMap_Click(object sender, EventArgs e)
        {
            flgListNoMap = !flgListNoMap;
            btnListNoMap.BackColor = flgListNoMap ? Color.Gray : Color.LightGray;
            btnListNoMap.ForeColor = flgListNoMap ? Color.White : Color.Black;
            DgvFilesViewColoring();
        }

        private string ScenarioListFilePath(int rowIndex)
        {
            string fileName = GridHelper.CellText(dgvFiles.Rows[rowIndex], 0);
            return Path.Combine(tbScenarioDirectory.Text, fileName);
        }

        private void dgvFiles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e != null && e.RowIndex >= 0 && e.RowIndex != indexDgvFiles)
            {
                if (LoadScenario(ScenarioListFilePath(e.RowIndex)))
                {
                    indexDgvFiles = e.RowIndex;
                }
            }
        }

        private void dgvFiles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e != null && e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                OpenFile(ScenarioListFilePath(e.RowIndex));
            }
        }

        #endregion
    }
}
