using BveFileExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    /// <summary>
    /// メインフォーム(共通処理・起動・設定)。
    /// 機能ごとの処理は以下の partial ファイルに分割している。
    ///   Form1.Scenario.cs : シナリオファイル・シナリオ一覧
    ///   Form1.Vehicle.cs  : 車両ファイル
    ///   Form1.Map.cs      : マップファイル・サウンド再生
    ///   Form1.Ats.cs      : ATSプラグイン・BVE6コンバータ
    /// </summary>
    public partial class Form1 : Form
    {
        private const string DetailModuleStoreHint = "推奨例:32bit DetailManager.dll格納フォルダ直下または同列に[x64]や[ATS64]フォルダを作成します";
        private const string ConvertHint = "コンバート後、[BVE6 DetailModules]タブで読込状況を確認してください。";

        private Scenario scenario;
        private Vehicle vehicle;
        private Map map;

        private string strVehicleFilePath = "";
        private readonly List<string> listMapFilePath = new List<string>();
        private string strAts32DetailManagerFilePath = "";
        private string strAts32SettingTextFilePath = "";
        private string strAts64DetailManagerFilePath = "";
        private string strAts64SettingTextFilePath = "";
        private string strAts64DetailManagerOrgFilePath = "";
        private string strAtsPluginFilePath = "";

        private int cbxVehicleIndex = 0;
        private int cbxMapIndex = 0;

        private string strBve5Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\mackoy\BveTs5\bvets.exe";
        private string strBve6Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\mackoy\BveTs6\bvets.exe";
        private string strStrviewPath;

        /// <summary>ステータスバー/メッセージボックスに表示するメッセージ(FlushMessagesで表示)</summary>
        private string strDisp = "";

        private TabPage _hiddenTabPage;

        /// <summary>
        /// エンコードコンボボックスをプログラムから変更する間 true。
        /// SelectedIndexChanged による二重読み込みを防ぐ。
        /// </summary>
        private bool _suppressEncEvents = false;

        /// <summary>
        /// ユーザーが手動で指定したエンコード(ファイルパス → Encoding)。
        /// ヘッダへの shift_jis 記載漏れ等に対応する。タブ切替等で読み直しても維持し、Reset でクリアする。
        /// </summary>
        private readonly Dictionary<string, Encoding> _encodingOverrides = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase);

        public Form1()
        {
            InitializeComponent();

            //_hiddenTabPage = tabControlScenario.TabPages[2];
            //tabControlScenario.TabPages.Remove(_hiddenTabPage);

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += " ( Beta ver " + version.ToString() + " )";

            InitVehicleTabs();
            InitScenarioEntryLists();
            BindFileButtons();
            btnPlayStop.Click += BtnPlayStop_Click;
        }

        #region ファイル/フォルダを開くボタン

        /// <summary>
        /// 「開く」「フォルダ」ボタンと、対象ファイルパスの取得方法をまとめて登録する。
        /// パスは押された時点で評価するため、未読込(null)でも落ちない。
        /// </summary>
        private void BindFileButtons()
        {
            // シナリオ
            BindOpen(btnOpenScenarioFile, () => scenario?.FilePath);
            btnOpenScenarioDirectory.Click += (s, e) => OpenFolder(tbScenarioDirectory.Text);

            // 車両
            BindOpen(btnOpenVehicleFile, CurrentVehicleFilePath);
            BindFolder(btnOpenVehicleDirectory, CurrentVehicleFilePath);
            BindOpenAndFolder(btnPerfoemanceCurveOpen, btnPerfoemanceCurveDirectory, () => vehicle?.PerformanceCurve?.FilePath);
            BindOpenAndFolder(btnParametersOpen, btnParametersDirectory, () => vehicle?.Parameters?.FilePath);
            BindOpenAndFolder(btnPanelOpen, btnPanelDirectory, () => vehicle?.Panel?.FilePath);
            BindOpenAndFolder(btnSoundOpen, btnSoundDirectory, () => vehicle?.Sound?.FilePath);
            BindOpenAndFolder(btnMotorNoiseOpen, btnMotorNoiseDirectory, () => vehicle?.MotorNoise?.FilePath);

            // ATSプラグイン
            BindOpen(btnAts32Open, () => strAts32SettingTextFilePath);
            BindFolder(btnAts32OpenDirectory, () => vehicle?.Ats32?.FilePath);
            BindOpen(btnAts32Open2, () => tbAts32DetailModules.Text);
            BindOpen(btnAts64Open, () => strAts64SettingTextFilePath);
            BindFolder(btnAts64OpenDirectory, () => vehicle?.Ats64?.FilePath);
            BindOpen(btnAts64Open2, () => tbAts64DetailModules.Text);

            // マップ
            BindOpenAndFolder(btnMapOpen, btnMapDirectory, () => map?.FilePath);
            BindOpenAndFolder(btnStructureOpen, btnStructureDirectory, () => map?.Structure?.FilePathAbs);
            BindOpenAndFolder(btnStationOpen, btnStationDirectory, () => map?.Station?.FilePathAbs);
            BindOpenAndFolder(btnSignalOpen, btnSignalDirectory, () => map?.Signal?.FilePathAbs);
            BindOpenAndFolder(btnSoundListOpen, btnSoundListDirectory, () => map?.SoundList?.FilePathAbs);
            BindOpenAndFolder(btnSound3DListOpen, btnSound3DListDirectory, () => map?.Sound3DList?.FilePathAbs);
            BindOpenAndFolder(btnTrainOpen, btnTrainDirectory, () => cbxTrain.Text);
        }

        private void BindOpen(Button button, Func<string> getPath)
        {
            button.Click += (s, e) => OpenFile(getPath());
        }

        private void BindFolder(Button button, Func<string> getPath)
        {
            button.Click += (s, e) => OpenFolderOf(getPath());
        }

        private void BindOpenAndFolder(Button openButton, Button folderButton, Func<string> getPath)
        {
            BindOpen(openButton, getPath);
            BindFolder(folderButton, getPath);
        }

        private string CurrentVehicleFilePath()
        {
            if (scenario == null || cbxVehicleIndex < 0 || cbxVehicleIndex >= scenario.VehicleFilesAbs.Count) return null;
            return scenario.VehicleFilesAbs[cbxVehicleIndex];
        }

        /// <summary>ファイルを関連付けアプリで開く</summary>
        private void OpenFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                Process.Start(filePath);
            }
            else if (cbMessageDisp.Checked)
            {
                MessageBox.Show("指定先のファイルが見つかりません");
            }
        }

        /// <summary>ファイルのあるフォルダを開く</summary>
        private void OpenFolderOf(string filePath)
        {
            string dir = null;
            try
            {
                if (!string.IsNullOrEmpty(filePath)) dir = Path.GetDirectoryName(filePath);
            }
            catch (ArgumentException) { }
            OpenFolder(dir);
        }

        /// <summary>フォルダを開く</summary>
        private void OpenFolder(string directory)
        {
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Process.Start(directory);
            }
            else if (cbMessageDisp.Checked)
            {
                MessageBox.Show("指定先のフォルダが見つかりません");
            }
        }

        #endregion

        #region 共通ヘルパー

        /// <summary>溜まったメッセージをステータスバー(と設定によりメッセージボックス)に表示する</summary>
        private void FlushMessages()
        {
            if (strDisp == "") return;

            if (cbMessageDisp.Checked)
            {
                MessageBox.Show(strDisp);
            }
            tsslDisp.Text = strDisp.Replace("\n", " ").Trim();
            strDisp = "";
        }

        /// <summary>SelectedIndexChanged を発生させずにエンコードコンボボックスを変更する</summary>
        private void SetEncIndex(ComboBox cbx, int index)
        {
            bool prev = _suppressEncEvents;
            _suppressEncEvents = true;
            try
            {
                cbx.SelectedIndex = index;
            }
            finally
            {
                _suppressEncEvents = prev;
            }
        }

        /// <summary>ファイルのエンコードを判定し、コンボボックスに表示する</summary>
        private Encoding DetectEncoding(string filePath, ComboBox cbx)
        {
            EncodingDetector.HeaderInfo header = EncodingDetector.Analyze(filePath);
            SetEncIndex(cbx, header.Mode);
            if (header.Warning != "")
            {
                tsslDisp.Text = Path.GetFileName(filePath) + ": " + header.Warning;
            }
            return header.Encoding;
        }

        /// <summary>手動指定されたエンコード(なければ null = ヘッダから自動判定)</summary>
        private Encoding GetEncodingOverride(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            return _encodingOverrides.TryGetValue(filePath, out Encoding enc) ? enc : null;
        }

        /// <summary>ユーザーがエンコードを選び直したときに読み込み直すべきか</summary>
        private bool IsUserEncodingChange(ComboBox cbx)
        {
            return !_suppressEncEvents && cbx.SelectedIndex > 0;
        }

        /// <summary>
        /// 参照ファイルの状態をテキストボックス・ボタンに反映する。
        /// </summary>
        /// <returns>1:存在 / -1:見つからない / 0:未宣言</returns>
        private int ShowPathStatus(TextBox tb, Button btnFile, Button btnDirectory, IFileReference c, out string filePath)
        {
            if (c == null)
            {
                tb.Text = "Not Found : Not Declared";
                tb.BackColor = Color.LightYellow;
                filePath = "";
                return -1;
            }

            filePath = c.AbsolutePath;
            if (c.Ret == 1)
            {
                tb.Text = c.AbsolutePath;
                btnFile.Enabled = true;
                btnDirectory.Enabled = true;
                tb.BackColor = SystemColors.Window;
            }
            else if (c.Ret == -1)
            {
                tb.Text = "Not Found or Supported : " + c.AbsolutePath;
                tb.BackColor = Color.LightYellow;
            }
            else
            {
                tb.Text = "Not Found : Not Declared";
                tb.BackColor = Color.LightYellow;
            }
            return c.Ret;
        }

        /// <summary>ファイル読み込み中の想定内エラーを、落とさずステータスバーに表示する</summary>
        private void TryLoad(string filePath, Action load)
        {
            try
            {
                load();
            }
            catch (IOException ex) { ShowLoadError(filePath, ex); }
            catch (UnauthorizedAccessException ex) { ShowLoadError(filePath, ex); }
            catch (Microsoft.VisualBasic.FileIO.MalformedLineException ex) { ShowLoadError(filePath, ex); }
            catch (ArgumentException ex) { ShowLoadError(filePath, ex); }
        }

        private void ShowLoadError(string filePath, Exception ex)
        {
            tsslDisp.Text = "読込エラー: " + filePath + " (" + ex.Message + ")";
        }

        #endregion

        #region リセット

        private void Reset()
        {
            map = null;
            vehicle = null;
            _encodingOverrides.Clear();
            foreach (ScenarioEntryList list in EntryLists)
            {
                list.Dirty = false;
                list.Info.Text = "";
            }
            cbxScenarioEnc.Enabled = false;
            cbxVehicleEnc.Enabled = false;
            cbxMapEnc.Enabled = false;

            strVehicleFilePath = "";
            strAts32DetailManagerFilePath = "";
            strAts32SettingTextFilePath = "";
            strAts64DetailManagerFilePath = "";
            strAts64SettingTextFilePath = "";

            // 無効化するボタン
            foreach (Control c in new Control[] {
                btnOpenScenarioFile, btnOpenVehicleFile, btnOpenVehicleDirectory,
                btnBootBVE5, btnBve5BootChooseVehicle, btnBve5BootChooseMap,
                btnBootBVE6, btnBve6BootChooseVehicle, btnBve6BootChooseMap,
                btnPerfoemanceCurveOpen, btnPerfoemanceCurveDirectory,
                btnParametersOpen, btnParametersDirectory,
                btnPanelOpen, btnPanelDirectory,
                btnSoundOpen, btnSoundDirectory,
                btnMotorNoiseOpen, btnMotorNoiseDirectory,
                btnAts32Open, btnAts32Open2, btnAts32OpenDirectory, btnAts32Check,
                btnAts64Open, btnAts64Open2, btnAts64OpenDirectory, btnAts64Check,
                btnAts32RelatePathGen, btnAts32Add, btnAts64RelatePathGen, btnAts64Add,
                btnMapOpen, btnMapDirectory,
                btnStructureOpen, btnStructureDirectory,
                btnStationOpen, btnStationDirectory,
                btnSignalOpen, btnSignalDirectory,
                btnSoundListOpen, btnSoundListDirectory,
                btnSound3DListOpen, btnSound3DListDirectory,
                btnTrainOpen, btnTrainDirectory,
                btnBve6Convert, btnDetailModuleStoreClear,
                btnVehicleAdd, btnVehicleRemove, btnVehicleSaveAs,
                btnRouteAdd, btnRouteRemove, btnRouteSaveAs })
            {
                c.Enabled = false;
            }

            // 非表示にするボタン
            foreach (Control c in new Control[] {
                btnAts32Reset, btnAts32Save, btnAts32Recovery,
                btnAts64Reset, btnAts64Save, btnAts64Recovery,
                btnDetailModuleStoreClear, btnBve5Recovery })
            {
                c.Visible = false;
            }

            // テキストのクリア
            cbxVehicle.Items.Clear();
            cbxMapFilePath.Items.Clear();
            listMapFilePath.Clear();
            cbxTrain.Items.Clear();
            foreach (Control c in new Control[] {
                tbSeinarioFileName,
                tbPerfoemanceCurve, tbParameters, tbPanel, tbSound, tbMotorNoise,
                tbAts32, tbAts32DetailModules, tbAts32RelatePath,
                tbAts64, tbAts64DetailModules, tbAts64RelatePath,
                cbxVehicle, cbxMapFilePath,
                tbStructure, tbStation, tbSignal, tbSoundList, tbSound3DList, cbxTrain })
            {
                c.Text = "";
            }
            cbxTrain.BackColor = SystemColors.Control;
            btnOpenVehicleFile.BackColor = SystemColors.Control;
            pbVehicleImage.Image = null;

            lblConvert.Text = ConvertHint;
            tbDetailModuleStoreDetail.Text = DetailModuleStoreHint;

            // グリッドのクリア
            dgvScenario.Rows.Clear();
            dgvAts32.Rows.Clear();
            dgvAts64.Rows.Clear();
            foreach (DataGridView dgv in new[] {
                dgvVehicle, dgvPerformanceCurve, dgvParameters, dgvPanel, dgvSound, dgvMotorNoise,
                dgvRoute, dgvIncludeList, dgvStructure, dgvStation, dgvSignal,
                dgvSoundList, dgvSound3DList, dgvTrain, dgvTrainFileList })
            {
                dgv.DataSource = null;
                dgv.Columns.Clear();
            }

            // エンコード表示のクリア
            foreach (ComboBox cbx in new[] {
                cbxScenarioEnc, cbxVehicleEnc, cbxMapEnc,
                cbxPerformanceCurveEnc, cbxParametersEnc, cbxPanelEnc, cbxSoundEnc, cbxMotorNoiseEnc,
                cbxStructureEnc, cbxSignalEnc, cbxStationEnc, cbxSoundListEnc, cbxSound3DListEnc, cbxTrainFileEnc })
            {
                SetEncIndex(cbx, 0);
            }

            // タブの選択位置は保持する(読込後に選択中タブの内容を読み直す)
        }

        #endregion

        #region フォームイベント

        private void Form1_Load(object sender, EventArgs e)
        {
            dgvFiles.Columns.Add("ListFileName", "ファイル名");
            dgvFiles.Columns[0].MinimumWidth = 220;
            dgvFiles.Columns[0].HeaderCell.ToolTipText = "ファイル名をクリックで展開、ダブルクリックで開きます";
            dgvFiles.Columns.Add("ListFileAuthor", "作　者");
            dgvFiles.Columns[1].MinimumWidth = 100;
            AddListCountColumn("ListVehicleCount", "V", 30, "車両ファイル数");
            AddListCountColumn("ListMapCount", "M", 30, "路線データ数");
            AddListCountColumn("ListVehicleVersion", "V ver", 60, "車両データバージョン(1定義目)");
            AddListCountColumn("ListMapVersion", "M ver", 60, "路線データバージョン(1定義目)");

            ToolTip tt = new ToolTip();
            tt.SetToolTip(btnListBVE5, "BVE5対応のシナリオファイルを強調します");
            tt.SetToolTip(btnListBVE6, "BVE6対応のシナリオファイルを強調します");
            tt.SetToolTip(btnListNoVehicle, "車両定義のないシナリオファイルを強調します");
            tt.SetToolTip(btnListNoMap, "マップ定義のないシナリオファイルを強調します");
        }

        private void AddListCountColumn(string name, string header, int width, string toolTip)
        {
            int i = dgvFiles.Columns.Add(name, header);
            dgvFiles.Columns[i].Width = width;
            dgvFiles.Columns[i].HeaderCell.ToolTipText = toolTip;
            dgvFiles.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            MessageBox.Show("※動作保証なし、データの破損等いかなる責任も負いかねます※\r\n\r\n※車両や路線データの改造に該当すると考えられますので、自己責任かつ個人使用の範囲内でお願いします※");
            cbMessageDisp.Checked = Settings.Default.cbMessage;
            tsslDisp.Text = "";
            if (Settings.Default.strBve5Path != "")
            {
                strBve5Path = Settings.Default.strBve5Path;
            }
            if (Settings.Default.strBve6Path != "")
            {
                strBve6Path = Settings.Default.strBve6Path;
            }
            string dm64 = Settings.Default.DetailManager64Path + @"\DetailManager.dll";
            if (File.Exists(dm64))
            {
                tbDetailManagerDetail.Text = dm64;
                strAts64DetailManagerOrgFilePath = dm64;

                strAtsPluginFilePath = Settings.Default.AtsPluginFilePath;
                tbAtsPluginFile.Text = Settings.Default.AtsPluginFilePath;
            }

            if (File.Exists(Settings.Default.strStrviewPath))
            {
                strStrviewPath = Settings.Default.strStrviewPath;
                btnStrview5.Enabled = true;
            }

            if (!string.IsNullOrEmpty(Settings.Default.RouteFileDirectory) && Directory.Exists(Settings.Default.RouteFileDirectory))
            {
                tbScenarioDirectory.Text = Settings.Default.RouteFileDirectory;
                OpenSenaroDirectory(Settings.Default.RouteFileDirectory, true);
            }
            else
            {
                string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\BveTs\Scenarios";
                if (Directory.Exists(tempDir))
                {
                    tbScenarioDirectory.Text = tempDir;
                    OpenSenaroDirectory(tempDir, true);
                    Settings.Default.RouteFileDirectory = tempDir;
                    Settings.Default.Save();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmDiscardEntryListChanges())
            {
                e.Cancel = true;
                return;
            }

            StopSound();
            TempFileChecker();
            Settings.Default.Save();
        }

        /// <summary>「選択車両/マップで起動」で作成した一時シナリオファイルを削除する</summary>
        private void TempFileChecker()
        {
            if (scenario?.FilePath == null) return;

            string tmp = TempScenarioPath();
            if (File.Exists(tmp))
            {
                File.Delete(tmp);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl + T で隠しタブ(ATSプラグイン関連)を表示
            if (e.Control && e.KeyCode == Keys.T)
            {
                if (_hiddenTabPage != null)
                {
                    tabControlScenario.TabPages.Add(_hiddenTabPage);
                    _hiddenTabPage = null;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;

                gbxBve6Converter.Visible = true;
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "【つかいかた(車両ファイル確認)】\r\n" +
                "1.シナリオが格納されたディレクトリを選択します。(デフォルトではUser\\Documents\\BveTs\\Scenarios\\となります)\r\n" +
                "2.各ファイル関係タブを選択し、リンクを確認します。ファイル名をダブルクリックすることで開きます。(全てではありません)\r\n" +
                "3.右上のBVE起動ボタンで選択した路線や選択した車両でプレイすることができます。\r\n" +
                "4.左のリストビュー上の凡例ボタンをクリックすることでBVE5/6対応および車両/マップデータの種類分けができます\r\n" +
                "\r\n" +
                "【できないこと】\r\n" +
                "1.DetailManager非対応のプラグインの適用\r\n" +
                "2.include形式ファイル、パス内変数形式\r\n" +
                "3.他列車ファイル、ほか\r\n" +
                "\r\n" +
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします。また、本ツールで「改造」したデータのアップロードは禁止です※\r\n" +
                "\r\n", "使い方");
        }

        private void cbMessage_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.cbMessage = cbMessageDisp.Checked;
            Settings.Default.Save();
        }

        #endregion

        #region BVE起動・パス設定

        private void btnBootBVE5_Click(object sender, EventArgs e) => BootBve(strBve5Path, scenario?.FilePath, "BVE5");

        private void btnBootBVE6_Click(object sender, EventArgs e) => BootBve(strBve6Path, scenario?.FilePath, "BVE6");

        private void btnBve5BootChooseVehicle_Click(object sender, EventArgs e) => BootBve(strBve5Path, CreateTempScenario("Vehicle", cbxVehicle.Text), "BVE5");

        private void btnBve6BootChooseVehicle_Click(object sender, EventArgs e) => BootBve(strBve6Path, CreateTempScenario("Vehicle", cbxVehicle.Text), "BVE6");

        private void btnBve5BootChooseMap_Click(object sender, EventArgs e) => BootBve(strBve5Path, CreateTempScenario("Route", CurrentMapFilePath()), "BVE5");

        private void btnBve6BootChooseMap_Click(object sender, EventArgs e) => BootBve(strBve6Path, CreateTempScenario("Route", CurrentMapFilePath()), "BVE6");

        private void BootBve(string exePath, string scenarioFile, string label)
        {
            if (string.IsNullOrEmpty(scenarioFile)) return;

            if (File.Exists(exePath))
            {
                Process.Start(exePath, "\"" + scenarioFile + "\"");
            }
            else
            {
                MessageBox.Show(label + "が見つかりません。" + label + "のパスを設定してください。");
            }
        }

        private void btnBve5PathSetting_Click(object sender, EventArgs e)
        {
            string path = SelectBveExe(strBve5Path, Settings.Default.strBve5Path, Environment.SpecialFolder.ProgramFilesX86, "BVE5", BVE_Version.BVE5);
            if (path != null)
            {
                strBve5Path = path;
                Settings.Default.strBve5Path = path;
            }
        }

        private void btnBve6PathSetting_Click(object sender, EventArgs e)
        {
            string path = SelectBveExe(strBve6Path, Settings.Default.strBve6Path, Environment.SpecialFolder.ProgramFiles, "BVE6", BVE_Version.BVE6);
            if (path != null)
            {
                strBve6Path = path;
                Settings.Default.strBve6Path = path;
            }
        }

        /// <summary>
        /// bvets.exe を選択させ、ビット数が合っていればそのパスを返す(キャンセル・不一致は null)
        /// </summary>
        private string SelectBveExe(string currentPath, string savedPath, Environment.SpecialFolder defaultFolder, string label, BVE_Version expected)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "bvets.exe";
                ofd.InitialDirectory = (File.Exists(currentPath) && File.Exists(savedPath))
                    ? Path.GetDirectoryName(savedPath)
                    : Environment.GetFolderPath(defaultFolder);
                ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
                ofd.Title = label + "の実行ファイルを選択してください";

                if (ofd.ShowDialog() != DialogResult.OK) return null;

                if (AtsPlugin.Checker(ofd.FileName) == expected)
                {
                    return ofd.FileName;
                }
                MessageBox.Show(label + "の実行ファイルではありません");
                return null;
            }
        }

        private void btnStrviewPathSetting_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "strview5.exe";
                if (File.Exists(strStrviewPath) && File.Exists(Settings.Default.strStrviewPath))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(Settings.Default.strStrviewPath);
                }
                ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
                ofd.Title = "ストラクチャビューワを選択してください";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    strStrviewPath = ofd.FileName;
                    btnStrview5.Enabled = true;
                    Settings.Default.strStrviewPath = strStrviewPath;
                    Settings.Default.Save();
                }
            }
        }

        private void btnStrview5_Click(object sender, EventArgs e)
        {
            if (File.Exists(strStrviewPath))
            {
                Process.Start(strStrviewPath);
            }
        }

        #endregion
    }
}
