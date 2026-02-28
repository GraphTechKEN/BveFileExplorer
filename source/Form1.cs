using BveFileExplorer.Properties;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BveFileExplorer
{
    public enum BVE_Version
    {
        NotFound,
        BVE5,
        BVE6,
        Null,
        Comment,
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            _hiddenTabPage = tabControlScenario.TabPages[2];
            tabControlScenario.TabPages.Remove(_hiddenTabPage);

            // 1. アセンブリのバージョンを取得
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text += " ( Beta ver " + version.ToString() + " )";

        }

        private string strVehicleFilePath = "";
        private readonly List<string> listMapFilePath = new List<string>();
        private string strAts32DetailManagerFilePath = "";
        private string strAts32SettingTextFilePath = "";
        private string strAtsPluginFilePath = "";
        private string strMapFilePath = "";
        private string strAts64DetailManagerFilePath = "";
        private string strAts64SettingTextFilePath = "";

        private int cbxVehicleIndex = 0;
        private int cbxMapIndex = 0;
        private string strBve5Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\mackoy\BveTs5\bvets.exe";
        private string strBve6Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\mackoy\BveTs6\bvets.exe";

        private string strDisp = "";
        private Scenario scenario;
        private Map map;

        private AtsPlugin ats32Plugin;
        private AtsPlugin ats64Plugin;

        private DataTable dt;

        private TabPage _hiddenTabPage;

        private bool IsDetailmanager32 = false;
        private bool IsDetailmanager64 = false;

        private Vehicle vehicle;
        private void btnOpenScenario_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "路線ファイル(*.txt)|*.txt";
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                ofd.InitialDirectory = Settings.Default.RouteFileDirectory;

                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    TempFileChecker();
                    Reset();

                    Settings.Default.RouteFileDirectory = Path.GetDirectoryName(ofd.FileName);
                    Settings.Default.Save();


                    //OKボタンがクリックされたとき、選択されたファイルを読み取り専用で開く
                    scenario = new Scenario(ofd.FileName);
                    OpenScenario();
                }
            }
        }

        /// <summary>
        /// 車両ファイルのリンク切れを黄色で表示するメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxVehicle_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // 背景の描画（選択されているか否かで色を変えることも可能）
            e.DrawBackground();

            // アイテムの背景色を個別に設定する場合の例
            Brush backBrush = Brushes.White;
            if (scenario.VehicleFilesCount > 0) {
                for (int i = 0; i < scenario.VehicleFilesCount; i++) {
                    if (e.Index ==i && !scenario.VehicleFilesExists[i]) backBrush = Brushes.Yellow; // 1番目のアイテム
                }
            }
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            // 文字の描画
            string text = cbxVehicle.Items[e.Index].ToString();
            e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds);

            // フォーカスを描画
            e.DrawFocusRectangle();
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
            cbxScenarioEnc.SelectedIndex = scenario.encMode;

            //内容を読み込み、表示する
            bool flgErrVehicle = false;
            bool flgErrMap = false;

            if (scenario.ImagePath != null && scenario.ImagePath != "")
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
            dgvScenario[1,4].Style.WrapMode = DataGridViewTriState.True;
            dgvScenario.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;

            //車両ファイルが指定されている場合の処理
            if (scenario.VehicleFilesExistsCount > 0 && !flgErrVehicle)
            {
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

                //車両データ(Vehicle)

                // DataGridViewにデータをセット
                dgvVehicle.DataSource = scenario.VehicleFilesList.Select(x => new { FilePath = x.Item1, Ratio = x.Item2 }).ToList();

                dgvVehicle.Columns[0].HeaderCell.ToolTipText = "相対パス";
                dgvVehicle.Columns[1].HeaderCell.ToolTipText = "出現割合(未指定の場合は1)";
                dgvVehicle.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


                //ソート禁止
                foreach (DataGridViewColumn c in dgvVehicle.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                //色付け
                for (int i = 0; i < dgvVehicle.Rows.Count; i++)
                {
                    if (!scenario.VehicleFilesExists[i])
                    {
                        dgvVehicle.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }

                }

            }
            //車両ファイルが空欄のとき
            else
            {
                btnBootBVE5.Enabled = false;
                btnBootBVE6.Enabled = false;
                btnBve5BootChooseVehicle.Enabled = false;
                btnBve6BootChooseVehicle.Enabled = false;
                cbxVehicle.Text = "Not defined";
                cbxVehicle.BackColor = Color.LightYellow;
                strDisp += "車両ファイルが指定されていません。\n";
            }

            //マップファイルが指定されている場合の処理
            if (scenario.MapFilesCount > 0 && !flgErrMap)
            {
                strMapFilePath = scenario.MapFilesAbs[0];

                for (int i = 0; i < scenario.MapFilesCount; i++)
                {
                    this.listMapFilePath.Add(scenario.MapFilesAbs[i]);
                }

                bool IsFileExists = false;
                for (int i = 0; i < scenario.MapFilesCount; i++)
                {
                    IsFileExists |= File.Exists(this.listMapFilePath[i]);
                }
                if (IsFileExists)
                {
                    if (scenario.MapFilesCount > 1)
                    {
                        btnBve5BootChooseMap.Enabled = btnBootBVE5.Enabled;
                        btnBve6BootChooseMap.Enabled = btnBootBVE6.Enabled;
                        strDisp += "マップファイルが複数あります。データ数：" + scenario.MapFilesCount + "\n";
                        //cbxMapFilePath.SelectedIndex = 0;
                    }
                    else
                    {
                        btnBve5BootChooseMap.Enabled = false;
                        btnBve6BootChooseMap.Enabled = false;
                    }
                    for (int i = 0; i < this.listMapFilePath.Count; i++)
                    {
                        cbxMapFilePath.Items.Add(this.listMapFilePath[i]);
                    }
                    cbxMapFilePath.BackColor = SystemColors.Window;

                    OpenNewMapFile(this.listMapFilePath[0]);

                }

                //マップファイルが見つからない場合の処理
                else
                {
                    btnBootBVE5.Enabled = false;
                    btnBootBVE6.Enabled = false;
                    btnBve5BootChooseMap.Enabled = false;
                    btnBve6BootChooseMap.Enabled = false;
                    for (int i = 0; i < scenario.MapFilesCount; i++)
                    {
                        this.listMapFilePath.Add("Not found or supported : " + scenario.MapFilesAbs[i]);
                    }
                    cbxMapFilePath.Text = "Not found or supported : " + scenario.MapFilesAbs[0];
                    cbxMapFilePath.BackColor = Color.LightYellow;
                    strDisp += "マップファイルが見つかりません。\n";
                }

                //路線車両データ(Route)

                // DataGridViewにデータをセット
                dgvRoute.DataSource = scenario.MapFilesList.Select(x => new { FilePath = x.Item1, Ratio = x.Item2 }).ToList();

                dgvRoute.Columns[0].HeaderCell.ToolTipText = "相対パス";
                dgvRoute.Columns[1].HeaderCell.ToolTipText = "出現割合(未指定の場合は1)";
                dgvRoute.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            //マップファイルが空欄のとき
            else
            {
                btnBootBVE5.Enabled = false;
                btnBootBVE6.Enabled = false;
                btnBve5BootChooseMap.Enabled = false;
                btnBve6BootChooseMap.Enabled = false;
                cbxMapFilePath.Text = "Not defined";
                cbxMapFilePath.BackColor = Color.LightYellow;
                strDisp += "マップファイルが指定されていません。\n";
            }
        }

        private void OpenNewMapFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                btnMapOpen.Enabled = true;
                btnMapDirectory.Enabled = true;
                int error = 0;
                cbxMapFilePath.Text = filePath;
                cbxTrain.Items.Clear();
                List<string> listTrain = new List<string>();

                map = new Map(filePath);

                error += (PathControl_Map(ref tbStructure, ref btnStructureOpen, ref btnStructureDirectory, map.Structure, out _) <= 0) ? 1 : 0;
                error += (PathControl_Map(ref tbStation, ref btnStationOpen, ref btnStationDirectory, map.Station, out _) <= 0) ? 1 : 0;
                error += (PathControl_Map(ref tbSignal, ref btnSignalOpen, ref btnSignalDirectory, map.Signal, out _) <= 0) ? 1 : 0;
                error += (PathControl_Map(ref tbSoundList, ref btnSoundListOpen, ref btnSoundListDirectory, map.SoundList, out _) <= 0) ? 1 : 0;
                error += (PathControl_Map(ref tbSound3DList, ref btnSound3DListOpen, ref btnSound3DListDirectory, map.Sound3DList, out _) <= 0) ? 1 : 0;
                error += (PathControl_Map(ref cbxTrain, ref btnTrainOpen, ref btnTrainDirectory, map.Train, out listTrain) <= 0) ? 1 : 0;


            }
            else
            {
                strDisp += "マップファイルが指定されていません\n";
            }
            if (strDisp != "")
            {
                if (cbMessageDisp.Checked)
                {
                    MessageBox.Show(strDisp);
                }
                tsslDisp.Text = strDisp.Replace("\n", " ").Trim();
                strDisp = "";
            }

        }

        private void OpenNewVehicleFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                btnOpenVehicleFile.Enabled = true;
                btnOpenVehicleDirectory.Enabled = true;
                btnOpenVehicleFile.Enabled = true;

                vehicle = new Vehicle(filePath);

                int error = 0;

                strVehicleFilePath = filePath;

                if (vehicle.FileVersion < 2.00)
                {
                    lblVehicleVer.Text = "Vehicle File Ver : " + vehicle.FileVersion.ToString("0.00") + " BVE5 専用車両ファイル";
                    lblAts32.Text = "Ats";
                    btnAts64Check.Visible = false;
                    btnAts64Open.Visible = false;
                    btnAts64Open2.Visible = false;
                    btnAts64Reset.Visible = false;
                    btnAts64OpenDirectory.Visible = false;
                    tbAts64.Visible = false;
                    //gbxBve6.Visible = false;
                    lblAts64.Visible = false;
                    label9.Visible = false;
                    btnAts64RelatePathGen.Visible = false;
                    btnAts64Add.Visible = false;
                    tbAts64RelatePath.Visible = false;

                    //BVE6コンバータ
                    lblDetailModuleStore.Visible = true;
                    tbDetailModuleStoreDetail.Visible = true;
                    tbDetailModuleStoreDetail.Enabled = true;
                    btnDetailModuleSelect.Visible = true;
                    btnDetailManagerBve6Clear.Visible = true;
                    btnDetailModuleStoreClear.Visible = true;
                    lblDetailManager.Visible = true;
                    tbDetailManagerDetail.Visible = true;
                    btnDetailManagerBve6Select.Visible = true;
                    btnDetailManagerBve6Clear.Visible = true;
                    btnBve6Convert.Visible = true;
                    if (strAts64SettingTextFilePath == "")
                    {
                        btnBve6Convert.Enabled = false;
                    }
                    btnBve5Recovery.Visible = false;


                }
                else
                {
                    lblVehicleVer.Text = "Vehicle File Ver : " + vehicle.FileVersion.ToString("0.00") + " BVE6 対応車両ファイル";
                    lblAts32.Text = "Ats32";
                    //BVE6コンバータ
                    lblDetailModuleStore.Visible = false;
                    tbDetailModuleStoreDetail.Visible = false;
                    btnDetailModuleSelect.Visible = false;
                    lblDetailManager.Visible = false;
                    tbDetailManagerDetail.Visible = false;
                    btnDetailManagerBve6Select.Visible = false;
                    btnDetailManagerBve6Clear.Visible = false;
                    btnBve6Convert.Visible = false;
                    btnDetailModuleStoreClear.Visible = false;
                    lblConvert.Text = "既にBVE6に対応している車両かコンバート済みです。";
                    if (File.Exists(scenario.VehicleFilesAbs[cbxVehicleIndex] + @".bak"))
                    {
                        btnBve5Recovery.Visible = true;
                        btnBve5Recovery.Enabled = true;
                    }

                }

                int iRet = PathControl_Vehicle(ref tbAts32, ref btnAts32Open, ref btnAts32Open, vehicle.Ats32, out strAts32DetailManagerFilePath);
                btnAts32Open2.Enabled = btnAts32Open.Enabled;
                if (vehicle.Ats32 != null)
                {
                    if (vehicle.Ats32.FilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        //strDisp += "BVE5用(32bit)ATSプラグイン(DetailManager)が見つかりました\n";
                        strAts32SettingTextFilePath = PathCombineAbs(vehicle.Ats32.FilePath,"detailmodules.txt");

                        tbAts32DetailModules.Text = strAts32SettingTextFilePath;

                        OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.BVE5);



                        IsDetailmanager32 = true;
                        btnAts32Open.Enabled = true;
                        btnAts32Open2.Enabled = true;
                    }
                    else
                    {
                        strDisp += "BVE5用(32bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n";
                        IsDetailmanager32 = false;
                        btnAts32Open.Enabled = false;
                        btnAts32Open2.Enabled = false;
                    }
                }
                if (iRet <= 0)
                {
                    error++;
                    btnAts32Check.Enabled = false;
                }
                else
                {
                    btnAts32OpenDirectory.Enabled = true;
                    btnAts32Check.Enabled = true;
                }
                error += (PathControl_Vehicle(ref tbPerfoemanceCurve, ref btnPerfoemanceCurveOpen, ref btnPerfoemanceCurveDirectory, vehicle.PerformanceCurve, out _) <= 0) ? 1 : 0;
                error += (PathControl_Vehicle(ref tbParameters, ref btnParametersOpen, ref btnParametersDirectory, vehicle.Parameters, out _) <= 0) ? 1 : 0;
                error += (PathControl_Vehicle(ref tbPanel, ref btnPanelOpen, ref btnPanelDirectory, vehicle.Panel, out _) <= 0) ? 1 : 0;
                error += (PathControl_Vehicle(ref tbSound, ref btnSoundOpen, ref btnSoundDirectory, vehicle.Sound, out _) <= 0) ? 1 : 0;
                error += (PathControl_Vehicle(ref tbMotorNoise, ref btnMotorNoiseOpen, ref btnMotorNoiseDirectory, vehicle.MotorNoise, out _) <= 0) ? 1 : 0;

                //ファイルチェック
                int ret = PathControl_Vehicle(ref tbAts64, ref btnAts64Open, ref btnAts64Open, vehicle.Ats64, out strAts64DetailManagerFilePath);
                btnAts64Open2.Enabled = btnAts64Open.Enabled;
                if (ret <= 0)
                {
                    error++;
                    btnBootBVE6.Enabled = false;
                    btnAts64Open.Enabled = false;
                    btnAts64Open2.Enabled = false;
                    tbAts64DetailModules.Enabled = false;
                    btnAts64OpenDirectory.Enabled = false;
                    btnAts64Check.Enabled = false;

                    btnAts64Check.Visible = false;
                    btnAts64Open.Visible = false;
                    btnAts64Open2.Visible = false;
                    tbAts64DetailModules.Visible = false;
                    btnAts64OpenDirectory.Visible = false;
                    tbAts64.Visible = false;
                    //gbxBve6.Visible = false;
                    lblAts64.Visible = false;
                }
                else if (ret >= 1)
                {
                    btnBootBVE6.Enabled = true;
                    btnAts64Check.Enabled = true;

                    btnAts64Check.Visible = true;
                    btnAts64Open.Visible = true;
                    btnAts64Open2.Visible = true;
                    tbAts64DetailModules.Visible = true;
                    btnAts64OpenDirectory.Visible = true;
                    btnAts64OpenDirectory.Enabled = true;
                    tbAts64.Visible = true;
                    //gbxBve6.Visible = true;
                    lblAts64.Visible = true;
                    label9.Visible = true;
                    btnAts64RelatePathGen.Visible = true;
                    btnAts64Add.Visible = true;
                    tbAts64RelatePath.Visible = true;

                    if (vehicle.FileVersion >= 2.0)
                    {
                        //AtsBridgeのとき
                        if (strAts64DetailManagerFilePath.Contains("_.dll"))
                        {
                            //strDisp += "bve-plugin-bridgeによって変換されています\n";
                            strAts64SettingTextFilePath = "bve-plugin-bridgeによって変換されています";
                            tbAts64DetailModules.Text = "bve-plugin-bridgeによって変換されています";
                            
                            IsDetailmanager64 = false;
                            btnAts64Open.Enabled = false;
                            btnAts64Open2.Enabled = false;
                            tbAts64DetailModules.Enabled = false;

                        }
                        //DetailManagerのとき
                        else if (strAts64DetailManagerFilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            //strDisp += "BVE6用(64bit)ATSプラグイン(DetailManager)が見つかりました\n";
                            strAts64SettingTextFilePath = PathCombineAbs(strAts64DetailManagerFilePath, "detailmodules.txt");
                            tbAts64DetailModules.Text = strAts64SettingTextFilePath;
                            OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.BVE6);
                            IsDetailmanager64 = true;
                            btnAts64Open.Enabled = true;
                            btnAts64Open2.Enabled = true;
                            tbAts64DetailModules.Enabled = true;
                        }
                    }
                    else
                    {
                        strDisp += "BVE6用(64bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n";
                        IsDetailmanager64 = false;
                        btnAts64Open.Enabled = false;
                        btnAts64Open2.Enabled = false;
                        tbAts64DetailModules.Enabled = false;
                    }

                }

                if (error > 0)
                {
                    strDisp += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m\n";
                }

                tabControlVehicle.SelectedIndex = 0;

            }
            else
            {
                btnOpenVehicleFile.Enabled = false;
                btnOpenVehicleDirectory.Enabled = false;
                btnOpenVehicleFile.Enabled = false;
                strDisp += "車両ファイルがないか指定されていません\n";
            }
            if (strDisp != "")
            {
                if (cbMessageDisp.Checked)
                {
                    MessageBox.Show(strDisp);
                }                
                tsslDisp.Text = strDisp.Replace("\n", " ").Trim();
                strDisp = "";
            }

        }



        private void OpenNewAtsPluginFile(string strAtsPluginFilePath, BVE_Version bve_ver)
        {
            if (File.Exists(strAtsPluginFilePath))
            {
                if (bve_ver == BVE_Version.BVE5)
                {
                    btnAts32OpenDirectory.Enabled = true;
                    btnAts32Check.Enabled = true;
                    dgvAts32.Rows.Clear();
                    //クラス化準備
                    ats32Plugin = new AtsPlugin(vehicle.Ats32.FilePath);
                    for (int i = 0; i < ats32Plugin.AtsList.Count; i++)
                    {
                        dgvAts32.Rows.Add(ats32Plugin.AtsList[i].FileName, ats32Plugin.AtsList[i].Version.ToString(), ats32Plugin.AtsList[i].RerativePath, ats32Plugin.AtsList[i].AbsolutePath);
                    }
                    ColoringDataGridView(dgvAts32, BVE_Version.BVE5);
                    if (File.Exists(strAts32SettingTextFilePath + ".bak"))
                    {
                        btnAts32Recovery.Visible = true;
                    }
                }
                else
                {
                    btnAts64OpenDirectory.Enabled = true;
                    btnAts64Check.Enabled = true;
                    dgvAts64.Rows.Clear();
                    //クラス化準備
                    ats64Plugin = new AtsPlugin(vehicle.Ats64.FilePath);
                    for (int i = 0; i < ats64Plugin.AtsList.Count; i++)
                    {

                        dgvAts64.Rows.Add(ats64Plugin.AtsList[i].FileName, ats64Plugin.AtsList[i].Version.ToString(), ats64Plugin.AtsList[i].RerativePath, ats64Plugin.AtsList[i].AbsolutePath);

                    }
                    ColoringDataGridView(dgvAts64, BVE_Version.BVE6);
                    if (File.Exists(strAts64SettingTextFilePath + ".bak"))
                    {
                        btnAts64Recovery.Visible = true;
                    }
                }
            }
            else
            {
                strDisp += "ATSプラグインが見つかりません:" + strAtsPluginFilePath + "\n";
            }
        }

        private void ColoringDataGridView(DataGridView dataGridView, BVE_Version version)
        {
            for (int i = 0; i < dataGridView.RowCount; i++)
            {
                if (dataGridView[1, i].Value.ToString() == BVE_Version.NotFound.ToString())
                {
                    dataGridView[1, i].Style.BackColor = Color.Red;
                }
                if (version == BVE_Version.BVE5)
                {
                    if (dataGridView[1, i].Value.ToString() == BVE_Version.BVE6.ToString())
                    {
                        dataGridView[1, i].Style.BackColor = Color.Yellow;
                    }
                }
                else
                {
                    if (dataGridView[1, i].Value.ToString() == BVE_Version.BVE5.ToString())
                    {
                        dataGridView[1, i].Style.BackColor = Color.Yellow;
                    }
                }

                if (dataGridView[2, i].Value.ToString().StartsWith("#") || dataGridView[2, i].Value.ToString().StartsWith(";"))
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
        }

        private void btnOpenRouteFile_Click(object sender, EventArgs e)
        {
            ProcessStart(scenario.FilePath, false);
        }

        private void btnOpenVehicleFile_Click(object sender, EventArgs e)
        {
            ProcessStart(scenario.VehicleFilesAbs[cbxVehicleIndex], false);
        }

        private void btnOpenAts32_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts32SettingTextFilePath, false);
        }

        private void btnOpenAts32Directory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Ats32.FilePath, true);
        }

        private void btnOpenAts64Directory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Ats64.FilePath, true);
        }

        private void btnOpenVehicleDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(scenario.VehicleFilesAbs[cbxVehicleIndex], true);
        }

        private void btnAtsPluginSelect_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                ofd.FileName = "";
                ofd.Filter = "ATSプラグインファイル(*.dll)|*.dll";
                ofd.Title = "ATSプラグインファイルを選択してください";
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                ofd.InitialDirectory = Settings.Default.AtsPluginFileDirectory;
                //ofd.RestoreDirectory = true;

                //ダイアログを表示する
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
            //flgAtsPluginDirectoryOpen = true;
            btnOpenScenario.Enabled = true;
            tbAtsPluginFile.Enabled = true;

            BVE_Version iRet = AtsPluginChecker(strAtsPluginFilePath, 300, cbMessageDisp.Checked);
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
                    if (vehicle.FileVersion >= 2.0)
                    {
                        btnAts64RelatePathGen.Enabled = true;
                        tbAts64.Enabled = true;
                    }
                    else
                    {
                        btnAts64RelatePathGen.Enabled = false;
                        tbAts64.Enabled = false;
                    }
                }
            }
        }

        private string GenerateRelativePath(string originalFilePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(originalFilePath) || string.IsNullOrWhiteSpace(targetPath)) return "";

            try
            {
                // % をエスケープ（元のコードの工夫を継承）
                string u1Path = originalFilePath.Replace("%", "%25");
                string u2Path = targetPath.Replace("%", "%25");

                Uri u1 = new Uri(u1Path);
                Uri u2 = new Uri(u2Path);

                // 相対パスを取得
                Uri relativeUri = u1.MakeRelativeUri(u2);
                string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                // デコードと区切り文字の変換
                return relativePath.Replace("%25", "%").Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return ""; // パスが不正な場合など
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
                "3.他列車ファイル、ほか\r\n"+
                "\r\n" +
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします。また、本ツールで「改造」したデータのアップロードは禁止です※\r\n" +
                "\r\n", "使い方");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TempFileChecker();
            Settings.Default.Save();
        }

        private void TempFileChecker()
        {
            if (scenario != null)
            {
                if (File.Exists(scenario.FilePath + ".tmp.txt"))
                {
                    File.Delete(scenario.FilePath + ".tmp.txt");
                }
                if (File.Exists(scenario.FilePath + ".bak"))
                {
                    File.Delete(scenario.FilePath + ".bak");
                }
            }
        }

        /// <summary>
        /// プロセス開始メソッド
        /// </summary>
        /// <param name="FilePath">ファイルパス</param>
        /// <param name="IsDirectory">ディレクトリ指定か？</param>
        private void ProcessStart(string FilePath, bool IsDirectory = false)
        {
            if (!IsDirectory)
            {
                if (File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
                else
                {
                    if (cbMessageDisp.Checked)
                    {
                        MessageBox.Show("指定先のファイルが見つかりません");
                    }
                }
            }
            else
            {           
                if (Directory.Exists(Path.GetDirectoryName(FilePath)))
                {
                    Process.Start(Path.GetDirectoryName(FilePath));
                }
                else
                {
                    if (cbMessageDisp.Checked)
                    {
                        MessageBox.Show("指定先のフォルダが見つかりません");
                    }
                }
            }
        }

        private void btnPerfoemanceCurveOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.PerformanceCurve.FilePath);
        }


        private void btnPerfoemanceCurveDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.PerformanceCurve.FilePath, true);
        }

        private void btnParametersOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Parameters.FilePath);
        }

        private void btnParametersDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Parameters.FilePath, true);
        }

        private void btnPanelOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Panel.FilePath);
        }


        private void btnPanelDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Panel.FilePath, true);
        }

        private void btnSoundOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Sound.FilePath);
        }
        private void btnSoundDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Sound.FilePath, true);
        }

        private void btnMotorNoiseOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.MotorNoise.FilePath);
        }

        private void btnMotorNoiseDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.MotorNoise.FilePath, true);
        }

        private void btnAts64Open_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts64SettingTextFilePath);
        }

        private void btnMapOpen_Click_1(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath);
        }

        private void btnMapDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath, true);
        }

        private void btnStructureOpen_Click_1(object sender, EventArgs e)
        {
            ProcessStart(map.Structure.FilePathAbs);
        }

        private void btnStructureDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Structure.FilePathAbs, true);
        }

        private void btnStationOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Station.FilePathAbs);
        }
        private void btnStationDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Station.FilePathAbs, true);
        }
        private void btnSignalOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Signal.FilePathAbs);
        }
        private void btnSignalDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Signal.FilePathAbs, true);
        }

        private void btnSoundListOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.SoundList.FilePathAbs);
        }
        private void btnSoundListDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.SoundList.FilePathAbs, true);
        }

        private void btnTrainOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(cbxTrain.Text);
        }

        private void btnTrainDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(cbxTrain.Text, true);
        }

        //マップファイルのファイルパスを生成する
        private string PathGenerator_Map(string _line, string _dirPath)
        {
            string s;
            if (_line.IndexOf("'") > 0)
            {
                int index = _line.IndexOf("'");
                string filepath = (_line.Substring(index + 1, _line.IndexOf("'", index + 1) - index - 1)).Trim();
                s = PathCombineAbs(_dirPath, filepath);
            }
            else if ((_line.IndexOf("(") > 0))
            {
                string filepath = (_line.Substring(_line.IndexOf("(") + 1, _line.IndexOf(")", _line.IndexOf("(") + 1) - _line.IndexOf("(") - 1)).Trim();
                s = PathCombineAbs(_dirPath, filepath);
            }
            else
            {
                s = "";
            }
            return s;
        }

        private int PathControl_Map(ref TextBox tb, ref Button btnFile, ref Button btnDirectory, Contents_Map c, out string strFilePath_)
        {
            if (c != null)
            {
                strFilePath_ = c.FilePathAbs;
                if (c.Ret == 1)
                {
                    tb.Text = c.FilePathAbs;
                    btnFile.Enabled = true;
                    btnDirectory.Enabled = true;
                    tb.BackColor = SystemColors.Window;
                }
                else if (c.Ret == -1)
                {
                    tb.Text = "Not Found or Supported : " + c.FilePathAbs;
                    tb.BackColor = Color.LightYellow;
                }
                else
                {
                    tb.Text = "Not Found : Not Declared";
                    tb.BackColor = Color.LightYellow;
                }
                return c.Ret;

            }
            else
            {
                tb.Text = "Not Found : Not Declared";
                tb.BackColor = Color.LightYellow;
                strFilePath_ = "";
                return -1;
            }
        }

        private int PathControl_Map(ref ComboBox cb, ref Button btnFile, ref Button btnDirectory, List<Contents_Map> c, out List<string> strFilePath_)
        {
            strFilePath_ = new List<string>();
            if (c != null)
            {
                for (int i = 0; i < c.Count; i++)
                {
                    strFilePath_.Add(c[i].FilePathAbs);
                    cb.Items.Add(c[i].FilePathAbs);
                }
                if (c.Count > 0)
                {
                    cb.Text = c[0].FilePathAbs;
                    btnFile.Enabled = true;
                    btnDirectory.Enabled = true;
                    cb.BackColor = SystemColors.Window;
                    return c[0].Ret;
                }
                else
                {
                    cb.Text = "Not Found : Not Declared";
                    cb.BackColor = Color.LightYellow;
                    strFilePath_ = null;
                    return -1;
                }
            }
            else
            {
                cb.Text = "Not Found : Not Declared";
                cb.BackColor = Color.LightYellow;
                strFilePath_ = null;
                return -1;
            }
        }

        //車両ファイルの存在を確認する
        private int PathControl_Vehicle(ref TextBox tb, ref Button btnFile, ref Button btnDirectory, Contents_Vehicle c, out string strFilePath_)
        {
            if (c != null)
            {
                strFilePath_ = c.FilePath;
                if (c.Ret == 1)
                {
                    tb.Text = c.FilePath;
                    btnFile.Enabled = true;
                    btnDirectory.Enabled = true;
                    tb.BackColor = SystemColors.Window;
                }
                else if (c.Ret == -1)
                {
                    tb.Text = "Not Found or Supported : " + c.FilePath;
                    tb.BackColor = Color.LightYellow;
                }
                else
                {
                    tb.Text = "Not Found : Not Declared";
                    tb.BackColor = Color.LightYellow;
                }
                return c.Ret;
            }
            else
            {
                tb.Text = "Not Found : Not Declared";
                tb.BackColor = Color.LightYellow;
                strFilePath_ = "";
                return -1;
            }
        }

        //リセット
        private void Reset()
        {
            //クラスクリア
            map = null;
            vehicle = null;

            btnOpenScenarioFile.Enabled = false;
            btnOpenVehicleFile.Enabled = false;
            btnOpenVehicleDirectory.Enabled = false;
            btnAts32OpenDirectory.Enabled = false;
            btnAts64OpenDirectory.Enabled = false;
            strVehicleFilePath = "";

            btnBve5BootChooseVehicle.Enabled = false;
            btnBve5BootChooseMap.Enabled = false;
            btnBootBVE5.Enabled = false;
            btnBve6BootChooseVehicle.Enabled = false;
            btnBve6BootChooseMap.Enabled = false;
            btnBootBVE6.Enabled = false;

            cbxScenarioEnc.SelectedIndex = 0;

            //gbxBve6.Visible = false;

            strMapFilePath = "";
            strAts32DetailManagerFilePath = "";
            strAts32SettingTextFilePath = "";
            strAts64DetailManagerFilePath = "";
            strAts64SettingTextFilePath = "";

            btnOpenVehicleFile.Enabled = false;
            btnPerfoemanceCurveOpen.Enabled = false;
            btnPerfoemanceCurveDirectory.Enabled = false;
            btnParametersOpen.Enabled = false;
            btnParametersDirectory.Enabled = false;
            btnPanelOpen.Enabled = false;
            btnPanelDirectory.Enabled = false;
            btnSoundOpen.Enabled = false;
            btnSoundDirectory.Enabled = false;
            btnMotorNoiseOpen.Enabled = false;
            btnMotorNoiseDirectory.Enabled = false;
            btnAts32Open.Enabled = false;
            btnAts64Open.Enabled = false;
            btnAts32Check.Enabled = false;
            btnAts64Check.Enabled = false;



            btnAts32Open2.Enabled = false;
            btnAts32Reset.Visible = false;
            btnAts32Save.Visible = false;
            btnAts32Recovery.Visible = false;

            btnAts64Open2.Enabled = false;
            btnAts64Reset.Visible = false;
            btnAts64Save.Visible = false;
            btnAts64Recovery.Visible = false;

            btnAts32RelatePathGen.Enabled = false;
            btnAts32Add.Enabled = false;
            btnAts64RelatePathGen.Enabled = false;
            btnAts64Add.Enabled = false;

            cbxVehicle.Items.Clear();

            tbSeinarioFileName.Text = "";
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32RelatePath.Text = "";
            tbAts64RelatePath.Text = "";
            tbAts32.Text = "";
            tbAts32DetailModules.Text = "";
            tbAts64.Text = "";
            tbAts64DetailModules.Text = "";
            cbxVehicle.Text = "";
            cbxMapFilePath.Text = "";
            dgvAts32.Rows.Clear();
            dgvAts64.Rows.Clear();


            //マップファイルページ
            btnMapOpen.Enabled = false;
            btnMapDirectory.Enabled = false;
            btnStructureOpen.Enabled = false;
            btnStructureDirectory.Enabled = false;
            btnStationOpen.Enabled = false;
            btnStationDirectory.Enabled = false;
            btnSignalOpen.Enabled = false;
            btnSignalDirectory.Enabled = false;
            btnSoundListOpen.Enabled = false;
            btnSoundListDirectory.Enabled = false;
            btnSound3DListOpen.Enabled = false;
            btnSound3DListDirectory.Enabled = false;
            btnTrainOpen.Enabled = false;
            btnTrainDirectory.Enabled = false;
            tbStructure.Text = "";
            tbStation.Text = "";
            tbSignal.Text = "";
            tbSoundList.Text = "";
            tbSound3DList.Text = "";
            cbxMapFilePath.Items.Clear();
            listMapFilePath.Clear();
            cbxTrain.Items.Clear();
            cbxTrain.Text = "";
            cbxTrain.BackColor = SystemColors.Control;

            pbVehicleImage.Image = null;

            IsDetailmanager32 = false;
            IsDetailmanager64 = false;

            btnBve6Convert.Enabled = false;

            lblConvert.Text = "コンバート後、[BVE6 DetailModules]タブで読込状況を確認してください。";
            tbDetailModuleStoreDetail.Text = "推奨例:32bit DetailManager.dll格納フォルダ直下または同列に[x64]や[ATS64]フォルダを作成します";
            btnDetailModuleStoreClear.Enabled = false;
            btnDetailModuleStoreClear.Visible = false;

            btnBve5Recovery.Visible = false;

            btnOpenVehicleFile.BackColor = SystemColors.Control;

            //シナリオDataGridViewクリア
            dgvScenario.Rows.Clear();

            //車両データDataGridViewクリア
            dgvVehicle.Columns.Clear();
            dgvPerformanceCurve.DataSource = null;
            dgvPerformanceCurve.Columns.Clear();
            dgvParameters.Columns.Clear();
            dgvPanel.Columns.Clear();
            dgvSound.Columns.Clear();
            dgvMotorNoise.Columns.Clear();
            tabControlVehicle.SelectedIndex = 0;
            cbxParametersEnc.SelectedIndex = 0;
            cbxPerformanceCurveEnc.SelectedIndex = 0;
            cbxSoundEnc.SelectedIndex = 0;
            cbxPanelEnc.SelectedIndex = 0;
            cbxMotorNoiseEnc.SelectedIndex = 0;

            //マップファイルDataGridViewクリア
            dgvRoute.DataSource = null;
            dgvRoute.Columns.Clear();
            dgvStructure.DataSource = null;
            dgvStructure.Columns.Clear();
            dgvStation.DataSource = null;
            dgvStation.Columns.Clear();
            dgvSignal.DataSource = null;
            dgvSignal.Columns.Clear();
            dgvSound.DataSource = null;
            dgvSoundList.Columns.Clear();
            dgvSound3DList.DataSource = null;
            dgvSound3DList.Columns.Clear();
            dgvTrain.DataSource = null;
            dgvTrain.Columns.Clear();
            dgvTrainFileList.DataSource = null;
            dgvTrainFileList.Columns.Clear();
            tabControlMaps.SelectedIndex = 0;
            cbxStationEnc.SelectedIndex = 0;
            cbxSignalEnc.SelectedIndex = 0;
            cbxStructureEnc.SelectedIndex = 0;
            cbxSoundEnc.SelectedIndex= 0;
            cbxSound3DListEnc.SelectedIndex = 0;
            cbxTrainFileEnc.SelectedIndex = 0;


            tabControlScenario.SelectTab(0);

        }

        private void btnBootBVE5_Click(object sender, EventArgs e)
        {
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + scenario.FilePath + "\"");
            }
            else
            {
                MessageBox.Show("BVE5が見つかりません。BVEパスを設定してください。");
            }
        }

        private void btnBootBVE6_Click(object sender, EventArgs e)
        {
            if (File.Exists(strBve6Path))
            {
                Process.Start(strBve6Path, "\"" + scenario.FilePath + "\"");
            }
            else
            {
                MessageBox.Show("BVE6が見つかりません。BVEパスを設定してください。");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            dgvFiles.Columns.Add("ListFileName", "ファイル名");
            dgvFiles.Columns[0].MinimumWidth = 220;
            dgvFiles.Columns[0].HeaderCell.ToolTipText = "ファイル名をクリックで展開、ダブルクリックで開きます";
            dgvFiles.Columns.Add("ListFileAuthor", "作　者");
            dgvFiles.Columns[1].MinimumWidth = 100;
            dgvFiles.Columns.Add("ListVehicleCount", "V");
            dgvFiles.Columns[2].Width = 30;
            dgvFiles.Columns[2].HeaderCell.ToolTipText = "車両ファイル数";
            dgvFiles.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvFiles.Columns.Add("ListMapCount", "M");
            dgvFiles.Columns[3].Width = 30;
            dgvFiles.Columns[3].HeaderCell.ToolTipText = "路線データ数";
            dgvFiles.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvFiles.Columns.Add("ListVehicleVersion", "V ver");
            dgvFiles.Columns[4].Width = 60;
            dgvFiles.Columns[4].HeaderCell.ToolTipText = "車両データバージョン(1定義目)";
            dgvFiles.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvFiles.Columns.Add("ListMapVersion", "M ver");
            dgvFiles.Columns[5].Width = 60;
            dgvFiles.Columns[5].HeaderCell.ToolTipText = "路線データバージョン(1定義目)";
            dgvFiles.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            ToolTip ttBtnListBVE5 = new ToolTip();
            ttBtnListBVE5.SetToolTip(btnListBVE5, "BVE5対応のシナリオファイルを強調します"); // ボタンとテキストを紐付け
            ToolTip ttBtnListBVE6 = new ToolTip();
            ttBtnListBVE6.SetToolTip(btnListBVE5, "BVE6対応のシナリオファイルを強調します"); // ボタンとテキストを紐付け
            ToolTip ttBtnListNoVehicle = new ToolTip();
            ttBtnListNoVehicle.SetToolTip(btnListNoVehicle, "車両定義のないシナリオファイルを強調します"); // ボタンとテキストを紐付け
            ToolTip ttBtnListNoMap = new ToolTip();
            ttBtnListNoMap.SetToolTip(btnListNoMap, "BVE6対応のシナリオファイルを強調します"); // ボタンとテキストを紐付け
        }

        private void btnBackUp_Click(object sender, EventArgs e)
        {
            Make_NewVehicleFile(scenario.FilePath, scenario.FilePath + ".tmp.txt");
        }

        private void Make_NewVehicleFile(string oldFileName_, string newFileName_)
        {
            File.Copy(oldFileName_, newFileName_ + ".bak", true);

            //読み込むテキストファイル
            string textFile = oldFileName_;
            //文字コード(ここでは、Shift JIS)
            Encoding enc = Encoding.GetEncoding("utf-8");

            string[] lines = File.ReadAllLines(textFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(";") != 0 && lines[i].IndexOf("#") != 0 && lines[i].Contains('='))
                {
                    if (lines[i].IndexOf("vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lines[i] = "Vehicle = " + GenerateRelativePath(oldFileName_, cbxVehicle.Text);
                    }
                }
            }
            //内容を書き込む
            //ファイルが存在しているときは、上書きする
            File.WriteAllLines(newFileName_, lines, enc);
        }
        private void make_newMapFile(string oldFileName_, string newFileName_)
        {
            File.Copy(oldFileName_, oldFileName_ + ".bak", true);

            //読み込むテキストファイル
            string textFile = oldFileName_;
            //文字コード(ここでは、Shift JIS)
            Encoding enc = Encoding.GetEncoding("utf-8");

            string[] lines = File.ReadAllLines(textFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(";") != 0 && lines[i].IndexOf("#") != 0 && lines[i].Contains('='))
                {
                    if (lines[i].IndexOf("route", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lines[i] = "Route = " + GenerateRelativePath(oldFileName_, cbxVehicle.Text);
                    }
                }
            }
            //内容を書き込む
            //ファイルが存在しているときは、上書きする
            File.WriteAllLines(newFileName_, lines, enc);
        }

        private void Make_NewDetailModulesFile(string oldFileName_, string newFileName_)
        {
            if (!File.Exists(newFileName_))
            {
                File.WriteAllText(newFileName_, "");
            }

            //読み込むテキストファイル
            string textFile = oldFileName_;
            //文字コード(ここでは、Shift JIS)
            Encoding enc = Encoding.GetEncoding("utf-8");
            if (oldFileName_.IndexOf("detailmodules.txt", StringComparison.OrdinalIgnoreCase) > 0)
            {
                string[] lines = File.ReadAllLines(textFile);

                for (int i = 0; i < lines.Length; i++)
                {
                    string strAts32Path = PathCombineAbs(strAts32SettingTextFilePath,lines[i]);
                    //mikan-go-go様 Plugin対応判定
                    if (lines[i].IndexOf(@"\GeneralAtsPlugin\Rock_On", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        string[] keyword = { "Rock_On" };
                        string[] arr = strAts32Path.Split(keyword, StringSplitOptions.None);
                        string strAts64Path = arr[0] + keyword[0] + @"\x64" + arr[1];
                        if (File.Exists(strAts64Path) && (AtsPluginChecker(strAts64Path, 300, false) == BVE_Version.BVE6))
                        {
                            lines[i] = GenerateRelativePath(strAts64SettingTextFilePath, strAts64Path);
                        }
                        else
                        {
                            lines[i] = "#" + GenerateRelativePath(strAts64SettingTextFilePath, strAts32Path);
                        }
                    }
                    else
                    {
                        lines[i] = "#" + GenerateRelativePath(strAts64SettingTextFilePath, strAts32Path);
                    }
                }
                //内容を書き込む
                //ファイルが存在しているときは、上書きする
                File.WriteAllLines(newFileName_, lines, enc);
            }
            else
            {
                File.WriteAllText(newFileName_, "", enc);
            }
        }

        private void cbMessage_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.cbMessage = cbMessageDisp.Checked;
            Settings.Default.Save();
        }

        private void btnBve5BootChooseVehicle_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = scenario.FilePath + ".tmp.txt";
            Make_NewVehicleFile(scenario.FilePath, strNewRouteFile);
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVE5が見つかりません。BVE5パスを設定してください。");
            }
        }

        private void btnBve6BootChooseVehicle_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = scenario.FilePath + ".tmp.txt";
            Make_NewVehicleFile(scenario.FilePath, strNewRouteFile);
            if (File.Exists(strBve6Path))
            {
                Process.Start(strBve6Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVE6が見つかりません。BVE6パスを設定してください。");
            }
        }

        private void cbxMapFilePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbStructure.Text = "";
            tbStation.Text = "";
            tbSignal.Text = "";
            tbSoundList.Text = "";
            tbSound3DList.Text = "";
            /*cbxTrain.Text = "";
            cbxMapFilePath.Text = "";*/
            //コンボボックスのインデックスを取得
            cbxMapIndex = cbxMapFilePath.SelectedIndex;
            if (cbxMapFilePath.Items.Count > 1)
            {
                //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
                OpenNewMapFile(listMapFilePath[cbxMapIndex]);
            }
        }

        private void btnBve5BootChooseMap_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = scenario.FilePath + ".tmp.txt";
            make_newMapFile(scenario.FilePath, strNewRouteFile);
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVEが見つかりません。BVEパスを設定してください。");
            }
        }

        private void btnBve6BootChooseMap_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = scenario.FilePath + ".tmp.txt";
            make_newMapFile(scenario.FilePath, strNewRouteFile);
            if (File.Exists(strBve6Path))
            {
                Process.Start(strBve6Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVEが見つかりません。BVEパスを設定してください。");
            }
        }

        private void btnBve5PathSetting_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                //はじめのファイル名を指定する
                //はじめに「ファイル名」で表示される文字列を指定する
                ofd.FileName = "bvets.exe";
                //はじめに表示されるフォルダを指定する
                //指定しない（空の文字列）の時は、現在のディレクトリが表示される
                if (File.Exists(strBve5Path) && File.Exists(Settings.Default.strBve5Path))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(Settings.Default.strBve5Path);
                }
                else
                {
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                }
                //[ファイルの種類]に表示される選択肢を指定する
                //指定しないとすべてのファイルが表示される
                ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
                //タイトルを設定する
                ofd.Title = "BVE5の実行ファイルを選択してください";
                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    strBve5Path = ofd.FileName;
                    AtsPlugin checker = new AtsPlugin();
                    if (checker.Checker(ofd.FileName, 300, false) == BVE_Version.BVE5)
                    {
                        Settings.Default.strBve5Path = strBve5Path;
                    }
                    else
                    {
                        MessageBox.Show("BVE5の実行ファイルではありません");
                    }
                }

            }
        }

        private void btnBve6PathSetting_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                //はじめのファイル名を指定する
                //はじめに「ファイル名」で表示される文字列を指定する
                ofd.FileName = "bvets.exe";
                //はじめに表示されるフォルダを指定する
                //指定しない（空の文字列）の時は、現在のディレクトリが表示される
                if (File.Exists(strBve6Path) && File.Exists(Settings.Default.strBve6Path))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(Settings.Default.strBve6Path);
                }
                else
                {
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                }
                //[ファイルの種類]に表示される選択肢を指定する
                //指定しないとすべてのファイルが表示される
                ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
                //タイトルを設定する
                ofd.Title = "BVE6の実行ファイルを選択してください";
                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    strBve6Path = ofd.FileName;
                    AtsPlugin checker = new AtsPlugin();
                    if (checker.Checker(ofd.FileName, 300, false) == BVE_Version.BVE6)
                    {
                        Settings.Default.strBve6Path = strBve6Path;
                    }
                    else
                    {
                        MessageBox.Show("BVE6の実行ファイルではありません");
                    }
                }
            }
        }

        private void cbxVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32.Text = "";
            tbAts32DetailModules.Text = "";
            tbAts64.Text = "";
            tbAts64DetailModules.Text = "";
            dgvAts32.Rows.Clear();
            dgvAts64.Rows.Clear();

            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(scenario.VehicleFilesAbs[cbxVehicleIndex]);
        }

        private void btnAts32Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle.Ats32.FilePath, 300, true);
        }

        private void btnAts64Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle.Ats64.FilePath, 300, true);
        }

        private BVE_Version AtsPluginChecker(string _FilePath, int _BufferSize, bool IsDisplayChecked)
        {
            //MessageBox.Show($"hit"+" AtsPluginChecker\r\n"+_FilePath);
            AtsPlugin atsPlugin = new AtsPlugin();
            return atsPlugin.Checker(_FilePath, _BufferSize, IsDisplayChecked);
        }

        private void btnAts32RPathGen_Click(object sender, EventArgs e)
        {
            tbAts32RelatePath.Text = GenerateRelativePath(strAts32SettingTextFilePath, strAtsPluginFilePath);
            if (tbAts32RelatePath.Text != "")
            {
                tbAts32RelatePath.Enabled = true;
                btnAts32Add.Enabled = true;
            }
        }

        private void btnAts64RPathGen_Click(object sender, EventArgs e)
        {
            tbAts64RelatePath.Text = GenerateRelativePath(strAts64SettingTextFilePath, strAtsPluginFilePath);
            if (tbAts64RelatePath.Text != "")
            {
                tbAts64RelatePath.Enabled = true;
                btnAts64Add.Enabled = true;
            }
        }

        private void btnAts32Add_Click(object sender, EventArgs e)
        {
            bool hit = false;
            for (int i = 0; i < dgvAts32.RowCount; i++)
            {
                if (dgvAts32[2, i].Value.ToString() == tbAts32RelatePath.Text)
                {
                    hit = true;
                    break;
                }
            }

            if (!hit)
            {
                AtsList atsList = new AtsList(tbAts32RelatePath.Text, strAts32SettingTextFilePath);
                dgvAts32.Rows.Add(atsList.FileName, atsList.Version, atsList.RerativePath, atsList.AbsolutePath);
                btnAts32Reset.Visible = true;
                btnAts32Save.Visible = true;
            }
            else
            {
                MessageBox.Show("既に適用されています");
            }
        }

        private void btnAts64Add_Click(object sender, EventArgs e)
        {
            bool hit = false;
            for (int i = 0; i < dgvAts64.RowCount; i++)
            {
                if (dgvAts64[2, i].Value.ToString() == tbAts64RelatePath.Text)
                {
                    hit = true;
                    break;
                }
            }

            if (!hit)
            {
                AtsList atsList = new AtsList(tbAts64RelatePath.Text, strAts64SettingTextFilePath);
                dgvAts64.Rows.Add(atsList.FileName, atsList.Version, atsList.RerativePath, atsList.AbsolutePath);
                btnAts64Reset.Visible = true;
                btnAts64Save.Visible = true;
            }
            else
            {
                MessageBox.Show("既に適用されています");
            }

        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (cbxVehicle.Items.Count > 0)
            {
                if (cbxVehicle.SelectedIndex == -1)
                {
                    cbxVehicle.SelectedIndex = 0;
                }
                //コンボボックスのインデックスを取得
                cbxVehicleIndex = cbxVehicle.SelectedIndex;
                //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
                OpenNewVehicleFile(scenario.VehicleFilesAbs[cbxVehicleIndex]);
            }
        }

        private void btnDetailModuleSelect_Click(object sender, EventArgs e)
        {
            //[引用]https://www.sejuku.net/blog/49295#index_id1

            using (FolderBrowserDialog fbDialog = new FolderBrowserDialog())
            {

                // ダイアログの説明文を指定する
                fbDialog.Description = "ダイアログの説明文";

                // デフォルトのフォルダを指定する
                fbDialog.SelectedPath = Path.GetDirectoryName(strAts32DetailManagerFilePath);

                // 「新しいフォルダーを作成する」ボタンを表示する
                fbDialog.ShowNewFolderButton = true;

                //フォルダを選択するダイアログを表示する
                if (fbDialog.ShowDialog() == DialogResult.OK)
                {
                    if (fbDialog.SelectedPath + @"\detailmodules.txt" == strAts32SettingTextFilePath)
                    {
                        MessageBox.Show("BVE5用(32bit)のフォルダと同一には出来ません。別のフォルダを選択するか作成してください。");
                    }
                    else
                    {
                        strAts64DetailManagerFilePath = fbDialog.SelectedPath + @"\DetailManager.dll";
                        strAts64SettingTextFilePath = fbDialog.SelectedPath + @"\detailmodules.txt";
                        tbDetailModuleStoreDetail.Text = fbDialog.SelectedPath;
                        tbAts64.Text = strAts64DetailManagerFilePath;
                        btnDetailModuleStoreClear.Enabled = true;
                        if (!File.Exists(strAts64DetailManagerFilePath))
                        {
                            File.Copy(strAts64DetailManagerOrgFilePath, strAts64DetailManagerFilePath);
                            if (!File.Exists(strAts64SettingTextFilePath))
                            {
                                File.WriteAllText(strAts64SettingTextFilePath, "");
                            }
                        }

                        if ((File.Exists(strAts64DetailManagerFilePath) && AtsPluginChecker(strAts64DetailManagerFilePath, 300, false) == BVE_Version.BVE6))
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

            }
        }


        private string strAts64DetailManagerOrgFilePath = "";
        private void btnDetailManagerBve6Select_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "BVE6 DetailManager(DetailManager.dll)|DetailManager.dll";
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                ofd.InitialDirectory = Settings.Default.DetailManager64Path;
                //ofd.RestoreDirectory = true;

                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (AtsPluginChecker(ofd.FileName, 300, false) == BVE_Version.BVE6)
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
        }

        private void btnBve6Convert_Click(object sender, EventArgs e)
        {
            if (!File.Exists(strVehicleFilePath + @".bak"))
            {
                File.Move(strVehicleFilePath, strVehicleFilePath + @".bak");
            }
            using (StreamReader sr_temp = new StreamReader(strVehicleFilePath + @".bak"))
            {
                Encoding enc = Encoding.GetEncoding("shift_jis");
                string tmp_str = sr_temp.ReadLine();
                if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    enc = Encoding.GetEncoding("utf-8");
                }
                using (StreamReader sr = new StreamReader(strVehicleFilePath + @".bak", enc))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            File.WriteAllText(strVehicleFilePath, "Bvets Vehicle 2.00", enc);
                        }
                        else if (enc == Encoding.GetEncoding("shift_jis"))
                        {
                            File.AppendAllText(strVehicleFilePath, ":shift_jis", enc);
                        }
                        else if ((line.IndexOf("ATS", StringComparison.OrdinalIgnoreCase) >= 0 && line.IndexOf("Ats32", StringComparison.OrdinalIgnoreCase) < 0) && line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            File.AppendAllText(strVehicleFilePath, Environment.NewLine + "Ats32 = " + GenerateRelativePath(strVehicleFilePath, strAts32DetailManagerFilePath) + Environment.NewLine + "Ats64 = " + GenerateRelativePath(strVehicleFilePath, strAts64DetailManagerFilePath), enc);
                        }
                        else
                        {
                            File.AppendAllText(strVehicleFilePath, Environment.NewLine + line, enc);
                        }
                    }
                }
            }
            Make_NewDetailModulesFile(strAts32SettingTextFilePath, strAts64SettingTextFilePath);
            OpenNewVehicleFile(scenario.VehicleFilesAbs[cbxVehicleIndex]);

        }

        private void btnBve5Recovery_Click(object sender, EventArgs e)
        {
            bool exist = false;
            if (File.Exists(scenario.VehicleFilesAbs[cbxVehicleIndex] + @".bak"))
            {
                File.Copy(scenario.VehicleFilesAbs[cbxVehicleIndex] + @".bak", scenario.VehicleFilesAbs[cbxVehicleIndex], true);
                exist = true;
            }
            if (exist)
            {
                File.Delete(strVehicleFilePath + @".bak");
                btnBve5Recovery.Visible = false;
                lblConvert.Text = "コンバート後、[BVE6 DetailModules]タブで読込状況を確認してください。";
                dgvAts64.Rows.Clear();
                OpenNewVehicleFile(scenario.VehicleFilesAbs[cbxVehicleIndex]);
            }
        }

        private void btnDetailManagerBve6Clear_Click(object sender, EventArgs e)
        {
            btnBve6Convert.Enabled = false;
            tbDetailManagerDetail.Text = @"例:\BveTs\Scenarios\UchAibo20\E217r\Ats\x64\DetailManager.dll等";
            Settings.Default.DetailManager64Path = "";
        }

        private void btnDetailModuleStoreDetailClear_Click(object sender, EventArgs e)
        {
            btnBve6Convert.Enabled = false;
            tbDetailModuleStoreDetail.Text = "推奨例:32bit DetailManager.dll格納フォルダ直下または同列に[x64]や[ATS64]フォルダを作成します";
            btnDetailModuleStoreClear.Enabled = false;
        }

        private void btnSound3DListOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Sound3DList.FilePathAbs, false);
        }

        private void btnSound3DListDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Sound3DList.FilePathAbs, true);
        }

        private void tsmi32Delete_Click(object sender, EventArgs e)
        {
            if (tabControlDetailModules.SelectedIndex == 0)
            {
                foreach (DataGridViewRow r in dgvAts32.SelectedRows)
                {
                    if (!r.IsNewRow)
                    {
                        dgvAts32.Rows.Remove(r);
                    }
                }
            }
            else
            {

                foreach (DataGridViewRow r in dgvAts64.SelectedRows)
                {
                    if (!r.IsNewRow)
                    {
                        dgvAts64.Rows.Remove(r);
                    }
                }
            }

        }


        private bool flgListBVE5 = false;
        private bool flgListBVE6 = false;
        private bool flgListNoVehicle = false;
        private bool flgListNoMap = false;

        private void btnScenarioClear_Click(object sender, EventArgs e)
        {
            Reset();
            //dgvFileListのインデックスをクリア
            indexDgvFiles = -1;
            btnScenarioClear.Enabled = false;
            btnScenarioReload.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // FolderBrowserDialogのインスタンスを作成
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // ダイアログの説明文を設定
            folderBrowserDialog.SelectedPath = Settings.Default.RouteFileDirectory;

            // ユーザーが新しいフォルダを作成できるようにする (必要に応じて)
            folderBrowserDialog.ShowNewFolderButton = true;

            // 初期ディレクトリを設定 (必要に応じて)
            // folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // ダイアログを表示
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)

            {
                // 選択されたフォルダのパスを取得
                string selectedFolderPath = folderBrowserDialog.SelectedPath;
                OpenSenaroDirectory(selectedFolderPath,true);
            }
        }

        private int filesCount = 0;
        private async void OpenSenaroDirectory(string selectedFolderPath, bool IsReadIndexOnly = false)
        {
            if (Directory.Exists(selectedFolderPath))
            {
                lblLegend.Text = "読込中";
                pgBarList.Visible = true;
                btnListBVE5.Visible = false;
                btnListBVE6.Visible = false;
                btnListNoVehicle.Visible = false;
                btnListNoMap.Visible = false;
                btnListOther.Visible = false;
                btnOpenScenarioDirectory.Enabled = true;
                // リストビューをクリア
                dgvFiles.Rows.Clear();

                try
                {
                    // フォルダ内のファイルパスを取得
                    string[] files = System.IO.Directory.GetFiles(selectedFolderPath, "*.txt");
                    tbScenarioDirectory.Text = selectedFolderPath;
                    Settings.Default.RouteFileDirectory = selectedFolderPath;
                    Settings.Default.Save();
                    filesCount = 0;
                    pgBarList.Minimum = 0;
                    pgBarList.Maximum = files.Length;
                    pgBarList.Value = 0;
                    lblPgBarStatus.Visible = true;
                    lblPgBarStatus.Text = "";

                    // UIスレッドで実行されるプログレスハンドラ
                    var progress = new Progress<ScenarioRow>(row =>
                    {
                        pgBarList.Value = filesCount;
                        lblPgBarStatus.Text = filesCount.ToString() + "/" + files.Length.ToString();
                        dgvFiles.Rows.Add(
                            row.FileName,
                            row.Author,
                            row.VehicleCount,
                            row.MapCount,
                            row.VehicleVersion,
                            row.MapVersion
                        );
                    });
                    try
                    {
                        // 非同期で実行
                        await Task.Run(() => LoadFilesAsync(selectedFolderPath, progress, IsReadIndexOnly));
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
                btnListBVE5.Visible = true;
                btnListBVE6.Visible = true;
                btnListNoVehicle.Visible = true;
                btnListNoMap.Visible = true;
                btnListOther.Visible = true;
                Settings.Default.RouteFileDirectory = selectedFolderPath;
                Settings.Default.Save();

            }
            else
            {
                btnOpenScenarioDirectory.Enabled = false;
            }
        }

        private void LoadFilesAsync(string folderPath, IProgress<ScenarioRow> progress, bool IsReadIndexOnly = false)
        {
            string[] files = Directory.GetFiles(folderPath, "*.txt");
            foreach (string file in files)
            {
                Scenario sn = new Scenario(file);
                var row = new ScenarioRow
                {
                    FileName = Path.GetFileName(file),
                    Author = sn.Author ?? "",
                    VehicleCount = sn.VehicleFilesExistsCount,
                    MapCount = sn.MapFilesCount
                };

                if (sn.VehicleFilesExistsCount > 0)
                {
                    Vehicle vehicle = new Vehicle(sn.VehicleFilesAbs[0], IsReadIndexOnly);
                    row.VehicleVersion = vehicle.FileVersion.ToString("0.00");
                }
                else
                {
                    row.VehicleVersion = "";
                }

                if (sn.MapFilesCount > 0)
                {
                    Map map = new Map(sn.MapFilesAbs[0],IsReadIndexOnly);
                    row.MapVersion = map.FileVersion.ToString("0.00");
                }
                else
                {
                    row.MapVersion = "";
                }
                filesCount++;

                progress.Report(row);
            }
        }

        private void DgvFilesViewColoring() {
            for (int i = 1; i < dgvFiles.RowCount-2; i++)
            {
                int.TryParse(dgvFiles.Rows[i+1].Cells[2].Value.ToString(), out int VehicleFilesCount);
                int.TryParse(dgvFiles.Rows[i+1].Cells[3].Value.ToString(), out int MapFilesCount);
                double.TryParse(dgvFiles.Rows[i+1].Cells[4].Value.ToString(), out double VehicleFilesVersion);
                //色付け
                //MessageBox.Show("i" + i + " " + dgvFiles.Rows[i + 1].Cells[2].Value.ToString());
                if (VehicleFilesCount == 0 && MapFilesCount == 0)
                {
                    dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = Color.DarkGray;

                }
                else if (VehicleFilesCount == 0 && flgListNoVehicle)
                {
                    dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = Color.LightYellow;
                }
                else if (MapFilesCount == 0 && flgListNoMap)
                {
                    dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = Color.LightGray;
                }
                else
                {
                    if (VehicleFilesVersion == 2.0 && flgListBVE6 && MapFilesCount > 0)
                    {
                        dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = Color.Thistle;
                    }
                    else if (VehicleFilesVersion > 0.0 && VehicleFilesVersion <= 2.0 && flgListBVE5 && MapFilesCount > 0)
                    {
                        dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = Color.PaleGreen;
                    }
                    else
                    {
                        dgvFiles.Rows[i+1].DefaultCellStyle.BackColor = SystemColors.Window;
                    }
                }
            }
        }

        private void btnOpenScenarioDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(tbScenarioDirectory.Text, true);
        }

        private void btnScenarioReload_Click(object sender, EventArgs e)
        {
            string tmpFile = tbSeinarioFileName.Text;
            Reset();
            scenario = new Scenario(tmpFile);
            OpenScenario();

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // 3. Ctrl + T が押されたか判定
            if (e.Control && e.KeyCode == Keys.T)
            {
                if (_hiddenTabPage != null)
                {
                    // 最後にタブを追加
                    tabControlScenario.TabPages.Add(_hiddenTabPage);
                    // 元の場所(インデックス0)に挿入したい場合
                    // tabControl1.TabPages.Insert(0, _hiddenTabPage);
                    _hiddenTabPage = null; // 変数をクリア
                }

                // イベントを処理済みにして、他のコントロールに伝播させない
                e.Handled = true;
                e.SuppressKeyPress = true;

                gbxBve6Converter.Visible = true;
            }
        }

        private void tabControlMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (map == null) return;
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlMaps.SelectedTab;
            // タブのテキストやインデックスで処理を分岐
            if (currentTab.Name == "tpStructure")
            {
                StructureFileExtract();
            }
            // タブのテキストやインデックスで処理を分岐
            else if (currentTab.Name == "tpStation")
            {
                StationFileExtract();
            }
            else if (currentTab.Name == "tpSignal")
            {
                SignalFileExtract();
            }
            else if (currentTab.Name == "tpSoundList")
            {
                SoundListListUp();
            }
            else if (currentTab.Name == "tpSound3D")
            {
                Sound3DListExtract();
            }
            // タブのテキストやインデックスで処理を分岐
            else if (currentTab.Name == "tpTrain")
            {
                // 1. DataTableの作成
                DataTable dtTrainFileList = new DataTable();
                dtTrainFileList.Columns.Add("TrainKey");
                dtTrainFileList.Columns.Add("FilePath");
                dtTrainFileList.Columns.Add("TrackKey");  // 必要に応じて追加
                dtTrainFileList.Columns.Add("Direction"); // 必要に応じて追加

                // 2. データの展開
                // map.Train は List<Contents_Map> なので、各要素(item)をループ
                foreach (var trainContent in map.Train)
                {
                    // 各 Contents_Map が持っている TrainFilesList (タプルのリスト) をループ
                    foreach (var train in trainContent.TrainFilesList)
                    {
                        dtTrainFileList.Rows.Add(
                            train.trainKey,
                            train.filePath,
                            train.trackKey,
                            train.direction
                        );
                    }
                }

                // 3. DataGridViewへの反映
                dgvTrainFileList.DataSource = dtTrainFileList;

                TrainFileExtract();
            }
        }

        private void Sound3DListExtract(Encoding enc = null)
        {
            if (map.Sound3DList != null && File.Exists(map.Sound3DList.FilePathAbs))
            {

                if (enc == null)
                {
                    // Shift_JISを指定して読み込む場合
                    enc = GetEndcordFromFile(map.Sound3DList.FilePathAbs, out int encMode);
                    cbxSound3DListEnc.SelectedIndex = encMode;
                }
                dt = new DataTable();
                using (TextFieldParser parser = new TextFieldParser(map.Sound3DList.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    string[] strs = { "SoundKey", "FilePath", "BufferSize", "Remarks" };
                    foreach (string field in strs)
                    {
                        dt.Columns.Add(field);
                    }

                    // データ行の読み込み
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        string[] subset = new string[4];
                        string remark = "";
                        char[] del = { '#', ';' };
                        List<string> temp_str;
                        bool flgFound = false;
                        int flgFoundIndex = 0;
                        for (int i = 1; i < fields.Length; i++)
                        {
                            if (fields[i].Contains("#") || fields[i].Contains(";"))
                            {
                                flgFound = true;
                                flgFoundIndex = i;
                                temp_str = fields[i].Split(del).ToList();
                                fields[i] = temp_str[0];
                                for (int j = 1; j < temp_str.Count; j++)
                                {
                                    if (temp_str[j].Length > 1)
                                    {
                                        remark = temp_str[j];
                                    }
                                }
                            }
                            if (flgFound && flgFoundIndex > i)
                            {
                                remark += fields[i];
                            }
                        }
                        //備考を追記
                        if (remark != "")
                        {
                            subset[3] = remark;
                            Array.Copy(fields, 0, subset, 0, fields.Length);
                            fields = subset;
                        }
                        //field配列が4以上の場合の場合分け
                        if (fields.Length <= 4)
                        {
                            dt.Rows.Add(fields);
                        }
                        else
                        {
                            Array.Copy(fields, 0, subset, 0, 3);
                            dt.Rows.Add(subset);
                        }
                    }
                }

                // DataGridViewにバインド
                dgvSound3DList.DataSource = dt;
                //ソート禁止
                foreach (DataGridViewColumn c in dgvSound3DList.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvSound3DList.Columns[0].HeaderCell.ToolTipText = "任意の文字列。このサウンド名は、マップファイル、停車場リストファイル、他列車ファイルで使用します。";
                dgvSound3DList.Columns[1].HeaderCell.ToolTipText = "wavファイルの相対パス(ダブルクリックで開きます)";
                dgvSound3DList.Columns[2].HeaderCell.ToolTipText = "その音を同時に再生できる数。省略した場合は 1 になります。";

                for (int i = 0; i < dgvSound3DList.Rows.Count - 1; i++)
                {
                    /*if (bools[i])
                    {
                        dgvSoundList.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    else
                    {*/
                    string cellValue = dgvSound3DList.Rows[i].Cells[1].Value.ToString();
                    if (!File.Exists(PathCombineAbs(map.Sound3DList.FilePathAbs, cellValue)) && cellValue != "")
                    {
                        dgvSound3DList.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                    cellValue = dgvSound3DList.Rows[i].Cells[0].Value.ToString();
                    if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                    {
                        dgvSound3DList.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if (cellValue == "")
                    {
                        dgvSound3DList.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                    //}
                }
            }
        }

        private void StationFileExtract(Encoding enc = null)
        {
            if (File.Exists(map.Station.FilePathAbs))
            {
                if (enc == null)
                {
                    // Shift_JISを指定して読み込む場合
                    enc = GetEndcordFromFile(map.Station.FilePathAbs, out int encMode);
                    cbxStationEnc.SelectedIndex = encMode;
                }

                dt = new DataTable();
                string[] strs = { "stationKey", "stationName", "arrivalTime", "depertureTime", "stoppageTime", "defaultTime", "signalFlag", "alightingTime", "passengers", "arrivalSoundKey", "depertureSoundKey", "doorReopen", "stuckInDoor" };
                foreach (string field in strs)
                {
                    dt.Columns.Add(field);
                }
                using (TextFieldParser parser = new TextFieldParser(map.Station.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    // データ行の読み込み
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        string[] subset = new string[2];
                        if (fields.Length <= 13)
                        {
                            dt.Rows.Add(fields);
                        }
                        else
                        {
                            Array.Copy(fields, 0, subset, 0, 2);
                            dt.Rows.Add(subset);
                        }
                        //}
                    }
                }

                // DataGridViewにバインド
                dgvStation.DataSource = dt;
                //ソート禁止
                foreach (DataGridViewColumn c in dgvStation.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                string[] tttStations = { "停車場名 (任意の文字列)", "時刻表に表示する停車場名", "到着時刻 (HH:mm:ss) (p: 通過駅)", "出発時刻または通過時刻 (HH:mm:ss) (t: 終着駅)", "標準停車時間 [s]", "駅にジャンプしたときの時刻 (HH:mm:ss)", "出発信号 (0: 最高現示 | 1: depertureTime の stoppageTime 前の時刻まで停止現示)", "降車時間 [s]", "出発時の乗車率", "ドアが開いたときに再生される音 (サウンドリストファイルで定義した soundKey)", "depertureTime の stoppageTime 前の時刻に再生される音 (サウンドリストファイルで定義した soundKey)", "ドアが再開閉される確率", "旅客がドアに挟まる時間 [s]" };
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dgvStation.Columns[i].HeaderCell.ToolTipText = tttStations[i];
                }
                //色付け
                for (int i = 0; i < dgvStation.Rows.Count - 1; i++)
                {
                    string cellValue = dgvStation.Rows[i].Cells[0].Value.ToString();
                    if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                    {
                        dgvStation.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if (cellValue == "")
                    {
                        dgvStation.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                }
            }
        }

        private void SignalFileExtract(Encoding enc = null)
        {
            if (map.Signal != null && System.IO.File.Exists(map.Signal.FilePathAbs))
            {
                if (enc == null)
                {
                    // Shift_JISを指定して読み込む場合
                    enc = GetEndcordFromFile(map.Signal.FilePathAbs, out int encMode);
                    cbxSignalEnc.SelectedIndex = encMode;
                }

                dt = new DataTable();
                int colnum = 0;
                //一旦最大列数をカウント
                using (TextFieldParser parser = new TextFieldParser(map.Signal.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }


                    // 1行ずつ読み込む
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        if (fields.Length > colnum)
                        {
                            colnum = fields.Length;
                        }

                    }

                }
                //データ配置
                using (TextFieldParser parser = new TextFieldParser(map.Signal.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    // 1行ずつ読み込む
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();

                        // 最初の行で列数を確認してDataTableの列を追加する
                        if (dt.Columns.Count == 0)
                        {
                            for (int i = 0; i < colnum; i++)
                            {
                                dt.Columns.Add((i + 1).ToString()); // 仮の列名
                            }
                        }
                        // 行を追加
                        dt.Rows.Add(fields);
                    }
                }

                // DataGridViewにバインド
                dgvSignal.DataSource = dt;
                //ソート禁止
                foreach (DataGridViewColumn c in dgvSignal.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void StructureFileExtract(Encoding enc = null)
        {
            if (map != null && map.Structure != null && File.Exists(map.Structure.FilePathAbs))
            {
                if (enc == null)
                {
                    // Shift_JISを指定して読み込む場合
                    enc = GetEndcordFromFile(map.Structure.FilePathAbs, out int encMode);
                    cbxStructureEnc.SelectedIndex = encMode;
                }

                dt = new DataTable();
                string[] strs = { "StructureNae", "FilePath", "Remarks" };
                foreach (string field in strs)
                {
                    dt.Columns.Add(field);
                }
                using (TextFieldParser parser = new TextFieldParser(map.Structure.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    // データ行の読み込み
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        // インデックス1から2つを新しい配列にコピー
                        string[] subset = new string[3];
                        string remark = "";
                        char[] del = { '#', ';' };
                        List<string> temp_str;
                        bool flgFound = false;
                        int flgFoundIndex = 0;
                        for (int i = 1; i < fields.Length; i++)
                        {
                            if (fields[i].Contains("#") || fields[i].Contains(";"))
                            {
                                flgFound = true;
                                flgFoundIndex = i;
                                temp_str = fields[i].Split(del).ToList();
                                fields[i] = temp_str[0];
                                for (int j = 1; j < temp_str.Count; j++)
                                {
                                    if (temp_str[j].Length > 1)
                                    {

                                        remark = temp_str[j];
                                    }
                                }
                            }
                            if (flgFound && i > flgFoundIndex)
                            {
                                remark += fields[i];
                            }
                        }
                        //備考を追記
                        if (remark != "")
                        {
                            subset[2] = remark;
                            Array.Resize(ref fields, 2);
                            Array.Copy(fields, 0, subset, 0, fields.Length);
                            fields = subset;
                        }
                        //field配列が3以上の場合の場合分け
                        if (fields.Length <= 3)
                        {
                            dt.Rows.Add(fields);
                        }
                        else
                        {
                            Array.Copy(fields, 0, subset, 0, 2);
                            dt.Rows.Add(subset);
                        }
                    }

                }

                // DataGridViewにバインド
                dgvStructure.DataSource = dt;
                //ソート禁止
                foreach (DataGridViewColumn c in dgvStructure.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                string[] tttStructures = { "任意の文字列。このストラクチャー名は、マップファイル、信号現示リストファイル、他列車ファイルで使用します。", "ストラクチャーファイルの相対パス(ダブルクリックで開きます)。ストラクチャビューワ5にドラッグ＆ドロップで表示できます。", "備考" };
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    dgvStructure.Columns[i].HeaderCell.ToolTipText = tttStructures[i];
                }
                //色付け
                for (int i = 0; i < dgvStructure.Rows.Count - 1; i++)
                {
                    string cellValue = dgvStructure.Rows[i].Cells[1].Value.ToString();
                    if (!File.Exists(PathCombineAbs(map.Structure.FilePathAbs, cellValue)) && cellValue != "")
                    {
                        dgvStructure.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                    cellValue = dgvStructure.Rows[i].Cells[0].Value.ToString();
                    if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                    {
                        dgvStructure.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if (cellValue == "")
                    {
                        dgvStructure.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                }
            }
        }

        private void TrainFileExtract()
        {
            //他列車(Train)
            // 1. DataTableを作成 (ヘッダー用)
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value");
            dt.Columns.Add("Remarks");

            // ファイルが存在するか確認
            if (File.Exists(cbxTrain.Text))
            {
                // Shift_JISを指定して読み込む場合
                Encoding enc = GetEndcordFromFile(cbxTrain.Text, out int encMode);
                cbxTrainFileEnc.SelectedIndex = encMode;
                // 2. ファイルを読み込む
                string[] lines = File.ReadAllLines(cbxTrain.Text, enc);

                foreach (string line in lines)
                {
                    // 空行をスキップ
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // 3. '=' で分割する
                    string[] parts = line.Split('=');

                    if (parts.Length >= 2)
                    {
                        // ';' で分割する
                        string[] parts2 = parts[1].Split(';');
                        if (parts2.Length < 2)
                        {
                            // 分割されたデータ（前・後）をDataTableに追加
                            dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                        }
                        else
                        {
                            // 分割されたデータ（前・後）をDataTableに追加
                            dt.Rows.Add(parts[0].Trim(), parts2[0].Trim(), parts2[1].Trim());
                        }
                    }
                    else if (parts.Length == 1)
                    {
                        // '='がない場合の処理（必要に応じて）
                        dt.Rows.Add(parts[0].Trim(), "");
                    }
                }

                // 4. DataGridViewにデータをセット
                /////
                dgvTrain.DataSource = dt;
                if (dgvTrain.Rows.Count > 0)
                {
                    // 最初の行(インデックス0)を削除
                    dgvTrain.Rows.RemoveAt(0);
                }
                //ソート禁止
                foreach (DataGridViewColumn c in dgvTrain.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;

                //色付け
                for (int i = 0; i < dgvTrain.Rows.Count - 1; i++)
                {
                    string cellValue = dgvTrain.Rows[i].Cells[0].Value.ToString();
                    if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                    {
                        dgvTrain.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    else if (cellValue == "")
                    {
                        dgvTrain.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                    else if (cellValue.Contains("["))
                    {
                        dgvTrain.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                    }

                }
            }
            else
            {
                dgvTrain.DataSource = null;
                dgvTrain.Rows.Clear();

            }
        }

        private void SoundListListUp(Encoding enc = null)
        {
            if (map != null && map.SoundList != null && File.Exists(map.SoundList.FilePathAbs))
            {
                if (enc == null)
                {
                    enc = GetEndcordFromFile(tbSoundList.Text, out int encMode);
                    cbxSoundListEnc.SelectedIndex = encMode;
                }
                dt = new DataTable();
                using (TextFieldParser parser = new TextFieldParser(map.SoundList.FilePathAbs, enc))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(","); // カンマ区切り

                    //1行目読み飛ばし
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    string[] strs = { "SoundKey", "FilePath", "BufferSize", "Remarks" };
                    foreach (string field in strs)
                    {
                        dt.Columns.Add(field);
                    }

                    // データ行の読み込み
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        string[] subset = new string[4];
                        string remark = "";
                        char[] del = { '#', ';' };
                        List<string> temp_str;
                        bool flgFound = false;
                        int flgFoundIndex = 0;
                        for (int i = 1; i < fields.Length; i++)
                        {
                            if (fields[i].Contains("#") || fields[i].Contains(";"))
                            {
                                flgFound = true;
                                flgFoundIndex = i;
                                temp_str = fields[i].Split(del).ToList();
                                fields[i] = temp_str[0];
                                for (int j = 1; j < temp_str.Count; j++)
                                {
                                    if (temp_str[j].Length > 1)
                                    {
                                        remark = temp_str[j];
                                    }
                                }
                            }
                            if (flgFound && flgFoundIndex > i)
                            {
                                remark += fields[i];
                            }
                        }
                        //備考を追記
                        if (remark != "")
                        {
                            subset[3] = remark;
                            Array.Copy(fields, 0, subset, 0, fields.Length);
                            fields = subset;
                        }
                        //field配列が4以上の場合の場合分け
                        if (fields.Length <= 4)
                        {
                            dt.Rows.Add(fields);
                        }
                        else
                        {
                            Array.Copy(fields, 0, subset, 0, 3);
                            dt.Rows.Add(subset);
                        }
                    }
                }

                // DataGridViewにバインド
                dgvSoundList.DataSource = dt;
                //ソート禁止
                foreach (DataGridViewColumn c in dgvSoundList.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvSoundList.Columns[0].HeaderCell.ToolTipText = "任意の文字列。このサウンド名は、マップファイル、停車場リストファイル、他列車ファイルで使用します。";
                dgvSoundList.Columns[1].HeaderCell.ToolTipText = "wavファイルの相対パス(ダブルクリックで開きます)";
                dgvSoundList.Columns[2].HeaderCell.ToolTipText = "その音を同時に再生できる数。省略した場合は 1 になります。";
                //色付け
                for (int i = 0; i < dgvSoundList.Rows.Count - 1; i++)
                {
                    string cellValue = dgvSoundList.Rows[i].Cells[1].Value.ToString();
                    if (!File.Exists(PathCombineAbs(map.SoundList.FilePathAbs, cellValue)) && cellValue != "")
                    {
                        dgvSoundList.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                    }
                    cellValue = dgvSoundList.Rows[i].Cells[0].Value.ToString();
                    if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                    {
                        dgvSoundList.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                    }
                    if (cellValue == "")
                    {
                        dgvSoundList.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                    //}
                }
            }
        }

        /// <summary>
        /// ファイルエンコードをヘッダから読み取るメソッド(Default:Utf-8)
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Encoding GetEndcordFromFile(string filePath, out int encMode)
        {
            // Shift_JISを指定して読み込む場合
            Encoding enc = Encoding.GetEncoding("utf-8");
            encMode = 1;

            //文字エンコードを確認
            using (StreamReader sr_temp = new StreamReader(filePath))
            {
                string tmp_str = sr_temp.ReadLine();
                if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) > 0 || tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    enc = Encoding.GetEncoding("Shift_JIS");
                    encMode = 2;
                }
            }

            return enc;
        }

        private void tabControlScenario_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlScenario.SelectedTab;
            
        }

        private void dgvStructure_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvStructure, map.Structure.FilePathAbs);
        }

        private void dgvPerformanceCurve_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvPerformanceCurve, tbPerfoemanceCurve.Text);
        }

        private void dgvSoundList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvSoundList, map.SoundList.FilePathAbs);
        }

        private void dgvPanel_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvPanel, tbPanel.Text);
        }

        private void dgvSound_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvSound, tbSound.Text);
        }

        private void dgvSound3DList_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvSound3DList, tbSound3DList.Text);
        }
        private void dgvMotorNoise_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 1, ref dgvMotorNoise, tbMotorNoise.Text);
        }

        private void GetFileData_ProcessStart_FromDataGridView(int columnIndex, int rowIndex, int v, ref DataGridView dataGridView, string fileName)
        {
            string cellValue = dataGridView.Rows[rowIndex].Cells[columnIndex].Value.ToString();
            string FilePath = PathCombineAbs(fileName,cellValue);
            GetFileData_ProcessStart(columnIndex, rowIndex, v, FilePath);
        }

        /// <summary>
        /// DataGridViewでクリックされた場所のファイルを開くメソッド
        /// </summary>
        /// <param name="column">クリックされた列</param>
        /// <param name="row">クリックされた行</param>
        /// <param name="colmunnum">ファイル指定列</param>
        private void GetFileData_ProcessStart(int column, int row, int colmunnum, string filePath)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (row < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (column == colmunnum)
            {
                if (File.Exists(filePath))
                {
                    Process.Start(filePath);
                }
            }
        }

        private void tabControlVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlVehicle.SelectedTab;

            // タブのテキストやインデックスで処理を分岐
            if (currentTab.Name == "tpPerformanceCurve") {

                //性能曲線表示(PerformanceCurve)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value");
                dt.Columns.Add("Remarks");

                // ファイルが存在するか確認
                if (File.Exists(tbPerfoemanceCurve.Text))
                {
                    //DataTableに展開、エンコードを表示
                    cbxPerformanceCurveEnc.SelectedIndex = DataExpandToDataTable(dt, tbPerfoemanceCurve.Text);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgvPerformanceCurve);
                    //色付け
                    DataGridViewColoringForVehicles(dgvPerformanceCurve);
                }
            }
            else if (currentTab.Name == "tpParameters")
            {
                //性能曲線表示(PerformanceCurve)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value");
                dt.Columns.Add("Remarks");

                // ファイルが存在するか確認
                if (File.Exists(tbParameters.Text))
                {
                    //DataTableに展開、エンコードを表示
                    cbxParametersEnc.SelectedIndex = DataExpandToDataTable(dt, tbParameters.Text);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgvParameters,true);
                    //色付け
                    DataGridViewColoringForVehicles(dgvParameters);
                }
            }
            else if (currentTab.Name == "tpPanel")
            {
                //パネル(Panel)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value1");
                dt.Columns.Add("Value2");
                dt.Columns.Add("Remarks");

                // ファイルが存在するか確認
                if (File.Exists(tbPanel.Text))
                {
                    //DataTableに展開、エンコードを表示
                    cbxPanelEnc.SelectedIndex = DataExpandToDataTableXY(dt, tbPanel.Text);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgvPanel, true);
                    //色付け
                    DataGridViewColoringForVehicles(dgvPanel);
                }
            }
            else if (currentTab.Name == "tpSound")
            {
                //サウンド(Sound)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value");
                dt.Columns.Add("Remarks");

                // ファイルが存在するか確認
                if (File.Exists(tbSound.Text))
                {
                    cbxSoundEnc.SelectedIndex = DataExpandToDataTable(dt, tbSound.Text);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgvSound, true);
                    //色付け
                    DataGridViewColoringForVehicles(dgvSound);
                }
            }
            else if (currentTab.Name == "tpMotorNoise")
            {
                //モーター音(MotorNoise)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value");
                dt.Columns.Add("Remarks");

                // ファイルが存在するか確認
                if (File.Exists(tbMotorNoise.Text))
                {
                    //DataTableに展開、エンコードを表示
                    cbxMotorNoiseEnc.SelectedIndex = DataExpandToDataTable(dt, tbMotorNoise.Text);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgvMotorNoise, true);
                    //色付け
                    DataGridViewColoringForVehicles(dgvMotorNoise);
                }
            }
        }

        private int DataExpandToDataTableXY(DataTable dt, string filePath, Encoding enc = null)
        {
            int encMode = 0;
            if (enc == null)
            {
                // Shift_JISを指定して読み込む場合
                enc = GetEndcordFromFile(filePath, out encMode);
            }
            // 2. ファイルを読み込む
            string[] lines = File.ReadAllLines(filePath, enc);

            foreach (string line in lines)
            {
                // 空行をスキップ
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 3. '=' で分割する
                string[] parts = line.Split('=');

                if (parts.Length >= 2)
                {
                    // partsのvalueを ',' で分割する
                    string[] parts2 = parts[1].Split(',');
                    if (parts2.Length < 2)
                    {
                        string[] parts3 = parts[1].Split(';');

                        if (parts3.Length < 2)
                        {
                            // 分割されたデータ（前・後）をDataTableに追加
                            dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                        }
                        else
                        {
                            dt.Rows.Add(parts[0].Trim(), parts3[0].Trim(), "", parts3[1].Trim());
                        }
                    }
                    else
                    {
                        string[] parts3 = parts2[1].Split(';');
                        if (parts3.Length < 2)
                        {
                            // 分割されたデータ（前・後）をDataTableに追加
                            dt.Rows.Add(parts[0].Trim(), parts2[0].Trim(), parts2[1].Trim());
                        }
                        else
                        {
                            dt.Rows.Add(parts[0].Trim(), parts2[0].Trim(), parts3[0].Trim(), parts3[1].Trim());
                        }
                    }
                }
                else if (parts.Length == 1)
                {
                    string[] parts2 = line.Split(';');
                    if (parts2.Length < 2 || parts2[0].Length == 0)
                    {
                        // '='がない場合の処理（必要に応じて）
                        dt.Rows.Add(parts[0].Trim(), "");
                    }
                    else
                    {
                        dt.Rows.Add(parts2[0].Trim(), "", "", parts2[1].Trim());
                    }
                }
            }
            return encMode;
        }

        /// <summary>
        /// DataGridViewの動作指定
        /// </summary>
        /// <param name="dt">DataTableを設定</param>
        /// <param name="dataGridView">DataGridViewを指定</param>
        /// <param name="headerRemove">ヘッダを削除(false)</param>
        /// <param name="sortable">ソート許可(false)</param>

        private void DataGridViewSettingForVehicle(DataTable dt, DataGridView dataGridView, bool headerRemove = false, bool sortable = false)
        {
            // 4. DataGridViewにデータをセット
            /////
            dataGridView.DataSource = dt;
            if (dataGridView.Rows.Count > 0 && headerRemove)
            {
                // 最初の行(インデックス0)を削除
                dataGridView.Rows.RemoveAt(0);
            }
            if (!sortable)
            {
                //ソート禁止
                foreach (DataGridViewColumn c in dataGridView.Columns)
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }
        /// <summary>
        /// DataGridViewの色付け
        /// </summary>
        /// <param name="dataGridView">DataGridViewを指定</param>
        private void DataGridViewColoringForVehicles(DataGridView dataGridView)
        {
            //色付け
            for (int i = 0; i < dataGridView.Rows.Count - 1; i++)
            {
                string cellValue = dataGridView.Rows[i].Cells[0].Value.ToString();
                if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                }
                else if (cellValue == "")
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
                else if (cellValue.Contains("["))
                {
                    dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.PaleGreen;
                }

            }
        }

        private int DataExpandToDataTable(DataTable dt,string filePath, Encoding enc = null)
        {
            int encMode = 0;
            if (enc == null)
            {
                // Shift_JISを指定して読み込む場合
                enc = GetEndcordFromFile(filePath , out encMode);
                cbxParametersEnc.SelectedIndex = encMode;
            }
            // 2. ファイルを読み込む
            string[] lines = File.ReadAllLines(filePath, enc);

            foreach (string line in lines)
            {
                // 空行をスキップ
                if (string.IsNullOrWhiteSpace(line)) continue;

                // 3. '=' で分割する
                string[] parts = line.Split('=');

                if (parts.Length >= 2)
                {
                    // ';' で分割する
                    string[] parts2 = parts[1].Split(';');
                    if (parts2.Length < 2)
                    {
                        // 分割されたデータ（前・後）をDataTableに追加
                        dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                    }
                    else
                    {
                        // 分割されたデータ（前・後）をDataTableに追加
                        dt.Rows.Add(parts[0].Trim(), parts2[0].Trim(), parts2[1].Trim());
                    }
                }
                else if (parts.Length == 1)
                {
                    // '='がない場合の処理（必要に応じて）
                    dt.Rows.Add(parts[0].Trim(), "");
                }
            }
            return encMode;
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

        private void dgvVehicle_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 0, ref dgvVehicle, tbSeinarioFileName.Text);
        }

        private void dgvVehicle_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            cbxVehicle.SelectedIndex = e.RowIndex;
        }

        private void btnAts32Open2_Click(object sender, EventArgs e)
        {
            ProcessStart(tbAts32DetailModules.Text,false);
        }

        private void btnAts64Open2_Click(object sender, EventArgs e)
        {
            ProcessStart(tbAts64DetailModules.Text, false);
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
            if (File.Exists(Settings.Default.DetailManager64Path + @"\DetailManager.dll"))
            {
                tbDetailManagerDetail.Text = Settings.Default.DetailManager64Path + @"\DetailManager.dll";
                strAts64DetailManagerOrgFilePath = Settings.Default.DetailManager64Path + @"\DetailManager.dll";

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
                OpenSenaroDirectory(Settings.Default.RouteFileDirectory,true);
            }
            else
            {
                string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\BveTs\Scenarios";
                if (Directory.Exists(tempDir))
                {
                    tbScenarioDirectory.Text = tempDir;
                    OpenSenaroDirectory(tempDir,true);
                    Settings.Default.RouteFileDirectory = tempDir;
                    Settings.Default.Save();

                }
            }
        }
        private static int indexDgvFiles = 0;
        private string strStrviewPath;

        private void dgvFiles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e != null && e.RowIndex >= 0 && e.RowIndex != indexDgvFiles)
            {
                indexDgvFiles = e.RowIndex;
                Reset();
                string cellValue = dgvFiles.Rows[indexDgvFiles].Cells[0].Value.ToString();
                string fullPath = tbScenarioDirectory.Text + @"\" + cellValue;                

                scenario = new Scenario(fullPath);
                OpenScenario();
            }
        }

        private void dgvFiles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e != null && e.RowIndex >= 0 && e.ColumnIndex == 0)
            {
                // 最初に選択されたアイテムを取得
                string cellValue = dgvFiles.Rows[e.RowIndex].Cells[0].Value.ToString();
                // SubItems[1] がファイルパスです（SubItems[0]はファイル名）。
                string fullPath = tbScenarioDirectory.Text + @"\" + cellValue;

                ProcessStart(fullPath, false);
            }

        }

        private void dgvRoute_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            GetFileData_ProcessStart_FromDataGridView(e.ColumnIndex, e.RowIndex, 0, ref dgvRoute, tbSeinarioFileName.Text);
        }

        private void dgvRoute_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            cbxMapFilePath.SelectedIndex = e.RowIndex;
        }

        private void dgvStation_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == 9 || e.ColumnIndex == 10)
            {
                if (dgvSoundList.RowCount == 0 || dgvSoundList == null)
                {
                    SoundListListUp();
                }
                string tmp_str = dgvStation[e.ColumnIndex, e.RowIndex].Value.ToString();
                if (!string.IsNullOrEmpty(tmp_str))
                {
                    string searchString = tmp_str; // 検索する文字列
                    string resultValue = null;

                    // DataGridViewの各行をループでチェック
                    foreach (DataGridViewRow row in dgvSoundList.Rows)
                    {
                        // 新しい行（最後の空行）をスキップ
                        if (row.IsNewRow) continue;

                        // セルの値がNullでないか、または目的の文字列と一致するか確認
                        if (row.Cells[0].Value != null && row.Cells[0].Value.ToString().Equals(searchString))
                        {
                            // マッチした行の別の列の値を取得
                            resultValue = row.Cells[1].Value.ToString();
                            ProcessStart(PathCombineAbs(tbSoundList.Text, resultValue), false);
                            break; // 最初に見つかった行で終了
                        }
                    }
                }
            }
        }

        private void dgvStation_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex != 2 && e.ColumnIndex != 3 && e.ColumnIndex != 5) return;

            string input = e.FormattedValue.ToString().Trim();

            // 1. 空白ならOK
            if (string.IsNullOrEmpty(input)) return;

            // 2. T または P ならOK (大文字小文字を区別しない)
            if (e.ColumnIndex == 2 || e.ColumnIndex == 3)
            {
                if (input.Equals("T", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("P", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            // 3. HH:mm:ss 形式ならOK
            if (TimeSpan.TryParseExact(input, @"hh\:mm\:ss", null, out _))
            {
                return;
            }

            // --- ここまで到達したらエラー ---
            string msg = (e.ColumnIndex == 5)
                ? "「空白」または「HH:MM:SS」形式で入力してください。"
                : "「空白」「T」「P」または「HH:MM:SS」形式で入力してください。";

            // いずれにも該当しない場合はエラー
            MessageBox.Show(msg,"入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // 入力キャンセル（フォーカスを移動させない）
            e.Cancel = true;

        }

        private void dgvTrainFileList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            cbxTrain.SelectedIndex = e.RowIndex;
            TrainFileExtract();
        }

        private DateTime _lastClickTime = DateTime.MinValue;
        private DataGridViewCell _lastClickedCell = null;
        private void dgvStructure_MouseDown(object sender, MouseEventArgs e)
        {
            // セルがクリックされたかチェック
            DataGridView.HitTestInfo hit = dgvStructure.HitTest(e.X, e.Y);
            if (hit.RowIndex >= 0 && hit.ColumnIndex == 1)
            {
                var clickedCell = dgvStructure.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
                TimeSpan duration = DateTime.Now - _lastClickTime;
                string textToDrag = PathCombineAbs(tbStructure.Text, clickedCell.Value?.ToString());

                if (!string.IsNullOrEmpty(textToDrag) && File.Exists(textToDrag))
                {
                    // 渡したいフォルダのフルパスを配列で用意
                    string[] folderPaths = { textToDrag };
                    // 1. .NET の DataObject を作成 (これは内部で Win32 IDataObject を実装している)
                    DataObject data = new DataObject();

                    // 2. FileDrop 形式（CF_HDROP）としてセット
                    // これにより、Win32 API レベルで正しい DROPFILES 構造体が自動生成される
                    data.SetData(DataFormats.FileDrop, true, folderPaths);

                    // 3. DoDragDrop を実行
                    // Form 上であれば Control.DoDragDrop、
                    // 純粋な API なら NativeMethods.DoDragDrop((System.Runtime.InteropServices.ComTypes.IDataObject)data, ...)
                    this.DoDragDrop(data, DragDropEffects.Copy);
                }

                // ダブルクリックの判定 (Windowsの設定値を基準にするとより良い)
                if (duration.TotalMilliseconds < SystemInformation.DoubleClickTime && _lastClickedCell == clickedCell)
                {
                    // ここでダブルクリック時の処理を実行
                    GetFileData_ProcessStart_FromDataGridView(hit.ColumnIndex, hit.RowIndex, 1, ref dgvStructure, map.Structure.FilePathAbs);

                    // 重要: ダブルクリックが処理されたら、クリック時間とセルをリセット
                    _lastClickTime = DateTime.MinValue;
                    _lastClickedCell = null;
                }
                else
                {
                    _lastClickTime = DateTime.Now;
                    _lastClickedCell = clickedCell;
                }
            }
        }

        private void btnStrviewPathSetting_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            using (OpenFileDialog ofd = new OpenFileDialog())
            {

                //はじめのファイル名を指定する
                //はじめに「ファイル名」で表示される文字列を指定する
                ofd.FileName = "strview5.exe";
                //はじめに表示されるフォルダを指定する
                //指定しない（空の文字列）の時は、現在のディレクトリが表示される
                if (File.Exists(strStrviewPath) && File.Exists(Settings.Default.strStrviewPath))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(Settings.Default.strStrviewPath);
                }
                //[ファイルの種類]に表示される選択肢を指定する
                //指定しないとすべてのファイルが表示される
                ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
                //タイトルを設定する
                ofd.Title = "ストラクチャビューワを選択してください";
                //ダイアログを表示する
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
            Process.Start(strStrviewPath);
        }

        private void dgvSound_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string cellValue = dgvSound.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            string FilePath = PathCombineAbs(tbSound.Text, cellValue);
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                if (File.Exists(FilePath))
                {
                    // SoundPlayerのインスタンスを作成
                    player = new System.Media.SoundPlayer(FilePath);

                    // 再生（非同期）
                    player.Play();
                    btnPlayStop.Visible = true;
                    btnPlayStop.Click += BtnPlayStop_Click;

                    // 必要に応じて後で破棄
                    player.Dispose();
                }
            }
        }

        private void dgvSoundList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string cellValue = dgvSoundList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            string FilePath = PathCombineAbs(tbSoundList.Text, cellValue);
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                if (File.Exists(FilePath))
                {
                    // SoundPlayerのインスタンスを作成
                    player = new System.Media.SoundPlayer(FilePath);

                    // 再生（非同期）
                    player.Play();
                    btnPlayStop.Visible = true;
                    btnPlayStop.Click += BtnPlayStop_Click;

                    // 必要に応じて後で破棄
                    player.Dispose();
                }
            }
        }
        private System.Media.SoundPlayer player;
        private void dgvStation_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //サウンドリストファイルを再生
            if (e.ColumnIndex == 9 || e.ColumnIndex == 10)
            {
                if (dgvSoundList.RowCount == 0 || dgvSoundList == null)
                {
                    SoundListListUp();
                }
                string tmp_str = dgvStation[e.ColumnIndex, e.RowIndex].Value.ToString();
                if (!string.IsNullOrEmpty(tmp_str))
                {
                    string searchString = tmp_str; // 検索する文字列
                    string resultValue = null;

                    // DataGridViewの各行をループでチェック
                    foreach (DataGridViewRow row in dgvSoundList.Rows)
                    {
                        // 新しい行（最後の空行）をスキップ
                        if (row.IsNewRow) continue;

                        // セルの値がNullでないか、または目的の文字列と一致するか確認
                        if (row.Cells[0].Value != null && row.Cells[0].Value.ToString().Equals(searchString))
                        {
                            // マッチした行の別の列の値を取得
                            resultValue = PathCombineAbs(tbSoundList.Text, row.Cells[1].Value.ToString());
                            if (File.Exists(resultValue))
                            {
                                // SoundPlayerのインスタンスを作成
                                player = new System.Media.SoundPlayer(resultValue);

                                // 再生（非同期）
                                player.Play();
                                btnPlayStop.Visible = true;
                                btnPlayStop.Click += BtnPlayStop_Click;

                                // 必要に応じて後で破棄
                                player.Dispose();
                            }
                            break; // 最初に見つかった行で終了
                        }
                    }
                }
            }
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            player.Stop();
            btnPlayStop.Click -= BtnPlayStop_Click;
            btnPlayStop.Visible = false;
        }

        private string PathCombineAbs(string directroy, string path)
        {
            string filePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName (directroy.Trim()), path.Trim()));
            return filePath;
        }

        private void dgvSound3DList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string cellValue = dgvSound3DList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            string FilePath = PathCombineAbs(tbSound3DList.Text, cellValue);
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                if (File.Exists(FilePath))
                {
                    // SoundPlayerのインスタンスを作成
                    player = new System.Media.SoundPlayer(FilePath);

                    // 再生（非同期）
                    player.Play();
                    btnPlayStop.Visible = true;
                    btnPlayStop.Click += BtnPlayStop_Click;

                    // 必要に応じて後で破棄
                    player.Dispose();
                }
            }
        }

        private void dgvTrainFileList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ProcessStart(cbxTrain.Text,false);
        }

        private void cbxParametersEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            //パラメータ表示(Parameter)
            string[] columns = { "Key", "Value", "Remarks" };
            ExtractDataGridView(columns, dgvParameters, tbParameters.Text, cbxParametersEnc.Text);
        }

        private void cbxPanelEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            //パネル(Panel)
            // 1. DataTableを作成 (ヘッダー用)
            DataTable dt = new DataTable();
            dt.Columns.Add("Key");
            dt.Columns.Add("Value1");
            dt.Columns.Add("Value2");
            dt.Columns.Add("Remarks");

            // ファイルが存在するか確認
            if (File.Exists(tbPanel.Text) && cbxPanelEnc.SelectedIndex > 0)
            {
                //エンコードを選択
                Encoding enc = Encoding.GetEncoding(cbxParametersEnc.Text);
                //DataTableに展開
                DataExpandToDataTableXY(dt, tbPanel.Text,enc);
                //DataGridView設定
                DataGridViewSettingForVehicle(dt, dgvPanel, true);
                //色付け
                DataGridViewColoringForVehicles(dgvPanel);
            }
        }

        private void cbxPerformanceCurveEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            //性能曲線表示(PerformanceCurve)
            string[] columns = { "Key", "Value", "Remarks" };
            ExtractDataGridView(columns, dgvPerformanceCurve, tbPerfoemanceCurve.Text, cbxPerformanceCurveEnc.Text);
        }

        private void cbxSoundEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            //サウンド(Sound)
            string[] columns = { "Key", "Value", "Remarks" };
            ExtractDataGridView(columns, dgvSound, tbSound.Text, cbxSoundEnc.Text);
        }

        private void cbxMotorNoiseEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            //モーター音(MotorNoise)
            string[] columns = { "Key", "Value", "Remarks" };
            ExtractDataGridView(columns, dgvMotorNoise, tbMotorNoise.Text, cbxMotorNoiseEnc.Text);
        }

        private void ExtractDataGridView(string[] columns, DataGridView dgv, string filePath, string strEncode)
        {
            // 1. DataTableを作成 (ヘッダー用)
            DataTable dt = new DataTable();
            foreach (string c in columns)
            {
                dt.Columns.Add(c);
            }

            // ファイルが存在するか確認
            if (File.Exists(filePath))
            {
                try
                {
                    //エンコードを選択
                    Encoding enc = Encoding.GetEncoding(strEncode);
                    //DataTableに展開、エンコードを表示
                    DataExpandToDataTable(dt, filePath, enc);
                    //DataGridView設定
                    DataGridViewSettingForVehicle(dt, dgv, true);
                    //色付け
                    DataGridViewColoringForVehicles(dgv);
                }
                catch { }
            }
        }

        private void cbxStructureEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(tbStructure.Text) && cbxStructureEnc.SelectedIndex > 0)
            {
                Encoding enc = Encoding.GetEncoding(cbxStructureEnc.Text);
                StructureFileExtract(enc);
            }
        }

        private void cbxSignalEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(tbSignal.Text) && cbxSignalEnc.SelectedIndex > 0)
            {
                Encoding enc = Encoding.GetEncoding(cbxSignalEnc.Text);
                SignalFileExtract(enc);
            }
        }

        private void cbxStationEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(tbStation.Text) && cbxStationEnc.SelectedIndex > 0)
            {
                Encoding enc = Encoding.GetEncoding(cbxStationEnc.Text);
                StationFileExtract(enc);
            }
        }

        private void cbxSoundListEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(map.SoundList.FilePathAbs) && cbxSoundListEnc.SelectedIndex > 0)
            {
                Encoding enc = Encoding.GetEncoding(cbxSoundListEnc.Text);
                SoundListListUp(enc);
            }
        }

        private void cbxSound3DListEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(tbSound3DList.Text) && cbxSound3DListEnc.SelectedIndex > 0)
            {
                Encoding enc = Encoding.GetEncoding(cbxSound3DListEnc.Text);
                Sound3DListExtract(enc);
            }

        }

        private void cbxScenarioEnc_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*if (File.Exists(tbSeinarioFileName.Text) && cbxScenarioEnc.SelectedIndex > 0) {
                scenario = new Scenario(tbSeinarioFileName.Text, Encoding.GetEncoding(cbxScenarioEnc.Text));
                OpenScenario();
            }*/
        }
    }
 
}    
