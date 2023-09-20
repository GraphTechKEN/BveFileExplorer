using BveFileExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtsPluginEditor
{
    public enum BVE_Version
    {
        NotFound,
        Ver5,
        Ver6,
        Null,
        Comment,
    }

    public partial class Form1 : Form
    {
        string systemDrive = Environment.GetEnvironmentVariable("SystemDrive");
        string currentDirectry = Environment.CurrentDirectory;
        public Form1()
        {
            InitializeComponent();
        }


        private string strRouteFilePath = "";
        private string strVehicleFilePath = "";
        private List<string> listVehicleFilePath = new List<string>();
        private List<string> listMapFilePath = new List<string>();
        private List<string> listVehicle = new List<string>();
        private string strAts32DetailManagerFilePath = "";
        private string strAts32SettingTextFilePath = "";
        private string strAtsPluginFilePath = "";
        private string strMapFilePath = "";
        private string strPerfoemanceCurveFilePath = "";
        private string strParametersFilePath = "";
        private string strPanelFilePath = "";
        private string strSoundFilePath = "";
        private string strMotorNoiseFilePath = "";
        private string strAts64DetailManagerFilePath = "";
        private string strAts64SettingTextFilePath = "";
        private string strStructureFilePath = "";
        private string strStationFilePath = "";
        private string strSignalFilePath = "";
        private string strSoundListFilePath = "";
        private string strTrainFilePath = "";
        private bool flgAtsPluginFileOpen = false;
        private bool flgAtsPluginDirectoryOpen = false;
        private int cbxVehicleIndex = 0;
        private int cbxMapIndex = 0;
        private string strBve5Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\mackoy\BveTs5\bvets.exe";
        private string strBve6Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\mackoy\BveTs6\bvets.exe";

        private string strDisp = "";



        private void btnOpenSenario_Click(object sender, EventArgs e)
        {
            TempFileChecker();
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "路線ファイル(*.txt)|*.txt";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.InitialDirectory = Settings.Default.RouteFileDirectory;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Reset();

                Settings.Default.RouteFileDirectory = Path.GetDirectoryName(ofd.FileName);
                //OKボタンがクリックされたとき、選択されたファイルを読み取り専用で開く
                Stream stream;
                stream = ofd.OpenFile();
                strRouteFilePath = ofd.FileName;
                btnOpenSenarioFile.Enabled = true;
                btnBootBVE5.Enabled = true;
                tbSeinarioFileName.Enabled = true;

                tbSeinarioFileName.Text = ofd.FileName;

                //内容を読み込み、表示する
                StreamReader sr_temp = new StreamReader(stream);
                stream = ofd.OpenFile();
                StreamReader sr;
                if (sr_temp.ReadLine().IndexOf("shift_jis") > 0)
                {
                    sr = new StreamReader(stream, Encoding.GetEncoding("shift_jis"));
                }
                else
                {
                    sr = new StreamReader(stream);
                }
                if (stream != null)
                {
                    string line = "";
                    bool flgErrVehicle = false;
                    bool flgErrMap = false;
                    List<string> _listVehicleFilePath = new List<string>();
                    List<string> _listMapFilePath = new List<string>();
                    string path = "";
                    string dir = Path.GetDirectoryName(strRouteFilePath);
                    txbLog.AppendText("路線ファイル：" + strRouteFilePath + "\r\n");

                    //最後まで読込
                    while ((line = sr.ReadLine()) != null)
                    {
                        //読込ログに追記
                        txbLog.AppendText(line + "\r\n");
                        //余計な文字列をトリム
                        line = line.Trim();

                        //先頭文字が「;」と「#」でないとき
                        if ((!line.StartsWith(";") || !line.StartsWith("#")) && line.Contains("="))
                        {
                            string item = line.Substring(0, line.IndexOf("=")).Trim();
                            //MessageBox.Show(item);
                            switch (item)
                            {
                                case "VehicleTitle"://車両タイトル
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblVehicleTitle.Text = line;
                                    }
                                    break;

                                case "Vehicle"://車両ファイル

                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        //文字列に「|」があるとき
                                        if (line.IndexOf("|") > 0)
                                        {
                                            path = line.Substring(0, line.IndexOf("|"));
                                            //listPath.Add(path.Substring(line.IndexOf("=") + 1));
                                            string temp_line = line.Substring(line.IndexOf("=") + 1);
                                            //"|"が文字列に無くなるまで
                                            while (temp_line.IndexOf("|") > 0)
                                            {
                                                //パスを仮文字列から「|」まで切り抜き
                                                path = temp_line.Substring(0, temp_line.IndexOf("|"));
                                                //もし「*」が含まれていたらそこまで切り抜く処理
                                                if (path.IndexOf("*") > 0)
                                                {
                                                    path = temp_line.Substring(0, temp_line.IndexOf("*"));
                                                }
                                                //パスリストに追加
                                                _listVehicleFilePath.Add(path);
                                                //次のループ用に文字列切り抜き
                                                temp_line = temp_line.Substring(temp_line.IndexOf("|") + 1);
                                            }
                                            path = temp_line;
                                            if (path.IndexOf("*") > 0)
                                            {
                                                path = temp_line.Substring(0, temp_line.IndexOf("*"));
                                            }
                                            //パスリストに追加
                                            _listVehicleFilePath.Add(path);

                                        }
                                        else
                                        {
                                            _listVehicleFilePath.Add(line.Substring(line.IndexOf("=") + 1));
                                        }
                                    }
                                    //後に「=」がない場合エラーフラグを立てる
                                    else
                                    {
                                        flgErrVehicle = true;
                                    }
                                    break;
                                case "Image"://Imageファイル抽出
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        string file = line.Substring(line.IndexOf("=") + 1);

                                        pictureBox1.ImageLocation = Path.GetDirectoryName(strRouteFilePath) + "\\" + file.Trim();
                                    }
                                    break;

                                case "RouteTitle"://マップタイトル
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblRouteTitle.Text = line;
                                    }
                                    break;

                                case "Route":
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        //文字列に「|」があるとき
                                        if (line.IndexOf("|") > 0)
                                        {

                                            path = line.Substring(0, line.IndexOf("|"));
                                            //listPath.Add(path.Substring(line.IndexOf("=") + 1));
                                            string temp_line = line.Substring(line.IndexOf("=") + 1);
                                            //"|"が文字列に無くなるまで
                                            while (temp_line.IndexOf("|") > 0)
                                            {
                                                //パスを仮文字列から「|」まで切り抜き
                                                path = temp_line.Substring(0, temp_line.IndexOf("|"));
                                                //もし「*」が含まれていたらそこまで切り抜く処理
                                                if (path.IndexOf("*") > 0)
                                                {
                                                    path = temp_line.Substring(0, temp_line.IndexOf("*"));
                                                }
                                                //パスリストに追加
                                                _listMapFilePath.Add(path);
                                                //次のループ用に文字列切り抜き
                                                temp_line = temp_line.Substring(temp_line.IndexOf("|") + 1);
                                            }
                                            path = temp_line;
                                            if (path.IndexOf("*") > 0)
                                            {
                                                path = temp_line.Substring(0, temp_line.IndexOf("*"));
                                            }
                                            //パスリストに追加
                                            _listMapFilePath.Add(path);

                                        }
                                        else
                                        {
                                            //もし「*」が含まれていたらそこまで切り抜く処理
                                            if (line.IndexOf("*") > 0)
                                            {
                                                line = line.Substring(0, line.IndexOf("*"));
                                            }
                                            _listMapFilePath.Add(line.Substring(line.IndexOf("=") + 1));
                                        }
                                    }
                                    //後に「=」がない場合エラーフラグを立てる
                                    else
                                    {
                                        flgErrMap = true;
                                    }
                                    break;

                                case "Title":
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblTitle.Text = line;
                                    }
                                    break;

                                case "Author":
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblAuthor.Text = line;
                                    }
                                    break;

                                case "Comment":
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        tbComment.Text = line;
                                    }
                                    break;

                            }
                        }
                    }

                    //ファイルを閉じる
                    sr.Close();
                    stream.Close();

                    //車両ファイルが指定されている場合の処理
                    if (_listVehicleFilePath.Count > 0 && !flgErrVehicle)
                    {
                        strVehicleFilePath = dir + @"\" + _listVehicleFilePath[0].Trim();
                        for (int i = 0; i < _listVehicleFilePath.Count; i++)
                        {
                            this.listVehicleFilePath.Add(dir + @"\" + _listVehicleFilePath[i].Trim());
                        }

                        bool IsFileExists = false;
                        for (int i = 0; i < _listVehicleFilePath.Count; i++)
                        {
                            IsFileExists |= File.Exists(this.listVehicleFilePath[i]);
                        }
                        if (IsFileExists)
                        {
                            if (_listVehicleFilePath.Count > 1)
                            {
                                btnBveBootChooseVehicle.Enabled = btnBootBVE5.Enabled;
                                strDisp += "車両ファイルが複数あります。データ数：" + _listVehicleFilePath.Count + "\n\n";

                            }
                            else
                            {
                                btnBveBootChooseVehicle.Enabled = false;
                            }
                            for (int i = 0; i < this.listVehicleFilePath.Count; i++)
                            {
                                cbxVehicle.Items.Add(this.listVehicleFilePath[i]);
                            }
                            cbxVehicle.BackColor = SystemColors.Window;
                            cbxVehicle.Text = this.listVehicleFilePath[0];
                            OpenNewVehicleFile(this.listVehicleFilePath[0]);
                        }

                        //車両ファイルが見つからない場合の処理
                        else
                        {
                            btnBootBVE5.Enabled = false;
                            btnBveBootChooseVehicle.Enabled = false;
                            for (int i = 0; i < _listVehicleFilePath.Count; i++)
                            {
                                this.listVehicleFilePath.Add("Not found or supported : " + dir + @"\" + _listVehicleFilePath[i].Trim());
                            }
                            cbxVehicle.Text = "Not found or supported : " + dir + @"\" + _listVehicleFilePath[0].Trim();
                            cbxVehicle.BackColor = Color.LightYellow;
                            strDisp += "車両ファイルが見つかりません。\n\n";
                        }
                    }
                    //車両ファイルが空欄のとき
                    else
                    {
                        btnBootBVE5.Enabled = false;
                        btnBveBootChooseVehicle.Enabled = false;
                        cbxVehicle.Text = "Not defined";
                        cbxVehicle.BackColor = Color.LightYellow;
                        strDisp += "車両ファイルが指定されていません。\n\n";
                    }

                    //マップファイルが指定されている場合の処理
                    if (_listMapFilePath.Count > 0 && !flgErrMap)
                    {
                        strMapFilePath = dir + @"\" + _listMapFilePath[0].Trim();

                        for (int i = 0; i < _listMapFilePath.Count; i++)
                        {
                            this.listMapFilePath.Add(dir + @"\" + _listMapFilePath[i].Trim());
                        }

                        bool IsFileExists = false;
                        for (int i = 0; i < _listMapFilePath.Count; i++)
                        {
                            IsFileExists |= File.Exists(this.listMapFilePath[i]);
                        }
                        if (IsFileExists)
                        {
                            if (_listMapFilePath.Count > 1)
                            {
                                btnBveBootChooseMap.Enabled = btnBootBVE5.Enabled;
                                strDisp += "マップファイルが複数あります。データ数：" + _listMapFilePath.Count + "\n\n";
                            }
                            else
                            {
                                btnBveBootChooseMap.Enabled = false;
                            }
                            for (int i = 0; i < this.listMapFilePath.Count; i++)
                            {
                                cbxMapFilePath.Items.Add(this.listMapFilePath[i]);
                            }
                            //cbxMapFilePath.Text = this.listMapFilePath[0];
                            cbxMapFilePath.BackColor = SystemColors.Window;
                            OpenNewMapFile(this.listMapFilePath[0]);

                        }

                        //マップファイルが見つからない場合の処理
                        else
                        {
                            btnBootBVE5.Enabled = false;
                            btnBveBootChooseMap.Enabled = false;
                            for (int i = 0; i < _listMapFilePath.Count; i++)
                            {
                                this.listMapFilePath.Add("Not found or supported : " + dir + @"\" + _listMapFilePath[i].Trim());
                            }
                            cbxMapFilePath.Text = "Not found or supported : " + dir + @"\" + _listMapFilePath[0].Trim();
                            cbxMapFilePath.BackColor = Color.LightYellow;
                            strDisp += "マップファイルが見つかりません。\n\n";
                        }
                    }
                    //マップファイルが空欄のとき
                    else
                    {
                        btnBootBVE5.Enabled = false;
                        btnBveBootChooseMap.Enabled = false;
                        cbxMapFilePath.Text = "Not defined";
                        cbxMapFilePath.BackColor = Color.LightYellow;
                        strDisp += "マップファイルが指定されていません。\n\n";
                    }
                }
            }
            btnAtsPluginDirectory.Enabled = true;
            ofd.Dispose();
        }

        private void OpenNewMapFile(string strMapFilePath_)
        {
            if (File.Exists(strMapFilePath_))
            {
                btnMapOpen.Enabled = true;
                btnMapDirectory.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strMapFilePath_);
                string line = "";
                int i = 0;
                int error = 0;
                txbLog.AppendText("\r\n");
                txbLog.AppendText("マップファイル：" + strMapFilePath_ + "\r\n");
                cbxMapFilePath.Text = strMapFilePath_;

                while (((line = sr.ReadLine()) != null))
                {
                    if (line.IndexOf("Structure.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbStructure, ref btnStructureOpen, ref btnStructureDirectory, line, strMapFilePath_, out strStructureFilePath) <= 0)
                        {
                            error++;
                        }

                    }
                    else if (line.IndexOf("Station.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbStation, ref btnStationOpen, ref btnStationDirectory, line, strMapFilePath_, out strStationFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Signal.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbSignal, ref btnSignalOpen, ref btnSignalDirectory, line, strMapFilePath_, out strSignalFilePath) <= 0)
                        {
                            error++;
                        }

                    }
                    else if (line.IndexOf("Sound.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbSoundList, ref btnSoundListOpen, ref btnSoundListDirectory, line, strMapFilePath_, out strSoundListFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Train.Add") >= 0)
                    {

                        if (line.Substring(line.IndexOf("(")).Length > 1)
                        {
                            line = line.Substring(line.IndexOf(","));
                            int index = line.IndexOf("'");
                            strTrainFilePath = Path.GetFullPath(Path.GetDirectoryName(strMapFilePath_) + "\\" + (line.Substring(index + 1, line.IndexOf("'", index + 1) - index - 1)).Trim());
                            if (File.Exists(strTrainFilePath))
                            {
                                tbTrain.Text = strTrainFilePath;
                                btnTrainOpen.Enabled = true;
                                btnTrainDirectory.Enabled = true;
                            }
                            else
                            {
                                tbTrain.Text = "Cannot Open";
                                error++;
                            }
                        }
                        else
                        {
                            tbTrain.Text = "Not Found";
                            error++;
                        }
                    }
                    i++;
                }
                //閉じる
                sr.Close();
                if (error > 0)
                {
                    strDisp += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m\n\n";
                }
            }
            else
            {
                strDisp += "マップファイルが指定されていません\n\n";
            }
            if (strDisp != "" && cbMessageDisp.Checked)
            {
                MessageBox.Show(strDisp);
                strDisp = "";
            }
        }

        private float f_ver = 0;
        private bool IsDetailmanager32 = false;
        private bool IsDetailmanager64 = false;

        private void OpenNewVehicleFile(string _strVehicleFilePath)
        {
            if (File.Exists(_strVehicleFilePath))
            {
                btnOpenVehicleFile.Enabled = true;
                btnOpenVehicleDirectory.Enabled = true;
                btnOpenVehicleFile.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(_strVehicleFilePath);
                string line = "";
                int i = 0;
                int error = 0;
                txbLog.AppendText("\r\n");
                txbLog.AppendText("車両ファイル：" + _strVehicleFilePath + "\r\n");

                strVehicleFilePath = _strVehicleFilePath;

                while ((line = sr.ReadLine()) != null)
                {
                    //テキストボックスに追記
                    txbLog.AppendText(line + "\r\n");
                    if (line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        int index_colon = line.IndexOf(":");
                        if (index_colon < 0)
                        {
                            f_ver = float.Parse(line.Substring(line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) + 13).Trim());
                        }
                        else
                        {
                            f_ver = float.Parse(line.Substring(line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) + 13, index_colon - 13).Trim());
                        }

                        if (f_ver < 2.00)
                        {
                            lblVehicleVer.Text = "Vehicle File Ver : " + f_ver.ToString("0.00") + " BVE5 専用車両ファイル";
                            lblAts32.Text = "Ats";
                            btnAts64Check.Visible = false;
                            btnAts64Open.Visible = false;
                            btnAts64OpenDirectory.Visible = false;
                            tbAts64.Visible = false;
                            gbxBve6.Visible = false;
                            lblAts64.Visible = false;
                            label9.Visible = false;
                            btnAts64RelatePathGen.Visible = false;
                            btnAts64Add.Visible = false;
                            tbAts64RelatePath.Visible = false;

                            //BVE6コンバータ
                            lblDetailModuleStore.Visible = true;
                            lblDetailModuleStoreDetail.Visible = true;
                            btnDetailModuleSelect.Visible = true;
                            lblDetailManager.Visible = true;
                            lblDetailManagerDetail.Visible = true;
                            btnDetailManagerBve6Select.Visible = true;
                            btnBve6Convert.Visible = true;
                            if (strAts64SettingTextFilePath=="") {
                                btnBve6Convert.Enabled = false;
                            }
                            btnBve5Recovery.Visible = false;


                        }
                        else
                        {
                            lblVehicleVer.Text = "Vehicle File Ver : " + f_ver.ToString("0.00") + " BVE6 対応車両ファイル";
                            lblAts32.Text = "Ats32";
                            //BVE6コンバータ
                            lblDetailModuleStore.Visible = false;
                            lblDetailModuleStoreDetail.Visible = false;
                            btnDetailModuleSelect.Visible = false;
                            lblDetailManager.Visible = false;
                            lblDetailManagerDetail.Visible = false;
                            btnDetailManagerBve6Select.Visible = false;
                            btnBve6Convert.Visible = false;
                            if (File.Exists(strVehicleFilePath + @".bak"))
                            {
                                btnBve5Recovery.Visible = true;
                                btnBve5Recovery.Enabled = true;
                            }

                        }
                    }
                    if ((line.IndexOf("ATS", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("Ats32", StringComparison.OrdinalIgnoreCase) >= 0) && line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        btnAts32OpenDirectory.Enabled = true;
                        int iRet = PathControl_Vehicle(ref tbAts32, ref btnAts32Open, ref btnAts32Open, line, _strVehicleFilePath, out strAts32DetailManagerFilePath);
                        if (strAts32DetailManagerFilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                        {
                            strDisp += "BVE5用(32bit)ATSプラグイン(DetailManager)が見つかりました\n\n";
                            strAts32SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(strAts32DetailManagerFilePath) + @"\detailmodules.txt");
                            OpenNewAtsPluginFile(strAts32SettingTextFilePath, BVE_Version.Ver5);
                            IsDetailmanager32 = true;
                            btnAts32Open.Enabled = true;
                        }
                        else
                        {
                            strDisp += "BVE5用(32bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n\n";
                            IsDetailmanager32 = false;
                            btnAts32Open.Enabled = false;
                        }
                        if (iRet <= 0)
                        {
                            error++;
                            btnAts32Check.Enabled = false;
                        }
                        else
                        {
                            btnAts32Check.Enabled = true;
                        }
                    }
                    else if (line.IndexOf("PerformanceCurve", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (PathControl_Vehicle(ref tbPerfoemanceCurve, ref btnPerfoemanceCurveOpen, ref btnPerfoemanceCurveDirectory, line, _strVehicleFilePath, out strPerfoemanceCurveFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Parameters", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (PathControl_Vehicle(ref tbParameters, ref btnParametersOpen, ref btnParametersDirectory, line, _strVehicleFilePath, out strParametersFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (PathControl_Vehicle(ref tbPanel, ref btnPanelOpen, ref btnPanelDirectory, line, _strVehicleFilePath, out strPanelFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Sound", StringComparison.OrdinalIgnoreCase) >= 0 && (line.IndexOf("Sound") < line.IndexOf("=")))
                    {
                        if (PathControl_Vehicle(ref tbSound, ref btnSoundOpen, ref btnSoundDirectory, line, _strVehicleFilePath, out strSoundFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("MotorNoise", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if (PathControl_Vehicle(ref tbMotorNoise, ref btnMotorNoiseOpen, ref btnMotorNoiseDirectory, line, _strVehicleFilePath, out strMotorNoiseFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) >= 0)//Ats64が見つかった場合
                    {
                        //ファイルチェック
                        int ret = PathControl_Vehicle(ref tbAts64, ref btnAts64Open, ref btnAts64Open, line, _strVehicleFilePath, out strAts64DetailManagerFilePath);
                        if (ret <= 0)
                        {
                            error++;
                            btnBootBVE6.Enabled = false;
                            btnAts64Open.Enabled = false;
                            btnAts64OpenDirectory.Enabled = false;
                            btnAts64Check.Enabled = false;

                            btnAts64Check.Visible = false;
                            btnAts64Open.Visible = false;
                            btnAts64OpenDirectory.Visible = false;
                            tbAts64.Visible = false;
                            gbxBve6.Visible = false;
                            lblAts64.Visible = false;
                        }
                        else if (ret >= 1)
                        {
                            btnBootBVE6.Enabled = true;
                            //btnAts64OpenFile.Enabled = true;
                            //btnAts64OpenDirectory.Enabled = true;
                            btnAts64Check.Enabled = true;

                            btnAts64Check.Visible = true;
                            btnAts64Open.Visible = true;
                            btnAts64OpenDirectory.Visible = true;
                            btnAts64OpenDirectory.Enabled = true;
                            tbAts64.Visible = true;
                            gbxBve6.Visible = true;
                            lblAts64.Visible = true;
                            label9.Visible = true;
                            btnAts64RelatePathGen.Visible = true;
                            btnAts64Add.Visible = true;
                            tbAts64RelatePath.Visible = true;

                            if (f_ver >= 2.0 && strAts64DetailManagerFilePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                strDisp += "BVE6用(64bit)ATSプラグイン(DetailManager)が見つかりました\n\n";
                                strAts64SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(strAts64DetailManagerFilePath) + @"\detailmodules.txt");
                                OpenNewAtsPluginFile(strAts64SettingTextFilePath, BVE_Version.Ver6);
                                IsDetailmanager64 = true;
                                btnAts64Open.Enabled = true;
                            }
                            else
                            {
                                strDisp += "BVE6用(64bit)ATSプラグインが見つからないか、対応していません(DetailManager以外)\n\n";
                                IsDetailmanager64 = false;
                                btnAts64Open.Enabled = false;
                            }

                        }
                    }
                    i++;
                }
                //閉じる
                sr.Close();

                if (error > 0)
                {
                    strDisp += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m\n\n";
                }

            }
            else
            {
                btnOpenVehicleFile.Enabled = false;
                btnOpenVehicleDirectory.Enabled = false;
                btnOpenVehicleFile.Enabled = false;
                strDisp += "車両が指定されていません\n\n";
            }
            if (strDisp != "" && cbMessageDisp.Checked)
            {
                MessageBox.Show(strDisp);
                strDisp = "";
            }

        }

        private void OpenNewAtsPluginFile(string strAtsPluginFilePath, BVE_Version bve_ver)
        {
            if (File.Exists(strAtsPluginFilePath))
            {
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strAtsPluginFilePath);
                string line = "";
                int i = 0;
                int row = 0;
                string strBveVer = "";
                List<string> _listAtsPlugins = new List<string>();
                if (bve_ver == BVE_Version.Ver5)
                {
                    strBveVer = "BVE5用(32bit)";
                    btnAts32OpenDirectory.Enabled = true;
                    btnAts32Check.Enabled = true;
                    dgvAts32.Rows.Clear();
                    txbLog.AppendText("\r\n");
                    txbLog.AppendText("ATSプラグイン" + strBveVer + ":" + strAtsPluginFilePath + "\r\n");
                    row = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("ATS32", StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Substring(line.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1).Trim();
                        }
                        txbLog.AppendText(line + "\r\n");
                        _listAtsPlugins.Add(line);

                        string marge_line = line.Replace("#", "").Trim();
                        marge_line = marge_line.Replace(";", "").Trim();
                        string FullPath = "";
                        try
                        {
                            FullPath = Path.GetFullPath(Path.GetDirectoryName(strAtsPluginFilePath) + @"\" + marge_line);
                        }
                        catch
                        {
                            FullPath = marge_line;
                        }
                        BVE_Version Ret = AtsPluginChecker(FullPath, 300, false);
                        dgvAts32.Rows.Add(Path.GetFileName(line), Ret.ToString(), line, FullPath);
                        //dgvAts32.Rows.Add(Path.GetFileName(line), Path.GetFullPath(Path.GetDirectoryName(strAtsPluginFilePath) + @"\" + line), line);

                        if (Ret == BVE_Version.NotFound)
                        {
                            dgvAts32[1, i].Style.BackColor = Color.Red;
                        }
                        else if (Ret == BVE_Version.Ver6)
                        {
                            dgvAts32[1, i].Style.BackColor = Color.Yellow;
                        }
                        if (line.StartsWith("#") || line.StartsWith(";"))
                        {
                            dgvAts32[0, i].Style.BackColor = Color.LightGray;
                            dgvAts32[1, i].Style.BackColor = Color.LightGray;
                            dgvAts32[2, i].Style.BackColor = Color.LightGray;
                            dgvAts32[3, i].Style.BackColor = Color.LightGray;
                        }

                        if (line.IndexOf(Path.GetFileName(this.strAtsPluginFilePath)) > 0)
                        {
                            row = i + 1;
                        }
                        i++;
                    }
                }
                else if (bve_ver == BVE_Version.Ver6)
                {
                    strBveVer = "BVE6用(64bit)";
                    btnAts64OpenDirectory.Enabled = true;//
                    btnAts64Check.Enabled = true;
                    dgvAts64.Rows.Clear();
                    txbLog.AppendText("\r\n");
                    txbLog.AppendText("ATSプラグイン" + strBveVer + ":" + strAtsPluginFilePath + "\r\n");
                    row = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("ATS64", StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Substring(line.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1).Trim();
                        }
                        txbLog.AppendText(line + "\r\n");
                        _listAtsPlugins.Add(line);
                        string marge_line = line.Replace("#", "").Trim();
                        marge_line = marge_line.Replace(";", "").Trim();
                        string FullPath = "";
                        try
                        {
                            FullPath = Path.GetFullPath(Path.GetDirectoryName(strAtsPluginFilePath) + @"\" + marge_line);
                        }
                        catch
                        {
                            FullPath = marge_line;
                        }
                        BVE_Version Ret = AtsPluginChecker(FullPath, 300, false);
                        dgvAts64.Rows.Add(Path.GetFileName(line), Ret.ToString(), line, FullPath);
                        //dgvAts64.Rows.Add(Path.GetFileName(line), Path.GetFullPath(Path.GetDirectoryName(strAtsPluginFilePath) + @"\" + line), line);

                        if (Ret == BVE_Version.NotFound)
                        {
                            dgvAts64[1, i].Style.BackColor = Color.Red;
                        }
                        else if (Ret == BVE_Version.Ver5)
                        {
                            dgvAts64[1, i].Style.BackColor = Color.Yellow;
                        }
                        if (line.StartsWith("#") || line.StartsWith(";"))
                        {
                            dgvAts64[0, i].Style.BackColor = Color.LightGray;
                            dgvAts64[1, i].Style.BackColor = Color.LightGray;
                            dgvAts64[2, i].Style.BackColor = Color.LightGray;
                            dgvAts64[3, i].Style.BackColor = Color.LightGray;
                        }

                        if (line.IndexOf(Path.GetFileName(this.strAtsPluginFilePath)) > 0)
                        {
                            row = i + 1;
                        }
                        i++;
                    }
                }


                //閉じる
                sr.Close();
                if (row > 0)
                {
                    strDisp += "既に追記されています。\n\n";
                }
                else
                {
                    strDisp += "ATSプラグインのターゲットを確認後、ATSプラグインパス生成ボタンにより相対パスを生成し、追記してください。\n\n";
                }
                flgAtsPluginFileOpen = true;
                if (flgAtsPluginDirectoryOpen)
                {
                    btnGenerateRelativePath.Enabled = true;
                }
            }
            else
            {
                strDisp += "ATSプラグインが見つかりません:" + strAtsPluginFilePath + "\n\n";
            }
        }

        private void btnOpenRouteFile_Click(object sender, EventArgs e)
        {
            ProcessStart(strRouteFilePath, false);
        }

        private void btnOpenVehicleFile_Click(object sender, EventArgs e)
        {
            if (listVehicleFilePath.Count > 1)
            {
                ProcessStart(listVehicleFilePath[cbxVehicleIndex], false);
            }
            else
            {
                ProcessStart(strVehicleFilePath, false);
            }
        }

        private void btnOpenAts32_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts32SettingTextFilePath, false);
        }

        private void btnOpenAts32Directory_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts32DetailManagerFilePath, true);
        }

        private void btnOpenAts64Directory_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts64DetailManagerFilePath, true);
        }

        private void btnOpenVehicleDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strVehicleFilePath, true);
        }

        private void btnAtsPluginDirectory_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.FileName = "";
            ofd.Filter = "ATSプラグインファイル(*.dll)|*.dll";
            ofd.Title = "ATSプラグインファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.InitialDirectory = Settings.Default.AtsPluginFileDirectory;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.AtsPluginFileDirectory = Path.GetDirectoryName(ofd.FileName);
                strAtsPluginFilePath = ofd.FileName;
                tbAtsPluginFile.Text = ofd.FileName;
                flgAtsPluginDirectoryOpen = true;
                btnOpenSenario.Enabled = true;
                if (flgAtsPluginFileOpen)
                {
                    btnGenerateRelativePath.Enabled = true;
                    tbAtsPluginFile.Enabled = true;
                }
            }

            BVE_Version iRet = AtsPluginChecker(strAtsPluginFilePath, 300, cbMessageDisp.Checked);
            tbAts32RelatePath.Text = "";
            tbAts64RelatePath.Text = "";
            tbAts32RelatePath.Enabled = false;
            tbAts64RelatePath.Enabled = false;
            if (iRet == BVE_Version.Ver5)
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
            else if (iRet == BVE_Version.Ver6)
            {
                lblAtsPluginFile.Text = "ATSプラグイン BVE6用 (64bit)ビルド";
                btnAts32Add.Enabled = false;
                btnAts32RelatePathGen.Enabled = false;
                btnAts64Add.Enabled = false;
                if (tbSeinarioFileName.Text != "" && strAts64SettingTextFilePath != "")
                {
                    if (f_ver >= 2.0)
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

            ofd.Dispose();
        }

        private string GanarateRelativePath(string OriginalFilePath, string TargetPath)
        {
            string relativePath = "";
            if ((OriginalFilePath != "" || TargetPath != "") && (File.Exists(OriginalFilePath) && File.Exists(TargetPath)))
            {
                Uri u1 = new Uri(OriginalFilePath);
                Uri u2 = new Uri(TargetPath);

                Uri relativeUri = u1.MakeRelativeUri(u2);

                relativePath = relativeUri.ToString();

                relativePath = relativePath.Replace('/', '\\');
            }

            return relativePath;
        }

        private void btnGenerateRelativePath_Click(object sender, EventArgs e)
        {
            BVE_Version iRet = AtsPluginChecker(strAtsPluginFilePath, 300, false);
            string relativePath = "";
            if (iRet == BVE_Version.Ver5)
            {
                relativePath = GanarateRelativePath(strAts32SettingTextFilePath, strAtsPluginFilePath);
            }
            else if (iRet == BVE_Version.Ver6)
            {
                relativePath = GanarateRelativePath(strAts64SettingTextFilePath, strAtsPluginFilePath);
            }

            txbLog.AppendText("\r\n");
            txbLog.AppendText("ATSプラグインファイル(detailmodules.txtに下記ファイルパスを追記して下さい\r\n");
            txbLog.AppendText(relativePath);
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("【説明】\r\n" +
                "Rock_On様作製のATSプラグイン(DetailManager)専用です\r\n" +
                "detailmodules.txtを書き換えたいときに使えるかと思います\r\n" +
                "\r\n" +
                "【つかいかた】\r\n" +
                "1.シナリオファイルを選択" +
                "2.プラグインを選択\r\n" +
                "\r\n3.ATSプラグインファイルを開く\r\n" +
                "4.ATSプラグインファイルパスを生成し、追記する(一応.bakでバックアップします)\r\r" +
                "\r\n" +
                "【できないこと】\r\n" +
                "DetailManager非対応のプラグイン、\r\n" +
                "include形式ファイル、パス内変数形式、自動削除機能\r\n" +
                "\r\n" +
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします※", "使い方");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TempFileChecker();
            Settings.Default.Save();
        }

        private void TempFileChecker()
        {
            if (File.Exists(strRouteFilePath + ".tmp.txt"))
            {
                File.Delete(strRouteFilePath + ".tmp.txt");
            }
            if (File.Exists(strRouteFilePath + ".bak"))
            {
                File.Delete(strRouteFilePath + ".bak");
            }
        }

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

        private void btnMapOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath, false);
        }

        private void btnPerfoemanceCurveOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strPerfoemanceCurveFilePath, false);
        }


        private void btnPerfoemanceCurveDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strPerfoemanceCurveFilePath, true);
        }

        private void btnParametersOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strParametersFilePath, false);
        }

        private void btnParametersDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strParametersFilePath, true);
        }

        private void btnPanelOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strPanelFilePath, false);
        }


        private void btnPanelDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strPanelFilePath, true);
        }

        private void btnSoundOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundFilePath, false);
        }
        private void btnSoundDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundFilePath, true);
        }

        private void btnMotorNoiseOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strMotorNoiseFilePath, false);
        }

        private void btnMotorNoiseDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strMotorNoiseFilePath, true);
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
            ProcessStart(strStructureFilePath, false);
        }

        private void btnStructureDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strStructureFilePath, true);
        }

        private void btnStationOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strStationFilePath, false);
        }
        private void btnStationDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strStationFilePath, true);
        }
        private void btnSignalOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSignalFilePath, false);
        }
        private void btnSignalDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strSignalFilePath, true);
        }

        private void btnSoundListOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundListFilePath, false);
        }
        private void btnSoundListDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundListFilePath, true);
        }

        private void btnTrainOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strTrainFilePath, false);
        }

        private void btnTrainDirectory_Click(object sender, EventArgs e)
        {
            ProcessStart(strTrainFilePath, true);
        }

        //マップファイルのファイルパスを生成する
        private string PathGenerator_Map(string _line, string _dirPath)
        {
            string s = "";
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

        //車両ファイルのファイルパスを生成する
        private string PathGenerator_Vehicle(string _line, string _dirPath)
        {
            string s = "";
            s = Path.GetFullPath(Path.GetDirectoryName(_dirPath) + "\\" + _line.Substring(_line.IndexOf("=") + 1).Trim());
            return s;
        }

        //マップの存在を確認する
        private int PathControl_Map(ref TextBox tb, ref Button btnFile, ref Button btnDirectory, string line_, string mapPath_, out string strFilePath_)
        {
            if (line_.Substring(line_.IndexOf("(")).Length > 1)
            {
                strFilePath_ = PathGenerator_Map(line_, mapPath_);
                if (File.Exists(strFilePath_))
                {
                    tb.Text = strFilePath_;
                    btnFile.Enabled = true;
                    btnDirectory.Enabled = true;
                    tb.BackColor = SystemColors.Window;
                    return 1;
                }
                else
                {
                    tb.Text = "Not Found or Supported : " + strFilePath_;
                    tb.BackColor = Color.LightYellow;
                    return -1;
                }
            }
            else
            {
                strFilePath_ = "Not Found : Not declared";
                tb.Text = "Not Found : Not Declared";
                tb.BackColor = Color.Salmon;
                return 0;
            }
        }

        //車両ファイルの存在を確認する
        private int PathControl_Vehicle(ref TextBox tb, ref Button btnFile, ref Button btnDirectory, string line_, string vehiclePath_, out string strFilePath_)
        {
            strFilePath_ = "";
            int Ret = 0;
            if (line_.Contains("="))
            {
                if (line_.Substring(line_.IndexOf("=")).Length > 1)
                {
                    strFilePath_ = PathGenerator_Vehicle(line_, vehiclePath_);
                    if (File.Exists(strFilePath_))
                    {
                        tb.Text = strFilePath_;
                        btnFile.Enabled = true;
                        btnDirectory.Enabled = true;
                        tb.BackColor = SystemColors.Window;
                        Ret = 1;
                    }
                    else
                    {
                        tb.Text = "Not Found or Supported : " + strFilePath_;
                        tb.BackColor = SystemColors.Window;
                        tb.BackColor = Color.LightYellow;
                        Ret = -1;
                    }

                }
                else
                {
                    strFilePath_ = "Not Found : Not declared";
                    tb.Text = "Not Found : Not Declared";
                    tb.BackColor = Color.Salmon;
                    Ret = 0;
                }
            }
            else
            {
                tb.Text = "Not Found or Supported : " + vehiclePath_;
                tb.BackColor = SystemColors.Window;
                tb.BackColor = Color.LightYellow;
                Ret = -1;
            }
            return Ret;
        }

        //リセット
        private void Reset()
        {
            btnOpenSenarioFile.Enabled = false;
            btnOpenVehicleFile.Enabled = false;
            btnOpenVehicleDirectory.Enabled = false;
            btnAts32OpenDirectory.Enabled = false;
            btnAts64OpenDirectory.Enabled = false;
            btnMapOpen.Enabled = false;
            strRouteFilePath = "";
            strVehicleFilePath = "";

            strMapFilePath = "";
            strPerfoemanceCurveFilePath = "";
            strParametersFilePath = "";
            strPanelFilePath = "";
            strSoundFilePath = "";
            strMotorNoiseFilePath = "";
            strAts32DetailManagerFilePath = "";
            strAts32SettingTextFilePath = "";
            strAts64DetailManagerFilePath = "";
            strAts64SettingTextFilePath = "";
            strStructureFilePath = "";
            strSignalFilePath = "";
            strSoundListFilePath = "";
            strTrainFilePath = "";

            flgAtsPluginFileOpen = false;
            btnGenerateRelativePath.Enabled = false;
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
            btnAts32RelatePathGen.Enabled = false;
            btnAts32Add.Enabled = false;
            btnAts64RelatePathGen.Enabled = false;
            btnAts64Add.Enabled = false;

            cbxVehicle.Items.Clear();
            cbxMapFilePath.Items.Clear();
            listVehicleFilePath.Clear();
            listMapFilePath.Clear();

            tbAtsPluginFile.Text = "";
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32RelatePath.Text = "";
            tbAts64RelatePath.Text = "";
            tbAts32.Text = "";
            tbAts64.Text = "";
            cbxVehicle.Text = "";
            cbxMapFilePath.Text = "";
            dgvAts32.Rows.Clear();
            dgvAts64.Rows.Clear();


            //マップファイルページ
            btnStructureOpen.Enabled = false;
            btnStationOpen.Enabled = false;
            btnSignalOpen.Enabled = false;
            btnSoundListOpen.Enabled = false;
            btnTrainOpen.Enabled = false;
            tbStructure.Text = "";
            tbStation.Text = "";
            tbSignal.Text = "";
            tbSoundList.Text = "";
            tbTrain.Text = "";

            txbLog.Clear();

            pictureBox1.Image = null;
            lblTitle.Text = "Title";
            lblRouteTitle.Text = "Route Title";
            lblVehicleTitle.Text = "Vehicle Title";
            lblAuthor.Text = "Author";
            tbComment.Text = "Comment";

            IsDetailmanager32 = false;
            IsDetailmanager64 = false;

            btnBve6Convert.Enabled = false;

        }

        private void btnBootBVE5_Click(object sender, EventArgs e)
        {
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + strRouteFilePath + "\"");
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
                Process.Start(strBve6Path, "\"" + strRouteFilePath + "\"");
            }
            else
            {
                MessageBox.Show("BVE6が見つかりません。BVEパスを設定してください。");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbMessageDisp.Checked = Settings.Default.cbMessage;
            if (Settings.Default.strBve5Path != "")
            {
                strBve5Path = Settings.Default.strBve5Path;
            }
            if (Settings.Default.strBve6Path != "")
            {
                strBve6Path = Settings.Default.strBve6Path;
            }
        }

        private void btnBackUp_Click(object sender, EventArgs e)
        {
            Make_NewVehicleFile(@strRouteFilePath, @strRouteFilePath + ".tmp.txt");
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
                        // 引用 https://dobon.net/vb/dotnet/file/getabsolutepath.html#uriencode

                        //"%"を"%25"に変換しておく（デコード対策）
                        string u1_Path = oldFileName_.Replace("%", "%25");
                        string u2_Path = cbxVehicle.Text.Replace("%", "%25");

                        //相対パスを取得する
                        Uri u1 = new Uri(u1_Path);
                        Uri u2 = new Uri(u2_Path);

                        Uri relativeUri = u1.MakeRelativeUri(u2);

                        string relativePath = relativeUri.ToString();

                        //URLデコードする（エンコード対策）
                        relativePath = Uri.UnescapeDataString(relativePath);

                        //"%25"を"%"に戻す
                        relativePath = relativePath.Replace("%25", "%");

                        relativePath = relativePath.Replace('/', '\\');
                        lines[i] = "Vehicle = " + relativePath;
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
                        // 引用 https://dobon.net/vb/dotnet/file/getabsolutepath.html#uriencode

                        //"%"を"%25"に変換しておく（デコード対策）
                        string u1_Path = oldFileName_.Replace("%", "%25");
                        string u2_Path = cbxMapFilePath.Text.Replace("%", "%25");

                        //相対パスを取得する
                        Uri u1 = new Uri(u1_Path);
                        Uri u2 = new Uri(u2_Path);

                        Uri relativeUri = u1.MakeRelativeUri(u2);

                        string relativePath = relativeUri.ToString();

                        //URLデコードする（エンコード対策）
                        relativePath = Uri.UnescapeDataString(relativePath);

                        //"%25"を"%"に戻す
                        relativePath = relativePath.Replace("%25", "%");

                        relativePath = relativePath.Replace('/', '\\');
                        lines[i] = "Route = " + relativePath;
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

            string[] lines = File.ReadAllLines(textFile);

            for (int i = 0; i < lines.Length; i++)
            {
                string strAts32Path = Path.GetFullPath(Path.GetDirectoryName(strAts32SettingTextFilePath) + @"\"+lines[i]);
                if (lines[i].IndexOf(@"\GeneralAtsPlugin\Rock_On", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    string[] keyword = { "Rock_On" };
                    string[] arr = strAts32Path.Split(keyword, StringSplitOptions.None);
                    string strAts64Path = arr[0] + keyword[0] + @"\x64" + arr[1];
                    if(File.Exists(strAts64Path) && (AtsPluginChecker(strAts64Path,300,false) == BVE_Version.Ver6))
                    {
                        lines[i] = GanarateRelativePath(strAts64SettingTextFilePath, strAts64Path);
                    }
                    else
                    {
                        Uri u1 = new Uri(strAts64SettingTextFilePath);
                        Uri u2 = new Uri(strAts64Path);

                        Uri relativeUri = u1.MakeRelativeUri(u2);

                        string relativePath = relativeUri.ToString();

                        relativePath = relativePath.Replace('/', '\\');

                        lines[i] = "#" + relativePath;
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

        private void cbMessage_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.cbMessage = cbMessageDisp.Checked;
            Settings.Default.Save();
        }

        private void btnBveBootChooseVehicle_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = strRouteFilePath + ".tmp.txt";
            Make_NewVehicleFile(@strRouteFilePath, strNewRouteFile);
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVEが見つかりません。BVEパスを設定してください。");
            }
        }

        private void cbxMapFilePath_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbStructure.Text = "";
            tbStation.Text = "";
            tbSignal.Text = "";
            tbSoundList.Text = "";
            tbTrain.Text = "";
            cbxMapFilePath.Text = "";
            //コンボボックスのインデックスを取得
            cbxMapIndex = cbxMapFilePath.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewMapFile(listMapFilePath[cbxMapIndex]);
        }

        private void btnBveBootChooseMap_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = strRouteFilePath + ".tmp.txt";
            make_newMapFile(@strRouteFilePath, strNewRouteFile);
            if (File.Exists(strBve5Path))
            {
                Process.Start(strBve5Path, "\"" + strNewRouteFile + "\"");
            }
            else
            {
                MessageBox.Show("BVEが見つかりません。BVEパスを設定してください。");
            }
        }

        private void btnBve5PathSetting_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "bvets.exe";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
            //タイトルを設定する
            ofd.Title = "BVEの実行ファイルを選択してください";
            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                strBve5Path = ofd.FileName;
                Settings.Default.strBve5Path = strBve5Path;
                Settings.Default.Save();
            }
            ofd.Dispose();
        }

        private void btnBve6PathSetting_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "bvets.exe";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "EXEファイル(*.EXE;*.exe)|*.EXE;*.exe";
            //タイトルを設定する
            ofd.Title = "BVEの実行ファイルを選択してください";
            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                strBve6Path = ofd.FileName;
                Settings.Default.strBve6Path = strBve6Path;
                Settings.Default.Save();
            }
            ofd.Dispose();
        }

        private void cbxVehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32.Text = "";
            tbAts64.Text = "";

            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
        }

        private void btnAts32Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(strAts32DetailManagerFilePath, 300, true);
            if (IsDetailmanager32)
            {
                tabControl1.SelectTab(tabControl1.TabPages["tpAts32"]);
            }
        }

        private void btnAts64Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(strAts64DetailManagerFilePath, 300, true);
            if (IsDetailmanager64)
            {
                tabControl1.SelectTab(tabControl1.TabPages["tpAts64"]);
            }
        }

        private BVE_Version AtsPluginChecker(string _FilePath, int _BufferSize, bool IsDisplayChecked)
        {
            BVE_Version iRet = BVE_Version.NotFound;
            byte[] bufr = new byte[_BufferSize];
            if (Path.GetFileName(_FilePath).StartsWith("#") || Path.GetFileName(_FilePath).StartsWith(";"))
            {
                iRet = BVE_Version.Comment;
            }
            else if (Path.GetFileName(_FilePath) != "")
            {
                if (File.Exists(_FilePath))
                {
                    using (FileStream fs = new FileStream(_FilePath, FileMode.Open, FileAccess.Read))
                    {
                        fs.Read(bufr, 0, _BufferSize - 1);           // ファイルから10バイト読み込む。
                                                                     // 格納先はbufrのインデックス0～9。
                        fs.Dispose();
                    }
                    bool ret = true;
                    int i = 0;
                    while (ret)
                    {
                        if ((i < _BufferSize - 5) && bufr[i] == 0x50 && bufr[i + 1] == 0x45 && bufr[i + 2] == 0x00 && bufr[i + 3] == 0x00 && bufr[i + 4] == 0x4C && bufr[i + 5] == 0x01)
                        {
                            if (IsDisplayChecked)
                            {
                                MessageBox.Show("BVE5用(32bit)にビルドされたプラグインです");
                            }
                            iRet = BVE_Version.Ver5;
                            ret = false;
                        }
                        else if ((i < _BufferSize - 5) && bufr[i] == 0x50 && bufr[i + 1] == 0x45 && bufr[i + 2] == 0x00 && bufr[i + 3] == 0x00 && bufr[i + 4] == 0x64 && bufr[i + 5] == 0x86)
                        {
                            if (IsDisplayChecked)
                            {
                                MessageBox.Show("BVE6用(64bit)にビルドされたプラグインです");
                            }
                            iRet = BVE_Version.Ver6; ;
                            ret = false;
                        }
                        else
                        {
                            if (i < _BufferSize)
                            {
                                i++;
                            }
                            else
                            {
                                if (IsDisplayChecked)
                                {
                                    MessageBox.Show("判定できませんでした");
                                }
                                iRet = BVE_Version.Null;
                                ret = false;
                            }
                        }
                    }
                }
            }
            else
            {
                iRet = BVE_Version.Null;
            }
            return iRet;
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
            DialogResult result = MessageBox.Show("BVE5(32bit)用 detailmodules.txtに追記します。\r\n\r\n操作は取り消しできません。続行します。", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                if (!File.Exists(strAts32SettingTextFilePath + ".bak"))
                {
                    File.Copy(strAts32SettingTextFilePath, strAts32SettingTextFilePath + ".bak", true);
                }
                StreamReader sr = new StreamReader(strAts32SettingTextFilePath);
                string str = sr.ReadToEnd().TrimEnd();
                sr.Close();
                str += Environment.NewLine + tbAts32RelatePath.Text;
                File.WriteAllText(strAts32SettingTextFilePath, str);
                //コンボボックスのインデックスを取得
                cbxVehicleIndex = cbxVehicle.SelectedIndex;
                //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
                OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
            }
        }

        private void btnAts64Add_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("BVE6(64bit)用 detailmodules.txtに追記します。\r\n\r\n操作は取り消しできません。続行します。", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                if (!File.Exists(strAts64SettingTextFilePath + ".bak"))
                {
                    File.Copy(strAts64SettingTextFilePath, strAts64SettingTextFilePath + ".bak", true);
                }
                StreamReader sr = new StreamReader(strAts64SettingTextFilePath);
                string str = sr.ReadToEnd().TrimEnd();
                sr.Close();
                str += Environment.NewLine + tbAts64RelatePath.Text;
                File.WriteAllText(strAts64SettingTextFilePath, str);
                //コンボボックスのインデックスを取得
                cbxVehicleIndex = cbxVehicle.SelectedIndex;
                //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
                OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
            }
        }

        private void dgvAts32_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //DataGridView dgvAts32 = (DataGridView)sender;
            if (e.RowIndex > 0)
            {
                if (dgvAts32[1, e.RowIndex].Value.ToString() == BVE_Version.NotFound.ToString())
                {
                    dgvAts32[1, e.RowIndex].Style.BackColor = Color.Red;
                }
                else if (dgvAts32[1, e.RowIndex].Value.ToString() == "Ver6")
                {
                    dgvAts32[1, e.RowIndex].Style.BackColor = Color.Yellow;
                }
            }
        }

        private void dgvAts64_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            //DataGridView dgvAts64 = (DataGridView)sender;
            if (e.RowIndex > 0)
            {
                if (dgvAts64[1, e.RowIndex].Value.ToString() == BVE_Version.NotFound.ToString())
                {
                    dgvAts64[1, e.RowIndex].Style.BackColor = Color.Red;
                }
                else if (dgvAts64[1, e.RowIndex].Value.ToString() == "Ver5")
                {
                    dgvAts64[1, e.RowIndex].Style.BackColor = Color.Yellow;
                }
            }
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (cbxVehicle.Items.Count > 0)
            {
                //コンボボックスのインデックスを取得
                cbxVehicleIndex = cbxVehicle.SelectedIndex;
                //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
                OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //[引用]https://www.sejuku.net/blog/49295#index_id1

            FolderBrowserDialog fbDialog = new FolderBrowserDialog();

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
                    tbAts64.Text = strAts64DetailManagerFilePath;
                    if (!File.Exists(strAts64DetailManagerFilePath))
                    {
                        File.Copy(strAts64DetailManagerOrgFilePath, strAts64DetailManagerFilePath);
                        if (!File.Exists(strAts64SettingTextFilePath))
                        {
                            File.WriteAllText(strAts64SettingTextFilePath, "");
                        }
                    }

                    if ((File.Exists(strAts64DetailManagerFilePath) && AtsPluginChecker(strAts64DetailManagerFilePath, 300, false) == BVE_Version.Ver6))
                    {
                        if (!File.Exists(strAts64SettingTextFilePath))
                        {
                            File.WriteAllText(strAts64SettingTextFilePath, "");
                        }
                        tbAts64.Visible = true;
                        tbAts64.Enabled = true;
                        btnBve6Convert.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("既に指定した場所にDetailManager.dllがありますがBVE6用(64bit)ではありません。場所をよく確認した後、手動で削除し、再度選択してください。");
                    }                    
                }
            }

            // オブジェクトを破棄する
            fbDialog.Dispose();

        }


        private string strAts64DetailManagerOrgFilePath = "";
        private void button5_Click(object sender, EventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "BVE6 DetailManager(DetailManager.dll)|DetailManager.dll";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.InitialDirectory = Settings.Default.DetailManager64Path;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (AtsPluginChecker(ofd.FileName, 300, false) == BVE_Version.Ver6)
                {
                    strAts64DetailManagerOrgFilePath = ofd.FileName;
                    Settings.Default.DetailManager64Path = Path.GetDirectoryName(ofd.FileName);
                }
                else
                {
                    MessageBox.Show("BVE6用(64bit)のDetailManager.dllではありません。選択し直してください。");
                }
            }
            ofd.Dispose();
        }

        private void btnBve6Convert_Click(object sender, EventArgs e)
        {
            if (!File.Exists(strVehicleFilePath + @".bak"))
            {
                File.Move(strVehicleFilePath, strVehicleFilePath + @".bak");
            }
            StreamReader sr_temp = new StreamReader(strVehicleFilePath + @".bak");
            StreamReader sr;
            Encoding enc;
            if (sr_temp.ReadLine().IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) > 0)
            {
                sr = new StreamReader(strVehicleFilePath + @".bak", Encoding.GetEncoding("shift_jis"));
                //文字コード(ここでは、Shift JIS)
                enc = Encoding.GetEncoding("shift_jis");
            }
            else
            {
                sr = new StreamReader(strVehicleFilePath + @".bak");
                enc = Encoding.GetEncoding("utf-8");
            }
            sr_temp.Close();
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
                    File.AppendAllText(strVehicleFilePath, Environment.NewLine + "Ats32 = "+ GanarateRelativePath(strVehicleFilePath, strAts32DetailManagerFilePath) + Environment.NewLine + "Ats64 = " + GanarateRelativePath(strVehicleFilePath, strAts64DetailManagerFilePath), enc);
                }
                else if (line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) >= 0)//Ats64が見つかった場合
                {

                }
                else
                {
                    File.AppendAllText(strVehicleFilePath, Environment.NewLine + line, enc);
                }
            }
            //閉じる
            sr.Close();
            
            Make_NewDetailModulesFile(strAts32SettingTextFilePath, strAts64SettingTextFilePath);
            OpenNewVehicleFile(strVehicleFilePath);

        }

        private void btnBve5Recovery_Click(object sender, EventArgs e)
        {
            bool exist = false;
            if (File.Exists(strVehicleFilePath + @".bak"))
            {
                File.Copy(strVehicleFilePath + @".bak", strVehicleFilePath, true);
                exist = true;
            }
            if (exist)
            {
                File.Delete(strVehicleFilePath + @".bak");
                btnBve5Recovery.Visible = false;
                OpenNewVehicleFile(strVehicleFilePath);
            
            }
        }
    }
}
