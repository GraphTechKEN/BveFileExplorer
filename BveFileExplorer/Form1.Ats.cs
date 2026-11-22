using BveFileExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// ATSプラグイン(DetailManager / detailmodules.txt)・BVE6簡易コンバータ
    /// </summary>
    public partial class Form1
    {
        private const string DetailManagerBve6Hint = @"例:\BveTs\Scenarios\UchAibo20\E217r\Ats\x64\DetailManager.dll等";

        #region detailmodules.txt の表示

        /// <summary>detailmodules.txt の内容をグリッドに表示する</summary>
        /// <param name="bve_ver">BVE5:32bit側 / BVE6:64bit側</param>
        private void OpenNewAtsPluginFile(string settingFilePath, BVE_Version bve_ver)
        {
            if (!File.Exists(settingFilePath))
            {
                strDisp += "ATSプラグインが見つかりません:" + settingFilePath + "\n";
                return;
            }

            bool is32 = bve_ver == BVE_Version.BVE5;
            DataGridView dgv = is32 ? dgvAts32 : dgvAts64;

            (is32 ? btnAts32OpenDirectory : btnAts64OpenDirectory).Enabled = true;
            (is32 ? btnAts32Check : btnAts64Check).Enabled = true;

            dgv.Rows.Clear();
            AtsPlugin plugin = new AtsPlugin(is32 ? vehicle.Ats32.FilePath : vehicle.Ats64.FilePath);
            foreach (AtsList ats in plugin.AtsList)
            {
                dgv.Rows.Add(ats.FileName, ats.Version.ToString(), ats.RerativePath, ats.AbsolutePath);
            }
            ColoringDataGridView(dgv, bve_ver);

            string savedSetting = is32 ? strAts32SettingTextFilePath : strAts64SettingTextFilePath;
            if (File.Exists(savedSetting + ".bak"))
            {
                (is32 ? btnAts32Recovery : btnAts64Recovery).Visible = true;
            }
        }

        /// <summary>
        /// 見つからない:赤 / ビット数が逆:黄 / コメントアウト行:灰
        /// </summary>
        private static void ColoringDataGridView(DataGridView dataGridView, BVE_Version version)
        {
            BVE_Version wrongVersion = (version == BVE_Version.BVE5) ? BVE_Version.BVE6 : BVE_Version.BVE5;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.IsNewRow) continue;

                string build = GridHelper.CellText(row, 1);
                if (build == BVE_Version.NotFound.ToString())
                {
                    row.Cells[1].Style.BackColor = Color.Red;
                }
                if (build == wrongVersion.ToString())
                {
                    row.Cells[1].Style.BackColor = Color.Yellow;
                }

                string relativePath = GridHelper.CellText(row, 2);
                if (relativePath.StartsWith("#") || relativePath.StartsWith(";"))
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
        }

        private void tsmi32Delete_Click(object sender, EventArgs e)
        {
            DataGridView dgv = (tabControlDetailModules.SelectedIndex == 0) ? dgvAts32 : dgvAts64;
            foreach (DataGridViewRow r in dgv.SelectedRows)
            {
                if (!r.IsNewRow)
                {
                    dgv.Rows.Remove(r);
                }
            }
        }

        #endregion

        #region ビット数チェック

        /// <summary>プラグインのビット数を判定し、必要なら結果を表示する</summary>
        private static BVE_Version AtsPluginChecker(string filePath, bool showResult)
        {
            BVE_Version result = AtsPlugin.Checker(filePath);
            if (showResult)
            {
                switch (result)
                {
                    case BVE_Version.BVE5:
                        MessageBox.Show("BVE5用(32bit)にビルドされたプラグインです");
                        break;
                    case BVE_Version.BVE6:
                        MessageBox.Show("BVE6用(64bit)にビルドされたプラグインです");
                        break;
                    case BVE_Version.Null:
                        MessageBox.Show("判定できませんでした");
                        break;
                }
            }
            return result;
        }

        private void btnAts32Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle?.Ats32?.FilePath, true);
        }

        private void btnAts64Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle?.Ats64?.FilePath, true);
        }

        #endregion

        #region ATSプラグインの追記

        private void btnAtsPluginSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "";
                ofd.Filter = "ATSプラグインファイル(*.dll)|*.dll";
                ofd.Title = "ATSプラグインファイルを選択してください";
                ofd.InitialDirectory = Settings.Default.AtsPluginFileDirectory;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AddAtsPlugin(ofd.FileName);
                }
            }
        }

        private void AddAtsPlugin(string fileName)
        {
            Settings.Default.AtsPluginFileDirectory = Path.GetDirectoryName(fileName);
            Settings.Default.AtsPluginFilePath = fileName;
            strAtsPluginFilePath = fileName;
            tbAtsPluginFile.Text = fileName;
            btnOpenScenario.Enabled = true;
            tbAtsPluginFile.Enabled = true;

            BVE_Version iRet = AtsPluginChecker(strAtsPluginFilePath, cbMessageDisp.Checked);
            tbAts32RelatePath.Text = "";
            tbAts64RelatePath.Text = "";
            tbAts32RelatePath.Enabled = false;
            tbAts64RelatePath.Enabled = false;

            if (iRet == BVE_Version.BVE5)
            {
                lblAtsPluginFile.Text = "ATSプラグイン BVE5用 (32bit)ビルド";
                btnAts64Add.Enabled = false;
                btnAts64RelatePathGen.Enabled = false;
                btnAts32Add.Enabled = false;

                if (tbSeinarioFileName.Text != "" && strAts32SettingTextFilePath != "")
                {
                    btnAts32RelatePathGen.Enabled = true;
                    tbAts32.Enabled = true;
                }
            }
            else if (iRet == BVE_Version.BVE6)
            {
                lblAtsPluginFile.Text = "ATSプラグイン BVE6用 (64bit)ビルド";
                btnAts32Add.Enabled = false;
                btnAts32RelatePathGen.Enabled = false;
                btnAts64Add.Enabled = false;

                if (tbSeinarioFileName.Text != "" && strAts64SettingTextFilePath != "")
                {
                    bool isBve6Vehicle = vehicle != null && vehicle.FileVersion >= 2.0;
                    btnAts64RelatePathGen.Enabled = isBve6Vehicle;
                    tbAts64.Enabled = isBve6Vehicle;
                }
            }
        }

        private void btnAts32RPathGen_Click(object sender, EventArgs e)
        {
            GenerateAtsRelativePath(tbAts32RelatePath, strAts32SettingTextFilePath, btnAts32Add);
        }

        private void btnAts64RPathGen_Click(object sender, EventArgs e)
        {
            GenerateAtsRelativePath(tbAts64RelatePath, strAts64SettingTextFilePath, btnAts64Add);
        }

        /// <summary>detailmodules.txt から見た、追加するプラグインの相対パスを生成する</summary>
        private void GenerateAtsRelativePath(TextBox tbRelativePath, string settingFilePath, Button btnAdd)
        {
            tbRelativePath.Text = PathUtil.GenerateRelativePath(settingFilePath, strAtsPluginFilePath);
            if (tbRelativePath.Text != "")
            {
                tbRelativePath.Enabled = true;
                btnAdd.Enabled = true;
            }
        }

        private void btnAts32Add_Click(object sender, EventArgs e)
        {
            AddDetailModuleRow(dgvAts32, tbAts32RelatePath.Text, strAts32SettingTextFilePath, btnAts32Reset, btnAts32Save);
        }

        private void btnAts64Add_Click(object sender, EventArgs e)
        {
            AddDetailModuleRow(dgvAts64, tbAts64RelatePath.Text, strAts64SettingTextFilePath, btnAts64Reset, btnAts64Save);
        }

        /// <summary>detailmodules.txt のグリッドにプラグインを1行追加する(重複は追加しない)</summary>
        private static void AddDetailModuleRow(DataGridView dgv, string relativePath, string settingFilePath, Button btnReset, Button btnSave)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow && GridHelper.CellText(row, 2) == relativePath)
                {
                    MessageBox.Show("既に適用されています");
                    return;
                }
            }

            AtsList atsList = new AtsList(relativePath, settingFilePath);
            dgv.Rows.Add(atsList.FileName, atsList.Version, atsList.RerativePath, atsList.AbsolutePath);
            btnReset.Visible = true;
            btnSave.Visible = true;
        }

        #endregion

        #region BVE6 簡易コンバータ

        private void btnDetailModuleSelect_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbDialog = new FolderBrowserDialog())
            {
                fbDialog.Description = "BVE6用(64bit)の detailmodules.txt を保存するフォルダを選択してください";
                if (!string.IsNullOrEmpty(strAts32DetailManagerFilePath))
                {
                    fbDialog.SelectedPath = Path.GetDirectoryName(strAts32DetailManagerFilePath);
                }
                fbDialog.ShowNewFolderButton = true;

                if (fbDialog.ShowDialog() != DialogResult.OK) return;

                if (Path.Combine(fbDialog.SelectedPath, "detailmodules.txt") == strAts32SettingTextFilePath)
                {
                    MessageBox.Show("BVE5用(32bit)のフォルダと同一には出来ません。別のフォルダを選択するか作成してください。");
                    return;
                }

                strAts64DetailManagerFilePath = Path.Combine(fbDialog.SelectedPath, "DetailManager.dll");
                strAts64SettingTextFilePath = Path.Combine(fbDialog.SelectedPath, "detailmodules.txt");
                tbDetailModuleStoreDetail.Text = fbDialog.SelectedPath;
                tbAts64.Text = strAts64DetailManagerFilePath;
                btnDetailModuleStoreClear.Enabled = true;

                if (!File.Exists(strAts64DetailManagerFilePath))
                {
                    if (!File.Exists(strAts64DetailManagerOrgFilePath))
                    {
                        MessageBox.Show("先にBVE6用(64bit)のDetailManager.dllを選択してください。");
                        return;
                    }
                    File.Copy(strAts64DetailManagerOrgFilePath, strAts64DetailManagerFilePath);
                }

                if (AtsPluginChecker(strAts64DetailManagerFilePath, false) == BVE_Version.BVE6)
                {
                    if (!File.Exists(strAts64SettingTextFilePath))
                    {
                        File.WriteAllText(strAts64SettingTextFilePath, "");
                    }
                    tbAts64.Visible = true;
                    tbAts64.Enabled = true;
                    if (strAts32SettingTextFilePath.IndexOf("detailmodules.txt", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        btnBve6Convert.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("既に指定した場所にDetailManager.dllがありますがBVE6用(64bit)ではありません。場所をよく確認した後、手動で削除し、再度選択してください。");
                }
            }
        }

        private void btnDetailManagerBve6Select_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "BVE6 DetailManager(DetailManager.dll)|DetailManager.dll";
                ofd.InitialDirectory = Settings.Default.DetailManager64Path;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                if (AtsPluginChecker(ofd.FileName, false) == BVE_Version.BVE6)
                {
                    tbDetailManagerDetail.Text = ofd.FileName;
                    strAts64DetailManagerOrgFilePath = ofd.FileName;
                    Settings.Default.DetailManager64Path = Path.GetDirectoryName(ofd.FileName);
                }
                else
                {
                    MessageBox.Show("BVE6用(64bit)のDetailManager.dllではありません。選択し直してください。");
                }
            }
        }

        private void btnDetailManagerBve6Clear_Click(object sender, EventArgs e)
        {
            btnBve6Convert.Enabled = false;
            tbDetailManagerDetail.Text = DetailManagerBve6Hint;
            Settings.Default.DetailManager64Path = "";
        }

        private void btnDetailModuleStoreDetailClear_Click(object sender, EventArgs e)
        {
            btnBve6Convert.Enabled = false;
            tbDetailModuleStoreDetail.Text = DetailModuleStoreHint;
            btnDetailModuleStoreClear.Enabled = false;
        }

        /// <summary>
        /// BVE5用車両ファイルを BVE6 対応(Vehicle 2.00, Ats32/Ats64)に書き換える。
        /// 元ファイルは .bak として残す。
        /// </summary>
        private void btnBve6Convert_Click(object sender, EventArgs e)
        {
            string backupPath = strVehicleFilePath + ".bak";
            if (!File.Exists(backupPath))
            {
                File.Move(strVehicleFilePath, backupPath);
            }

            // 手動指定があればそれを優先(ヘッダに shift_jis の記載がない Shift_JIS ファイル対策)
            Encoding enc = GetEncodingOverride(strVehicleFilePath) ?? EncodingDetector.Detect(backupPath, out _);
            bool isShiftJis = EncodingDetector.ModeOf(enc) == EncodingDetector.ModeShiftJis;

            var output = new List<string>();
            foreach (string rawLine in File.ReadAllLines(backupPath, enc))
            {
                string line = rawLine.Trim();
                int eq = line.IndexOf('=');
                string key = eq >= 0 ? line.Substring(0, eq).Trim() : "";

                if (line.IndexOf("BveTs Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // ヘッダ: バージョンを 2.00 に(Shift_JIS の場合はエンコード指定も維持)
                    output.Add("Bvets Vehicle 2.00" + (isShiftJis ? ":shift_jis" : ""));
                }
                else if (!line.StartsWith(";") && !line.StartsWith("#") && string.Equals(key, "Ats", StringComparison.OrdinalIgnoreCase))
                {
                    // Ats = ... → Ats32 / Ats64 の2行に置き換え
                    output.Add("Ats32 = " + PathUtil.GenerateRelativePath(strVehicleFilePath, strAts32DetailManagerFilePath));
                    output.Add("Ats64 = " + PathUtil.GenerateRelativePath(strVehicleFilePath, strAts64DetailManagerFilePath));
                }
                else
                {
                    output.Add(line);
                }
            }
            File.WriteAllText(strVehicleFilePath, string.Join(Environment.NewLine, output), enc);

            Make_NewDetailModulesFile(strAts32SettingTextFilePath, strAts64SettingTextFilePath);
            OpenNewVehicleFile(CurrentVehicleFilePath());
        }

        private void btnBve5Recovery_Click(object sender, EventArgs e)
        {
            string vehicleFile = CurrentVehicleFilePath();
            if (vehicleFile == null || !File.Exists(vehicleFile + ".bak")) return;

            File.Copy(vehicleFile + ".bak", vehicleFile, true);
            File.Delete(vehicleFile + ".bak");
            btnBve5Recovery.Visible = false;
            lblConvert.Text = ConvertHint;
            dgvAts64.Rows.Clear();
            OpenNewVehicleFile(vehicleFile);
        }

        /// <summary>
        /// 32bit用 detailmodules.txt を元に 64bit用 detailmodules.txt を作成する。
        /// 64bit版が見つかったプラグインはそのパスに、見つからないものはコメントアウトする。
        /// </summary>
        private void Make_NewDetailModulesFile(string oldFileName_, string newFileName_)
        {
            Encoding enc = EncodingDetector.Utf8;
            if (oldFileName_.IndexOf("detailmodules.txt", StringComparison.OrdinalIgnoreCase) <= 0)
            {
                File.WriteAllText(newFileName_, "", enc);
                return;
            }

            string[] lines = File.ReadAllLines(oldFileName_);
            for (int i = 0; i < lines.Length; i++)
            {
                string ats32Path = PathUtil.CombineAbs(strAts32SettingTextFilePath, lines[i]);
                string ats64Path = FindMikanGoGo64bitPlugin(lines[i], ats32Path);

                lines[i] = ats64Path != null
                    ? PathUtil.GenerateRelativePath(strAts64SettingTextFilePath, ats64Path)
                    : "#" + PathUtil.GenerateRelativePath(strAts64SettingTextFilePath, ats32Path);
            }
            File.WriteAllLines(newFileName_, lines, enc);
        }

        /// <summary>
        /// mikan-go-go様 GeneralAtsPlugin の 64bit 版(Rock_On\x64\...)があればそのパスを返す
        /// </summary>
        private static string FindMikanGoGo64bitPlugin(string line, string ats32Path)
        {
            if (line.IndexOf(@"\GeneralAtsPlugin\Rock_On", StringComparison.OrdinalIgnoreCase) <= 0) return null;

            string[] keyword = { "Rock_On" };
            string[] arr = ats32Path.Split(keyword, 2, StringSplitOptions.None);
            if (arr.Length < 2) return null;

            string ats64Path = arr[0] + keyword[0] + @"\x64" + arr[1];
            return (File.Exists(ats64Path) && AtsPluginChecker(ats64Path, false) == BVE_Version.BVE6) ? ats64Path : null;
        }

        #endregion
    }
}
