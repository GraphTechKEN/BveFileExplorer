using BveFileExplorer.Properties;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.ComponentModel.Com2Interop;

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

            cbxVehicle.DrawItem += new DrawItemEventHandler(cbxVehicle_DrawItem);
            //tabControl1.TabPages.Remove(tpAtsPlugin);
            gbxBve6Converter.Visible = false;
            tabControlSenario.TabPages.RemoveAt(2);

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
        private Senario senario;
        private Map map;

        private AtsPlugin ats32Plugin;
        private AtsPlugin ats64Plugin;

        private DataTable dt;
        private void btnOpenSenario_Click(object sender, EventArgs e)
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


                    //OKボタンがクリックされたとき、選択されたファイルを読み取り専用で開く
                    senario = new Senario(ofd.FileName);
                    OpenSenario();
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
            if (senario.VehicleFilesCount > 0) {
                for (int i = 0; i < senario.VehicleFilesCount; i++) {
                    if (e.Index ==i && !senario.VehicleFilesExists[i]) backBrush = Brushes.Yellow; // 1番目のアイテム
                }
            }
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            // 文字の描画
            string text = cbxVehicle.Items[e.Index].ToString();
            e.Graphics.DrawString(text, e.Font, Brushes.Black, e.Bounds);

            // フォーカスを描画
            e.DrawFocusRectangle();
        }

        private void OpenSenario()
        {
            btnOpenSenarioFile.Enabled = true;
            btnSenarioReload.Visible = true;
            btnAtsPluginSelect.Enabled = true;
            btnBootBVE5.Enabled = true;
            tbSeinarioFileName.Enabled = true;
            tbSeinarioFileName.Text = senario.FilePath;
            btnSenarioClear.Visible = true;


            //内容を読み込み、表示する
            List<string> listMapFilePath;
            bool flgErrVehicle = false;
            bool flgErrMap = false;
            string dir = Path.GetDirectoryName(senario.FilePath);

            lblVehicleTitle.Text = senario.VehicleTitle;
            pictureBox1.ImageLocation = senario.ImagePath;
            lblRouteTitle.Text = senario.RouteTitle;
            listMapFilePath = senario.MapFiles;
            lblTitle.Text = senario.Title;
            lblAuthor.Text = senario.Author;
            tbComment.Text = senario.Comment;



            //車両ファイルが指定されている場合の処理
            if (senario.VehicleFilesExistsCount > 0 && !flgErrVehicle)
            {
                cbxVehicle.BackColor = SystemColors.Window;
                cbxVehicle.Text = senario.VehicleFilesAbs[0];
                OpenNewVehicleFile(senario.VehicleFilesAbs[0]);
                cbxVehicle.Items.AddRange(senario.VehicleFilesAbs.ToArray());

                if (senario.VehicleFilesCount > 1)
                {
                    btnBve5BootChooseVehicle.Enabled = btnBootBVE5.Enabled;
                    btnBve6BootChooseVehicle.Enabled = btnBootBVE6.Enabled;
                    strDisp += "車両ファイルが複数あります。データ数：" + senario.VehicleFilesCount + "\n";
                    cbxVehicle.SelectedIndex = 0;
                    if (senario.VehicleFilesNotExistsCount > 0)
                    {
                        btnOpenVehicleFile.BackColor = Color.Yellow;
                        strDisp += "車両ファイルのリンク切れがあります。データ数：" + senario.VehicleFilesNotExistsCount + "\n";
                    }
                }
                else
                {
                    btnBve5BootChooseVehicle.Enabled = false;
                    btnBve6BootChooseVehicle.Enabled = false;
                }

                if (tbAtsPluginFile.Text != "" && System.IO.File.Exists(tbAtsPluginFile.Text))
                {
                    AddAtsPlugin(tbAtsPluginFile.Text);
                }

                //性能曲線表示(PerformanceCurve)
                // 1. DataTableを作成 (ヘッダー用)
                DataTable dt = new DataTable();
                dt.Columns.Add("Key");
                dt.Columns.Add("Value");

                // ファイルが存在するか確認
                if (File.Exists(tbPerfoemanceCurve.Text))
                {
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(tbPerfoemanceCurve.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    // 2. ファイルを読み込む
                    string[] lines = File.ReadAllLines(tbPerfoemanceCurve.Text, enc);

                    foreach (string line in lines)
                    {
                        // 空行をスキップ
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // 3. '=' で分割する
                        string[] parts = line.Split('=');

                        if (parts.Length >= 2)
                        {
                            // 分割されたデータ（前・後）をDataTableに追加
                            dt.Rows.Add(parts[0].Trim(), parts[1].Trim());
                        }
                        else if (parts.Length == 1)
                        {
                            // '='がない場合の処理（必要に応じて）
                            dt.Rows.Add(parts[0].Trim(), "");
                        }
                    }

                    // 4. DataGridViewにデータをセット
                    dgvPerformanceCurve.DataSource = dt;

                    //ソート禁止
                    foreach (DataGridViewColumn c in dgvPerformanceCurve.Columns)
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    //色付け
                    for (int i = 0; i < dgvPerformanceCurve.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvPerformanceCurve.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                        {
                            dgvPerformanceCurve.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else if (cellValue == "")
                        {
                            dgvPerformanceCurve.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else if (cellValue.Contains("["))
                        {
                            dgvPerformanceCurve.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        }

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
            if (senario.MapFilesCount > 0 && !flgErrMap)
            {
                strMapFilePath = dir + @"\" + listMapFilePath[0].Trim();

                for (int i = 0; i < senario.MapFilesCount; i++)
                {
                    this.listMapFilePath.Add(dir + @"\" + listMapFilePath[i].Trim());
                }

                bool IsFileExists = false;
                for (int i = 0; i < senario.MapFilesCount; i++)
                {
                    IsFileExists |= File.Exists(this.listMapFilePath[i]);
                }
                if (IsFileExists)
                {
                    if (senario.MapFilesCount > 1)
                    {
                        btnBve5BootChooseMap.Enabled = btnBootBVE5.Enabled;
                        btnBve6BootChooseMap.Enabled = btnBootBVE6.Enabled;
                        strDisp += "マップファイルが複数あります。データ数：" + senario.MapFilesCount + "\n";
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
                    for (int i = 0; i < senario.MapFilesCount; i++)
                    {
                        this.listMapFilePath.Add("Not found or supported : " + dir + @"\" + listMapFilePath[i].Trim());
                    }
                    cbxMapFilePath.Text = "Not found or supported : " + dir + @"\" + listMapFilePath[0].Trim();
                    cbxMapFilePath.BackColor = Color.LightYellow;
                    strDisp += "マップファイルが見つかりません。\n";
                }
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

        private void OpenNewMapFile(string strMapFilePath_)
        {
            if (File.Exists(strMapFilePath_))
            {
                btnMapOpen.Enabled = true;
                btnMapDirectory.Enabled = true;
                int error = 0;
                cbxMapFilePath.Text = strMapFilePath_;
                cbxTrain.Items.Clear();
                List<string> listTrain = new List<string>();

                map = new Map(strMapFilePath);

                if (PathControl_Map(ref tbStructure, ref btnStructureOpen, ref btnStructureDirectory, map.Structure, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Map(ref tbStation, ref btnStationOpen, ref btnStationDirectory, map.Station, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Map(ref tbSignal, ref btnSignalOpen, ref btnSignalDirectory, map.Signal, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Map(ref tbSoundList, ref btnSoundListOpen, ref btnSoundListDirectory, map.SoundList, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Map(ref tbSound3DList, ref btnSound3DListOpen, ref btnSound3DListDirectory, map.Sound3DList, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Map(ref cbxTrain, ref btnTrainOpen, ref btnTrainDirectory, map.Train, out listTrain) <= 0)
                {
                    error++;
                }


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

        private bool IsDetailmanager32 = false;
        private bool IsDetailmanager64 = false;

        Vehicle vehicle;

        private void OpenNewVehicleFile(string _strVehicleFilePath)
        {
            if (File.Exists(_strVehicleFilePath))
            {
                btnOpenVehicleFile.Enabled = true;
                btnOpenVehicleDirectory.Enabled = true;
                btnOpenVehicleFile.Enabled = true;

                vehicle = new Vehicle(_strVehicleFilePath);

                int error = 0;

                strVehicleFilePath = _strVehicleFilePath;

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
                    if (File.Exists(senario.VehicleFilesAbs[cbxVehicleIndex] + @".bak"))
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
                        strAts32SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(vehicle.Ats32.FilePath) + @"\detailmodules.txt");

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
                if (PathControl_Vehicle(ref tbPerfoemanceCurve, ref btnPerfoemanceCurveOpen, ref btnPerfoemanceCurveDirectory, vehicle.PerformanceCurve, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Vehicle(ref tbParameters, ref btnParametersOpen, ref btnParametersDirectory, vehicle.Parameters, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Vehicle(ref tbPanel, ref btnPanelOpen, ref btnPanelDirectory, vehicle.Panel, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Vehicle(ref tbSound, ref btnSoundOpen, ref btnSoundDirectory, vehicle.Sound, out _) <= 0)
                {
                    error++;
                }
                if (PathControl_Vehicle(ref tbMotorNoise, ref btnMotorNoiseOpen, ref btnMotorNoiseDirectory, vehicle.MotorNoise, out _) <= 0)
                {
                    error++;
                }
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

                    if (vehicle.FileVersion >= 2.0 && strAts64DetailManagerFilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        //strDisp += "BVE6用(64bit)ATSプラグイン(DetailManager)が見つかりました\n";
                        strAts64SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(strAts64DetailManagerFilePath) + @"\detailmodules.txt");
                        tbAts64DetailModules.Text = strAts64SettingTextFilePath;
                        OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.BVE6);
                        IsDetailmanager64 = true;
                        btnAts64Open.Enabled = true;
                        btnAts64Open2.Enabled = true;
                        tbAts64DetailModules.Enabled = true;
                    }
                    else
                    {
                        strDisp += "BVE6用(64bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n";
                        IsDetailmanager64 = false;
                        btnAts64Open.Enabled = false;
                        btnAts64Open2.Enabled = true;
                        tbAts64DetailModules.Enabled = true;
                    }

                }

                if (error > 0)
                {
                    strDisp += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m\n";
                }

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
                    dataGridView[0, i].Style.BackColor = Color.LightGray;
                    dataGridView[1, i].Style.BackColor = Color.LightGray;
                    dataGridView[2, i].Style.BackColor = Color.LightGray;
                    dataGridView[3, i].Style.BackColor = Color.LightGray;
                }
            }
        }

        private void btnOpenRouteFile_Click(object sender, EventArgs e)
        {
            ProcessStart(senario.FilePath, false);
        }

        private void btnOpenVehicleFile_Click(object sender, EventArgs e)
        {
            ProcessStart(senario.VehicleFilesAbs[cbxVehicleIndex], false);
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
            ProcessStart(senario.VehicleFilesAbs[cbxVehicleIndex], true);
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
            btnOpenSenario.Enabled = true;
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

        private string GanarateRelativePath(string OriginalFilePath, string TargetPath)
        {
            string relativePath = "";
            if ((OriginalFilePath != "" || TargetPath != "") && (File.Exists(OriginalFilePath) && File.Exists(TargetPath)))
            {
                // 引用 https://dobon.net/vb/dotnet/file/getabsolutepath.html#uriencode

                //"%"を"%25"に変換しておく（デコード対策）
                string u1_Path = OriginalFilePath.Replace("%", "%25");
                string u2_Path = TargetPath.Replace("%", "%25");

                //相対パスを取得する
                Uri u1 = new Uri(u1_Path);
                Uri u2 = new Uri(u2_Path);

                Uri relativeUri = u1.MakeRelativeUri(u2);

                relativePath = relativeUri.ToString();

                //URLデコードする（エンコード対策）
                relativePath = Uri.UnescapeDataString(relativePath);

                //"%25"を"%"に戻す
                relativePath = relativePath.Replace("%25", "%");

                relativePath = relativePath.Replace('/', '\\');

            }

            return relativePath;
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("【説明】\r\n" +
                "Rock_On様作製のATSプラグイン(DetailManager)専用です\r\n" +
                "detailmodules.txtを書き換えたいときに使えるかと思います\r\n" +
                "\r\n" +

                "【つかいかた(車両ファイル確認)】\r\n" +
                "1.シナリオファイルを選択\r\n" +
                "2.車両ファイル関係タブを選択し、リンクを確認する\r\n" +
                "3.右上のBVE起動ボタンで選択路線や車両で運転ができます\r\n" +
                "\r\n" +

                "【つかいかた(車両ATSプラグイン関連)】\r\n" +
                "1.シナリオファイルを選択\r\n" +
                "2.車両ATSプラグイン関係タブを選択し、緑色枠内に移動する\r\n" +
                "3.車両ATSプラグインを選択して開く\r\n" +
                "4.ATSプラグインファイルパスを生成し、追記する(*.bakファイルを生成し復元可能になります)と、detailmodules内最下位にパスを生成します\r\r" +
                "5.車両ATSプラグイン関係タブ下部のdetailmodules.txtの内容を確認します\r\r" +
                "6.Drag&Dropで順序の入れ替え、Delキーで削除できます\r\r" +
                "7.保存する個で適用されます(初回のみ*.bakを生成し、復元可能になります)\r\r" +
                "\r\n" +
                "【できないこと】\r\n" +
                "1.DetailManager非対応のプラグインの適用\r\n" +
                "2.include形式ファイル、パス内変数形式\r\n" +
                "\r\n" +
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします！また、本ツールで「改造」したデータのアップロードは禁止です※\r\n" +
                "\r\n", "使い方");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TempFileChecker();
            Settings.Default.Save();
        }

        private void TempFileChecker()
        {
            if (senario != null)
            {
                if (File.Exists(senario.FilePath + ".tmp.txt"))
                {
                    File.Delete(senario.FilePath + ".tmp.txt");
                }
                if (File.Exists(senario.FilePath + ".bak"))
                {
                    File.Delete(senario.FilePath + ".bak");
                }
            }
        }

        /// <summary>
        /// プロセス開始メソッド
        /// </summary>
        /// <param name="FilePath">ファイルパス</param>
        /// <param name="IsDirectory">ディレクトリ指定か？</param>
        private void ProcessStart(string FilePath, bool IsDirectory)
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
                if (File.Exists(FilePath) && Directory.Exists(Path.GetDirectoryName(FilePath)))
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

        private void btnMapOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath, false);
        }

        private void btnPerfoemanceCurveOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.PerformanceCurve.FilePath, false);
        }


        private void btnPerfoemanceCurveDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.PerformanceCurve.FilePath, true);
        }

        private void btnParametersOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Parameters.FilePath, false);
        }

        private void btnParametersDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Parameters.FilePath, true);
        }

        private void btnPanelOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Panel.FilePath, false);
        }


        private void btnPanelDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Panel.FilePath, true);
        }

        private void btnSoundOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Sound.FilePath, false);
        }
        private void btnSoundDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.Sound.FilePath, true);
        }

        private void btnMotorNoiseOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.MotorNoise.FilePath, false);
        }

        private void btnMotorNoiseDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(vehicle.MotorNoise.FilePath, true);
        }

        private void btnAts64Open_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts64SettingTextFilePath, false);
        }

        private void btnMapOpen_Click_1(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath, false);
        }

        private void btnMapDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath, true);
        }

        private void btnStructureOpen_Click_1(object sender, EventArgs e)
        {
            ProcessStart(map.Structure.FilePath, false);
        }

        private void btnStructureDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Structure.FilePath, true);
        }

        private void btnStationOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Station.FilePath, false);
        }
        private void btnStationDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Station.FilePath, true);
        }
        private void btnSignalOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Signal.FilePath, false);
        }
        private void btnSignalDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Signal.FilePath, true);
        }

        private void btnSoundListOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(map.SoundList.FilePath, false);
        }
        private void btnSoundListDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.SoundList.FilePath, true);
        }

        private void btnTrainOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(cbxTrain.Text, false);
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
                s = Path.GetFullPath(Path.GetDirectoryName(_dirPath) + "\\" + (_line.Substring(index + 1, _line.IndexOf("'", index + 1) - index - 1)).Trim());
            }
            else if ((_line.IndexOf("(") > 0))
            {
                s = Path.GetFullPath(Path.GetDirectoryName(_dirPath) + "\\" + (_line.Substring(_line.IndexOf("(") + 1, _line.IndexOf(")", _line.IndexOf("(") + 1) - _line.IndexOf("(") - 1)).Trim());
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

        private int PathControl_Map(ref ComboBox cb, ref Button btnFile, ref Button btnDirectory, List<Contents_Map> c, out List<string> strFilePath_)
        {
            strFilePath_ = new List<string>();
            if (c != null)
            {
                for (int i = 0; i < c.Count; i++)
                {
                    strFilePath_.Add(c[i].FilePath);
                    cb.Items.Add(c[i].FilePath);
                }
                if (c.Count > 0)
                {
                    cb.Text = c[0].FilePath;
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
            btnOpenSenarioFile.Enabled = false;
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

            pictureBox1.Image = null;
            lblTitle.Text = "Title";
            lblRouteTitle.Text = "Route Title";
            lblVehicleTitle.Text = "Vehicle Title";
            lblAuthor.Text = "Author";
            tbComment.Text = "Comment";

            IsDetailmanager32 = false;
            IsDetailmanager64 = false;

            btnBve6Convert.Enabled = false;

            lblConvert.Text = "コンバート後、[BVE6 DetailModules]タブで読込状況を確認してください。";
            tbDetailModuleStoreDetail.Text = "推奨例:32bit DetailManager.dll格納フォルダ直下または同列に[x64]や[ATS64]フォルダを作成します";
            btnDetailModuleStoreClear.Enabled = false;
            btnDetailModuleStoreClear.Visible = false;

            btnBve5Recovery.Visible = false;

            btnOpenVehicleFile.BackColor = SystemColors.Control;

            //車両データDataGridViewクリア
            dgvPerformanceCurve.Columns.Clear();
            dgvParameters.Columns.Clear();
            dgvPanel.Columns.Clear();
            dgvSound.Columns.Clear();
            dgvMotorNoise.Columns.Clear();
            tabControlVehicle.SelectedTab = tpPerformanceCurve;

            //マップファイルDataGridViewクリア
            dgvStructure.Columns.Clear();
            dgvStation.Columns.Clear();
            dgvSignal.Columns.Clear();
            dgvSoundList.Columns.Clear();
            dgvSound3DList.Columns.Clear();
            tabControlMaps.SelectedTab = tpStructure;

            // 名前（"tpAtsPlugin"）でコントロールを探す
            var controls = tabControlSenario.Controls.Find("tpVehicle", false); // trueで子コントロールも検索

            if (controls.Length > 0)
            {
                tabControlSenario.SelectTab(tabControlSenario.TabPages["tpVehicle"]);
            }

        }

        private void btnBootBVE5_Click(object sender, EventArgs e)
        {
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + senario.FilePath + "\"");
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
                Process.Start(strBve6Path, "\"" + senario.FilePath + "\"");
            }
            else
            {
                MessageBox.Show("BVE6が見つかりません。BVEパスを設定してください。");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            if(Settings.Default.RouteFileDirectory != null && Settings.Default.RouteFileDirectory != "" && Directory.Exists(Settings.Default.RouteFileDirectory))
            {
                lvFiles.View = View.Details;
                lvFiles.Columns.Add("ファイル名", 220);
                //lvFiles.Columns.Add("パス",800);
                lvFiles.Columns.Add("車両データ", 65);
                lvFiles.Columns.Add("マップデータ", 65);
                lvFiles.Columns[1].TextAlign = HorizontalAlignment.Center;
                lvFiles.Columns[2].TextAlign = HorizontalAlignment.Center;
                tbSenarioDirectory.Text = Settings.Default.RouteFileDirectory;
                OpenSenaroDirectory(Settings.Default.RouteFileDirectory);
            }
            if(Settings.Default.RouteFileDirectory == null || Settings.Default.RouteFileDirectory == "" || Directory.Exists(Settings.Default.RouteFileDirectory))
            {
                string tempDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\BveTs\Scenarios";
                if (Directory.Exists(tempDir))
                {
                    tbSenarioDirectory.Text = tempDir;
                    OpenSenaroDirectory(tempDir);
                    Settings.Default.RouteFileDirectory = tempDir;
                    Settings.Default.Save();

                }
            }
            MessageBox.Show("※動作保証なし、データの破損等いかなる責任も負いかねます※\r\n\r\n※車両や路線データの改造に該当すると考えられますので、自己責任かつ個人使用の範囲内でお願いします※");
        }

        private void btnBackUp_Click(object sender, EventArgs e)
        {
            Make_NewVehicleFile(@senario.FilePath, @senario.FilePath + ".tmp.txt");
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
                if (lines[i].IndexOf(";") != 0 && lines[i].IndexOf("#") != 0)
                {
                    if (lines[i].Contains("Vehicle =") || lines[i].Contains("Vehicle="))
                    {
                        lines[i] = "Vehicle = " + GanarateRelativePath(oldFileName_, cbxVehicle.Text);
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
                if (lines[i].IndexOf(";") != 0 && lines[i].IndexOf("#") != 0)
                {
                    if (lines[i].Contains("Route =") || lines[i].Contains("Route="))
                    {
                        lines[i] = "Route = " + GanarateRelativePath(oldFileName_, cbxVehicle.Text);
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
                    string strAts32Path = Path.GetFullPath(Path.GetDirectoryName(strAts32SettingTextFilePath) + @"\" + lines[i]);
                    //mikan-go-go様 Plugin対応判定
                    if (lines[i].IndexOf(@"\GeneralAtsPlugin\Rock_On", StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        string[] keyword = { "Rock_On" };
                        string[] arr = strAts32Path.Split(keyword, StringSplitOptions.None);
                        string strAts64Path = arr[0] + keyword[0] + @"\x64" + arr[1];
                        if (File.Exists(strAts64Path) && (AtsPluginChecker(strAts64Path, 300, false) == BVE_Version.BVE6))
                        {
                            lines[i] = GanarateRelativePath(strAts64SettingTextFilePath, strAts64Path);
                        }
                        else
                        {
                            lines[i] = "#" + GanarateRelativePath(strAts64SettingTextFilePath, strAts32Path);
                        }
                    }
                    else
                    {
                        lines[i] = "#" + GanarateRelativePath(strAts64SettingTextFilePath, strAts32Path);
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
            string strNewRouteFile = senario.FilePath + ".tmp.txt";
            Make_NewVehicleFile(@senario.FilePath, strNewRouteFile);
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
            string strNewRouteFile = senario.FilePath + ".tmp.txt";
            Make_NewVehicleFile(@senario.FilePath, strNewRouteFile);
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
            string strNewRouteFile = senario.FilePath + ".tmp.txt";
            make_newMapFile(@senario.FilePath, strNewRouteFile);
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
            string strNewRouteFile = senario.FilePath + ".tmp.txt";
            make_newMapFile(@senario.FilePath, strNewRouteFile);
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

            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(senario.VehicleFilesAbs[cbxVehicleIndex]);
        }

        private void btnAts32Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle.Ats32.FilePath, 300, true);
            /*if (IsDetailmanager32)
            {
                if (MessageBox.Show("DetailModulesの内容を確認しますか？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // 名前（"tpAtsPlugin"）でコントロールを探す
                    var controls = tabControl1.Controls.Find("tpAtsPlugin", false); // trueで子コントロールも検索

                    if (controls.Length > 0)
                    {
                        tabControl1.SelectTab(tabControl1.TabPages["tpAtsPlugin"]);
                        tabControl2.SelectTab(tabControl2.TabPages["tpAts32"]);
                    }
                }

            }*/
        }

        private void btnAts64Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(vehicle.Ats64.FilePath, 300, true);
            /*if (IsDetailmanager64)
            {
                if (MessageBox.Show("DetailModulesの内容を確認しますか？", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // 名前（"tpAtsPlugin"）でコントロールを探す
                    var controls = tabControl1.Controls.Find("tpAtsPlugin", false); // trueで子コントロールも検索

                    if (controls.Length > 0)
                    {
                        tabControl1.SelectTab(tabControl1.TabPages["tpAtsPlugin"]);
                        tabControl2.SelectTab(tabControl2.TabPages["tpAts64"]);
                    }
                }
            }*/
        }

        private BVE_Version AtsPluginChecker(string _FilePath, int _BufferSize, bool IsDisplayChecked)
        {
            //MessageBox.Show($"hit"+" AtsPluginChecker\r\n"+_FilePath);
            AtsPlugin atsPlugin = new AtsPlugin();
            return atsPlugin.Checker(_FilePath, _BufferSize, IsDisplayChecked);
        }

        private void btnAts32RPathGen_Click(object sender, EventArgs e)
        {
            tbAts32RelatePath.Text = GanarateRelativePath(strAts32SettingTextFilePath, strAtsPluginFilePath);
            if (tbAts32RelatePath.Text != "")
            {
                tbAts32RelatePath.Enabled = true;
                btnAts32Add.Enabled = true;
            }
        }

        private void btnAts64RPathGen_Click(object sender, EventArgs e)
        {
            tbAts64RelatePath.Text = GanarateRelativePath(strAts64SettingTextFilePath, strAtsPluginFilePath);
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
                OpenNewVehicleFile(senario.VehicleFilesAbs[cbxVehicleIndex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
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
                            File.AppendAllText(strVehicleFilePath, Environment.NewLine + "Ats32 = " + GanarateRelativePath(strVehicleFilePath, strAts32DetailManagerFilePath) + Environment.NewLine + "Ats64 = " + GanarateRelativePath(strVehicleFilePath, strAts64DetailManagerFilePath), enc);
                        }
                        else
                        {
                            File.AppendAllText(strVehicleFilePath, Environment.NewLine + line, enc);
                        }
                    }
                }
            }
            Make_NewDetailModulesFile(strAts32SettingTextFilePath, strAts64SettingTextFilePath);
            OpenNewVehicleFile(senario.VehicleFilesAbs[cbxVehicleIndex]);

        }

        private void btnBve5Recovery_Click(object sender, EventArgs e)
        {
            bool exist = false;
            if (File.Exists(senario.VehicleFilesAbs[cbxVehicleIndex] + @".bak"))
            {
                File.Copy(senario.VehicleFilesAbs[cbxVehicleIndex] + @".bak", senario.VehicleFilesAbs[cbxVehicleIndex], true);
                exist = true;
            }
            if (exist)
            {
                File.Delete(strVehicleFilePath + @".bak");
                btnBve5Recovery.Visible = false;
                lblConvert.Text = "コンバート後、[BVE6 DetailModules]タブで読込状況を確認してください。";
                dgvAts64.Rows.Clear();
                OpenNewVehicleFile(senario.VehicleFilesAbs[cbxVehicleIndex]);
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
            ProcessStart(map.Sound3DList.FilePath, false);
        }

        private void btnSound3DListDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(map.Sound3DList.FilePath, true);
        }

        private void tsmi32Delete_Click(object sender, EventArgs e)
        {
            if (tabControl2.SelectedIndex == 0)
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


        private Rectangle dragBoxFromMouseDown32;      // 座標用
        private int rowIndexFromMouseDown32;           // 移動元Index用
        private int rowIndexOfItemUnderMouseToDrop32; // 移動先Index用

        private Rectangle dragBoxFromMouseDown64;      // 座標用
        private int rowIndexFromMouseDown64;           // 移動元Index用
        private int rowIndexOfItemUnderMouseToDrop64; // 移動先Index用

        private void dgvAts32_MouseMove(object sender, MouseEventArgs e)
        {
            // 左クリックの場合
            if (e.Button == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown32 != Rectangle.Empty && !(dragBoxFromMouseDown32.Contains(e.X, e.Y)))
                {
                    DragDropEffects dropEffect = dgvAts32.DoDragDrop(dgvAts32.Rows[rowIndexFromMouseDown32], DragDropEffects.Move);
                }
            }
        }

        private void dgvAts32_MouseDown(object sender, MouseEventArgs e)
        {
            rowIndexFromMouseDown32 = dgvAts32.HitTest(e.X, e.Y).RowIndex;

            // ヘッダー以外
            if (rowIndexFromMouseDown32 > -1)
            {
                var dragSize = SystemInformation.DragSize;
                // ドラッグ操作が開始されない範囲を取得
                dragBoxFromMouseDown32 = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
            }
            else
            {
                dragBoxFromMouseDown32 = Rectangle.Empty;
            }
        }

        private void dgvAts32_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvAts32_DragDrop(object sender, DragEventArgs e)
        {
            // データグリッドのポイントを取得
            Point clientPoint = dgvAts32.PointToClient(new Point(e.X, e.Y));
            // 移動先INDEX
            rowIndexOfItemUnderMouseToDrop32 = dgvAts32.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // ドラッグ＆ドロップ効果【移動】の場合・INDEX範囲内の場合
            if (e.Effect == DragDropEffects.Move &&
               rowIndexOfItemUnderMouseToDrop32 > -1)
            {
                // 移動データ退避
                //データグリッドビューの列コピー
                DataTable Dt = new DataTable();

                DataGridViewClone(Dt, dgvAts32);

                //データグリッドビューをテーブルに取得
                Dt = retDtgrdvwValue(dgvAts32, Dt);
                if (rowIndexFromMouseDown32 < (dgvAts32.RowCount - 1))
                {
                    Object[] rowArray = Dt.Rows[rowIndexFromMouseDown32].ItemArray;
                    //DataRow row = Dt.NewRow();
                    //row.ItemArray = rowArray;

                    // 移動元削除
                    dgvAts32.Rows.RemoveAt(rowIndexFromMouseDown32);
                    // 移動先新規行挿入
                    dgvAts32.Rows.Insert(rowIndexOfItemUnderMouseToDrop32, rowArray);

                    btnAts32Reset.Visible = true;
                    btnAts32Save.Visible = true;
                    ColoringDataGridView(dgvAts32, BVE_Version.BVE5);
                }
            }
        }

        private void dgvAts64_MouseMove(object sender, MouseEventArgs e)
        {
            // 左クリックの場合
            if (e.Button == MouseButtons.Left)
            {
                if (dragBoxFromMouseDown64 != Rectangle.Empty && !(dragBoxFromMouseDown64.Contains(e.X, e.Y)))
                {
                    DragDropEffects dropEffect = dgvAts64.DoDragDrop(dgvAts32.Rows[rowIndexFromMouseDown64], DragDropEffects.Move);
                }
            }
        }

        private void dgvAts64_MouseDown(object sender, MouseEventArgs e)
        {
            rowIndexFromMouseDown64 = dgvAts64.HitTest(e.X, e.Y).RowIndex;

            // ヘッダー以外
            if (rowIndexFromMouseDown64 > -1)
            {
                var dragSize = SystemInformation.DragSize;
                // ドラッグ操作が開始されない範囲を取得
                dragBoxFromMouseDown64 = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
            }
            else
            {
                dragBoxFromMouseDown64 = Rectangle.Empty;
            }
        }

        private void dgvAts64_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvAts64_DragDrop(object sender, DragEventArgs e)
        {
            // データグリッドのポイントを取得
            Point clientPoint = dgvAts64.PointToClient(new Point(e.X, e.Y));
            // 移動先INDEX
            rowIndexOfItemUnderMouseToDrop64 = dgvAts64.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // ドラッグ＆ドロップ効果【移動】の場合・INDEX範囲内の場合
            if (e.Effect == DragDropEffects.Move &&
               rowIndexOfItemUnderMouseToDrop64 > -1)
            {
                // 移動データ退避
                //データグリッドビューの列コピー
                DataTable Dt = new DataTable();

                DataGridViewClone(Dt, dgvAts64);

                //データグリッドビューをテーブルに取得
                Dt = retDtgrdvwValue(dgvAts64, Dt);

                if (rowIndexFromMouseDown64 < (dgvAts64.RowCount - 1))
                {
                    Object[] rowArray = Dt.Rows[rowIndexFromMouseDown64].ItemArray;
                    //DataRow row = Dt.NewRow();
                    //row.ItemArray = rowArray;

                    // 移動元削除
                    dgvAts64.Rows.RemoveAt(rowIndexFromMouseDown64);
                    // 移動先新規行挿入
                    dgvAts64.Rows.Insert(rowIndexOfItemUnderMouseToDrop64, rowArray);

                    btnAts64Reset.Visible = true;
                    btnAts64Save.Visible = true;
                    ColoringDataGridView(dgvAts64, BVE_Version.BVE6);
                }
            }
        }

        private DataTable DataGridViewClone(DataTable dt, DataGridView targetDtgrdvw)
        {
            //カラム件数分繰り返す
            for (int i = 0; i < targetDtgrdvw.ColumnCount; i++)
            {
                dt.Columns.Add(targetDtgrdvw.Columns[i].HeaderText);
            }

            return dt;
        }
        public DataTable retDtgrdvwValue(DataGridView dtgrdvwTarget, DataTable dtResult)
        {
            for (int row = 0; row < dtgrdvwTarget.Rows.Count - 1; row++)
            {
                DataRow drResult = dtResult.NewRow();
                for (int col = 0; col < dtgrdvwTarget.Columns.Count; col++)
                {
                    drResult[col] = dtgrdvwTarget.Rows[row].Cells[col].Value;
                }
                dtResult.Rows.InsertAt(drResult, row);
            }
            return dtResult;
        }

        private void btnAts32Reset_Click(object sender, EventArgs e)
        {
            OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.BVE5);
            btnAts32Reset.Visible = false;
            btnAts32Save.Visible = false;
        }

        private void btnAts64Reset_Click(object sender, EventArgs e)
        {
            OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.BVE6);
            btnAts64Reset.Visible = false;
            btnAts64Save.Visible = false;
        }

        private void btnAts32Save_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("BVE5(32bit)用 detailmodules.txtを書き換えます。続行しますか？", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                if (!File.Exists(strAts32SettingTextFilePath + ".bak"))
                {
                    File.Copy(strAts32SettingTextFilePath, strAts32SettingTextFilePath + ".bak", true);
                    btnAts32Recovery.Visible = true;

                }
                btnAts32Reset.Visible = false;
                btnAts32Save.Visible = false;
                string str = "";

                for (int i = 0; i < dgvAts32.RowCount; i++)
                {
                    str += dgvAts32[2, i].Value + Environment.NewLine;
                }

                File.WriteAllText(strAts32SettingTextFilePath, str);
            }
        }
        private void btnAts64Save_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("BVE6(64bit)用 detailmodules.txtを書き換えます。\r\n\r\n続行しますか？", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                if (!File.Exists(strAts64SettingTextFilePath + ".bak"))
                {
                    File.Copy(strAts64SettingTextFilePath, strAts64SettingTextFilePath + ".bak", true);
                    btnAts64Recovery.Visible = true;
                }
                btnAts64Reset.Visible = false;
                btnAts64Save.Visible = false;
                string str = "";

                for (int i = 0; i < dgvAts64.RowCount; i++)
                {
                    str += dgvAts64[2, i].Value + Environment.NewLine;
                }

                File.WriteAllText(strAts64SettingTextFilePath, str);
            }
        }
        private void dgvAts32_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (dgvAts32.RowCount > 0)
            {
                btnAts32Reset.Visible = true;
                btnAts32Save.Visible = true;
            }
        }

        private void dgvAts64_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            if (dgvAts64.RowCount > 0)
            {
                btnAts64Reset.Visible = true;
                btnAts64Save.Visible = true;
            }
        }

        private void btnAts32Recovery_Click(object sender, EventArgs e)
        {
            if (File.Exists(strAts32SettingTextFilePath + ".bak"))
            {
                File.Copy(strAts32SettingTextFilePath + ".bak", strAts32SettingTextFilePath, true);
                File.Delete(strAts32SettingTextFilePath + ".bak");
            }
            btnAts32Reset.Visible = false;
            btnAts32Save.Visible = false;
            btnAts32Recovery.Visible = false;
            OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.BVE5);
        }

        private void btnAts64Recovery_Click(object sender, EventArgs e)
        {
            if (File.Exists(strAts64SettingTextFilePath + ".bak"))
            {
                File.Copy(strAts64SettingTextFilePath + ".bak", strAts64SettingTextFilePath, true);
                File.Delete(strAts64SettingTextFilePath + ".bak");
            }
            btnAts64Reset.Visible = false;
            btnAts64Save.Visible = false;
            btnAts64Recovery.Visible = false;
            OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.BVE6);
        }

        private void btnSenarioClear_Click(object sender, EventArgs e)
        {
            Reset();
            btnSenarioClear.Visible = false;
        }

        private void dgvAts32_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.BVE5);
            ColoringDataGridView(dgvAts32, BVE_Version.BVE5);
            btnAts32Reset.Visible = true;
            btnAts32Save.Visible = true;
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
                OpenSenaroDirectory(selectedFolderPath);
            }
        }

        private void OpenSenaroDirectory(string selectedFolderPath)
        {
            if (Directory.Exists(selectedFolderPath))
            {
                btnOpenSenarioDirectory.Enabled = true;
                // リストビューをクリア
                lvFiles.Items.Clear();

                try
                {
                    // フォルダ内のファイルパスを取得
                    string[] files = System.IO.Directory.GetFiles(selectedFolderPath);
                    tbSenarioDirectory.Text = selectedFolderPath;
                    Settings.Default.RouteFileDirectory = selectedFolderPath;
                    Settings.Default.Save();

                    int i = 0;

                    // ファイル名をリストビューに列挙
                    foreach (string file in files)
                    {

                        // ファイル名のみ取得
                        string fileName = Path.GetFileName(file);
                        // ファイルのフルパスを取得
                        string fullPath = file;

                        Senario sn = new Senario(fullPath);


                        // ListViewItemを作成し、サブアイテム（パス）を追加
                        ListViewItem item = new ListViewItem(fileName);
                        //item.SubItems.Add(fullPath);
                        item.SubItems.Add(sn.VehicleFilesExistsCount.ToString());
                        item.SubItems.Add(sn.MapFilesCount.ToString());

                        lvFiles.Items.Add(item); // listViewFiles を lvFiles に変更
                        if (sn.VehicleFilesExistsCount == 0 && sn.MapFilesCount == 0)
                        {
                            lvFiles.Items[i].BackColor = Color.DarkGray;
                        }
                        else if (sn.VehicleFilesExistsCount == 0)
                        {
                            lvFiles.Items[i].BackColor = Color.LightYellow;
                        }
                        else if (sn.MapFilesCount == 0)
                        {
                            lvFiles.Items[i].BackColor = Color.LightGray;
                        }
                        else
                        {
                            Vehicle vehicle = new Vehicle(sn.VehicleFilesAbs[0]);
                            if(vehicle.FileVersion == 2.0)
                            {
                                lvFiles.Items[i].BackColor = Color.Thistle;
                            }
                            else
                            {
                                lvFiles.Items[i].BackColor = Color.PaleGreen;
                            }
                        }
                        i++;
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ファイルの取得中にエラーが発生しました: " + ex.Message);
                    btnOpenSenarioDirectory.Enabled = false;
                }
            }
            else
            {
                btnOpenSenarioDirectory.Enabled = false;
            }
        }

        /////
        /// <summary>
        /// 左リストビュー選択イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択されているアイテムがあるか確認
            if (lvFiles.SelectedItems.Count > 0)
            {
                Reset();
                // 最初に選択されたアイテムを取得
                ListViewItem selectedItem = lvFiles.SelectedItems[0];

                // SubItems[1] がファイルパスです（SubItems[0]はファイル名）。
                string fullPath = tbSenarioDirectory.Text + @"\" + selectedItem.SubItems[0].Text;

                senario = new Senario(fullPath);
                // メッセージボックスでフルパスを表示
                OpenSenario();
            }
        }

        private void btnOpenSenarioDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(tbSenarioDirectory.Text, true);
        }

        private void btnSenarioReload_Click(object sender, EventArgs e)
        {
            Reset();
            OpenSenario();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // 3. Ctrl + T が押されたか判定
            if (e.Control && e.KeyCode == Keys.T)
            {

                // イベントを処理済みにして、他のコントロールに伝播させない
                e.Handled = true;
                e.SuppressKeyPress = true;

                gbxBve6Converter.Visible = true;
            }
        }

        private void tabControlMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlMaps.SelectedTab;
            // タブのテキストやインデックスで処理を分岐
            if (currentTab.Name == "tpStation")
            {
                if (map != null && File.Exists(map.Station.FilePath))
                {
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(map.Station.FilePath))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    dt = new DataTable();
                    using (TextFieldParser parser = new TextFieldParser(map.Station.FilePath, enc))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(","); // カンマ区切り

                        //1行目読み飛ばし
                        if (!parser.EndOfData)
                        {
                            parser.ReadLine();
                        }

                        string[] strs = { "stationKey", "stationName", "arrivalTime", "depertureTime", "stoppageTime", "defaultTime", "signalFlag", "alightingTime", "passengers", "arrivalSoundKey", "depertureSoundKey", "doorReopen", "stuckInDoor" };
                        foreach (string field in strs)
                        {
                            dt.Columns.Add(field);
                        }

                        // データ行の読み込み
                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            string[] subset = new string[2];
                            //if (!fields[0].Contains("#") && !fields[0].Contains(";") && !fields[0].Contains("//") && fields[0].Length > 0)
                            //{
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
            else if (currentTab.Name == "tpSignal")
            {
                if (map != null && map.Signal != null && System.IO.File.Exists(map.Signal.FilePath))
                {
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(map.Signal.FilePath))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    dt = new DataTable();
                    int colnum = 0;
                    //一旦最大列数をカウント
                    using (TextFieldParser parser = new TextFieldParser(map.Signal.FilePath, enc))
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

                            if(fields.Length > colnum)
                            {
                                colnum = fields.Length;
                            }

                        }

                    }
                    //データ配置
                    using (TextFieldParser parser = new TextFieldParser(map.Signal.FilePath, enc))
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
            else if (currentTab.Name == "tpSoundList")
            {
                if (map != null && map.SoundList != null && File.Exists(map.SoundList.FilePath))
                {
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(map.SoundList.FilePath))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    dt = new DataTable();
                    using (TextFieldParser parser = new TextFieldParser(map.SoundList.FilePath, enc))
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
                                if(flgFound && flgFoundIndex > i)
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
                    dgvSoundList.Columns[1].HeaderCell.ToolTipText = "wavファイルの相対パス(クリックで開きます)";
                    dgvSoundList.Columns[2].HeaderCell.ToolTipText = "その音を同時に再生できる数。省略した場合は 1 になります。";

                    for (int i = 0; i < dgvSoundList.Rows.Count - 1; i++)
                    {
                        /*if (bools[i])
                        {
                            dgvSoundList.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else
                        {*/
                        string cellValue = dgvSoundList.Rows[i].Cells[1].Value.ToString();
                        if (!File.Exists($@"{Path.GetDirectoryName(map.SoundList.FilePath)}\{cellValue}") && cellValue != "")
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
            else if (currentTab.Name == "tpSound3D")
            {
                if (map != null && map.Sound3DList != null && File.Exists(map.Sound3DList.FilePath))
                {

                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(map.Sound3DList.FilePath))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    dt = new DataTable();
                    using (TextFieldParser parser = new TextFieldParser(map.Sound3DList.FilePath, enc))
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
                                if(flgFound && flgFoundIndex > i)
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
                    dgvSound3DList.Columns[1].HeaderCell.ToolTipText = "wavファイルの相対パス(クリックで開きます)";
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
                        if (!File.Exists($@"{Path.GetDirectoryName(map.Sound3DList.FilePath)}\{cellValue}") && cellValue != "")
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
            // タブのテキストやインデックスで処理を分岐
            else if (currentTab.Name == "tpTrain")
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
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(cbxTrain.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }
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
            }
        }

        private void dgvSoundList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvSoundList.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string soundFilePath = $@"{Path.GetDirectoryName(map.SoundList.FilePath)}\{cellValue}";
                if (cellValue != "" && File.Exists(soundFilePath))
                {
                    Process.Start(soundFilePath);
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlSenario.SelectedTab;
            // タブのテキストやインデックスで処理を分岐
            if (currentTab.Name == "tpRoute")
            {
                if (map != null && map.Structure != null && File.Exists(map.Structure.FilePath))
                {
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(map.Structure.FilePath))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }

                    dt = new DataTable();
                    using (TextFieldParser parser = new TextFieldParser(map.Structure.FilePath, enc))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(","); // カンマ区切り

                        //1行目読み飛ばし
                        if (!parser.EndOfData)
                        {
                            parser.ReadLine();
                        }

                        string[] strs = { "StructureNae", "FilePath" ,"Remarks"};
                        foreach (string field in strs)
                        {
                            dt.Columns.Add(field);
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
                                if (flgFound && i > flgFoundIndex )
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
                    string[] tttStructures = { "任意の文字列。このストラクチャー名は、マップファイル、信号現示リストファイル、他列車ファイルで使用します。", "ストラクチャーファイルの相対パス。" ,"備考"};
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        dgvStructure.Columns[i].HeaderCell.ToolTipText = tttStructures[i];
                    }

                    for (int i = 0; i < dgvStructure.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvStructure.Rows[i].Cells[1].Value.ToString();
                        //MessageBox.Show($@"{Path.GetDirectoryName(map.FilePath)}\{cellValue}");
                        if (!File.Exists($@"{Path.GetDirectoryName(map.Structure.FilePath)}\{cellValue}") && cellValue != "")
                        {
                            dgvStructure.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                        }
                        cellValue = dgvStructure.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#")|| cellValue.Contains(";") || cellValue.Contains("//"))
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
        }

        private void dgvStructure_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvStructure.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string structureFilePath = $@"{Path.GetDirectoryName(map.Structure.FilePath)}\{cellValue}";
                if (cellValue != "" && File.Exists(structureFilePath))
                {
                    Process.Start(structureFilePath);
                }
            }
        }

        private void dgvPerformanceCurve_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvPerformanceCurve.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string FilePath = $@"{Path.GetDirectoryName(tbPerfoemanceCurve.Text)}\{cellValue}";
                if (cellValue != "" && File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
            }
        }

        private void tabControlVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 新しく選択されたTabPageを取得
            TabPage currentTab = tabControlVehicle.SelectedTab;
            // タブのテキストやインデックスで処理を分岐
            if (currentTab.Name == "tpParameters")
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
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(tbParameters.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }
                    // 2. ファイルを読み込む
                    string[] lines = File.ReadAllLines(tbParameters.Text, enc);

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
                    dgvParameters.DataSource = dt;
                    if (dgvParameters.Rows.Count > 0)
                    {
                        // 最初の行(インデックス0)を削除
                        dgvParameters.Rows.RemoveAt(0);
                    }
                    //ソート禁止
                    foreach (DataGridViewColumn c in dgvParameters.Columns)
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    //色付け
                    for (int i = 0; i < dgvParameters.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvParameters.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                        {
                            dgvParameters.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else if (cellValue == "")
                        {
                            dgvParameters.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else if (cellValue.Contains("["))
                        {
                            dgvParameters.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        }

                    }
                }
            }

            // タブのテキストやインデックスで処理を分岐
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
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(tbPanel.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }
                    // 2. ファイルを読み込む
                    string[] lines = File.ReadAllLines(tbPanel.Text, enc);

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
                                    dt.Rows.Add(parts[0].Trim(), parts3[0].Trim(),"",parts3[1].Trim());
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
                                dt.Rows.Add(parts2[0].Trim(), "","", parts2[1].Trim());
                            }
                        }
                    }

                    // 4. DataGridViewにデータをセット
                    /////
                    dgvPanel.DataSource = dt;
                    if (dgvPanel.Rows.Count > 0)
                    {
                        // 最初の行(インデックス0)を削除
                        dgvPanel.Rows.RemoveAt(0);
                    }
                    //ソート禁止
                    foreach (DataGridViewColumn c in dgvPanel.Columns)
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    //色付け
                    for (int i = 0; i < dgvPanel.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvPanel.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                        {
                            dgvPanel.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else if (cellValue == "")
                        {
                            dgvPanel.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else if (cellValue.Contains("["))
                        {
                            dgvPanel.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        }

                    }
                }
            }
            // タブのテキストやインデックスで処理を分岐
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
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(tbSound.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }
                    // 2. ファイルを読み込む
                    string[] lines = File.ReadAllLines(tbSound.Text, enc);

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
                    dgvSound.DataSource = dt;
                    if (dgvSound.Rows.Count > 0)
                    {
                        // 最初の行(インデックス0)を削除
                        dgvSound.Rows.RemoveAt(0);
                    }
                    //ソート禁止
                    foreach (DataGridViewColumn c in dgvSound.Columns)
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    //色付け
                    for (int i = 0; i < dgvSound.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvSound.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                        {
                            dgvSound.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else if (cellValue == "")
                        {
                            dgvSound.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else if (cellValue.Contains("["))
                        {
                            dgvSound.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        }

                    }
                }
            }
            // タブのテキストやインデックスで処理を分岐
            else if (currentTab.Name == "tpMorotNoise")
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
                    // Shift_JISを指定して読み込む場合
                    Encoding enc = Encoding.GetEncoding("Shift_JIS");

                    //文字エンコードを確認
                    using (StreamReader sr_temp = new StreamReader(tbMotorNoise.Text))
                    {
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                        {
                            enc = Encoding.GetEncoding("utf-8");
                        }
                    }
                    // 2. ファイルを読み込む
                    string[] lines = File.ReadAllLines(tbMotorNoise.Text, enc);

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
                    dgvMotorNoise.DataSource = dt;
                    //ソート禁止
                    foreach (DataGridViewColumn c in dgvMotorNoise.Columns)
                        c.SortMode = DataGridViewColumnSortMode.NotSortable;

                    //色付け
                    for (int i = 0; i < dgvMotorNoise.Rows.Count - 1; i++)
                    {
                        string cellValue = dgvMotorNoise.Rows[i].Cells[0].Value.ToString();
                        if (cellValue.Contains("#") || cellValue.Contains(";") || cellValue.Contains("//"))
                        {
                            dgvMotorNoise.Rows[i].DefaultCellStyle.BackColor = Color.LightBlue;
                        }
                        else if (cellValue == "")
                        {
                            dgvMotorNoise.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                        }
                        else if (cellValue.Contains("["))
                        {
                            dgvMotorNoise.Rows[i].DefaultCellStyle.BackColor = Color.LightGreen;
                        }

                    }
                }
            }
        }

        private void dgvPanel_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvPanel.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string FilePath = $@"{Path.GetDirectoryName(tbPanel.Text)}\{cellValue}";
                if (cellValue != "" && File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
            }
        }

        private void dgvSound_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvSound.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string FilePath = $@"{Path.GetDirectoryName(tbSound.Text)}\{cellValue}";
                if (cellValue != "" && File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
            }
        }

        private void dgvMotorNoise_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダー（行インデックス < 0）を除外
            if (e.RowIndex < 0) return;

            // クリックされたセルが特定列（例：列インデックス0）か判断
            if (e.ColumnIndex == 1)
            {
                string cellValue = dgvMotorNoise.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                string FilePath = $@"{Path.GetDirectoryName(tbMotorNoise.Text)}\{cellValue}";
                if (cellValue != "" && File.Exists(FilePath))
                {
                    Process.Start(FilePath);
                }
            }
        }
    }
 
}    
