using BveFileExplorer.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AtsPluginEditor
{
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
        private List<string> listVehicleFilePath = new List<string>() ;
        private List<string> listMapFilePath = new List<string>();
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

                tbSeinarioFileName.Text =ofd.FileName;

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
                    textBox1.AppendText("路線ファイル：" + strRouteFilePath + "\r\n");

                    //最後まで読込
                    while ((line = sr.ReadLine()) != null)
                    {
                        textBox1.AppendText(line + "\r\n");
                        //余計な文字列をトリム
                        line = line.Trim();
                        //先頭文字が「;」と「#」でないとき
                        if (line.IndexOf(";") < 0 && line.IndexOf("#") < 0) {
                            //先頭文字がVehicleでVehicleTileでないとき
                            if (line.IndexOf("Vehicle") >= 0)
                            {

                                //車両ファイルセクションここから
                                //
                                if (line.IndexOf("VehicleTitle") < 0)
                                {
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
                                }
                                //車両ファイルセクションここまで

                                //VehicleTitleのとき
                                else
                                {                                    
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblVehicleTitle.Text = line;
                                    }

                                }

                            }
                            //Imageファイル抽出
                            else if (line.IndexOf("Image") >= 0 )
                            {
                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    string file = line.Substring(line.IndexOf("=") + 1);

                                    pictureBox1.ImageLocation = Path.GetDirectoryName(strRouteFilePath) + "\\" + file.Trim();
                                }
                            }
                            //路線
                            else if (line.IndexOf("Route") >= 0)
                            {
                                //マップファイルセクションここから
                                //
                                if (line.IndexOf("RouteTitle") < 0)
                                {
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
                                }
                                //マップファイルセクションここまで

                                else
                                {
                                    //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                    if (line.Substring(line.IndexOf("=")).Length > 1)
                                    {
                                        lblRouteTitle.Text = line;
                                    }
                                }
                            }
                            else if (line.IndexOf("Title") == 0)
                            {
                                //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    lblTitle.Text = line;
                                }
                            }
                            else if (line.IndexOf("Author") == 0)
                            {
                                //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    lblAuthor.Text = line;
                                }
                            }
                            else if (line.IndexOf("Comment") == 0)
                            {
                                //文字列中に「=」があり、切り抜き後の文字列が存在するとき
                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    tbComment.Text = line;
                                }
                            }
                        }
                    }

                    //ファイルを閉じる
                    sr.Close();
                    stream.Close();

                    //車両ファイルが指定されている場合の処理
                    if (_listVehicleFilePath.Count > 0 && !flgErrVehicle)
                    {
                        strVehicleFilePath = dir + "\\" + _listVehicleFilePath[0].Trim();
                        for (int i = 0; i < _listVehicleFilePath.Count; i++)
                        {
                            this.listVehicleFilePath.Add(dir + "\\" + _listVehicleFilePath[i].Trim());
                        }

                        bool IsFileExists = false;
                        for (int i = 0; i < _listVehicleFilePath.Count; i++)
                        {
                            IsFileExists |= File.Exists(this.listVehicleFilePath[i]);
                        }
                        if (IsFileExists)
                        {
                            if ( _listVehicleFilePath.Count > 1)
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
                                this.listVehicleFilePath.Add("Not found or supported : " + dir + "\\" + _listVehicleFilePath[i].Trim());
                            }
                            cbxVehicle.Text = "Not found or supported : " + dir + "\\" + _listVehicleFilePath[0].Trim();
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
                        strMapFilePath = dir + "\\" + _listMapFilePath[0].Trim();
                        
                        for (int i = 0; i < _listMapFilePath.Count; i++)
                        {
                            this.listMapFilePath.Add(dir + "\\" + _listMapFilePath[i].Trim());
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
                                this.listMapFilePath.Add("Not found or supported : " + dir + "\\" + _listMapFilePath[i].Trim());
                            }
                            cbxMapFilePath.Text = "Not found or supported : " + dir + "\\" + _listMapFilePath[0].Trim();
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
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strMapFilePath_);
                string line = "";
                int i = 0;
                int error = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("マップファイル：" + strMapFilePath_ + "\r\n");
                cbxMapFilePath.Text = strMapFilePath_;

                while (((line = sr.ReadLine()) != null))
                {
                    if (line.IndexOf("Structure.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbStructure, ref btnStructureOpen, line, strMapFilePath_, out strStructureFilePath) <= 0)
                        {
                            error++;
                        }
                          
                    }
                    else if (line.IndexOf("Station.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbStation, ref btnStationOpen, line, strMapFilePath_, out strStationFilePath) <= 0) {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Signal.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbSignal, ref btnSignalOpen, line, strMapFilePath_, out strSignalFilePath) <= 0) {
                            error++;
                        }

                    }
                    else if (line.IndexOf("Sound.Load") >= 0)
                    {
                        if (PathControl_Map(ref tbSoundList, ref btnSoundListOpen, line, strMapFilePath_, out strSoundListFilePath) <= 0)
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
                int row = 0;
                int error = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("車両ファイル：" + _strVehicleFilePath + "\r\n");
                

                while ((line = sr.ReadLine()) != null)
                {
                    //テキストボックスに追記
                    textBox1.AppendText(line+"\r\n");
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
                        
                        if(f_ver < 2.00)
                        {
                            lblVehicleVer.Text = "Vehicle File Ver : " + f_ver.ToString("0.00") + " BVE5 専用車両ファイル";
                            lblAts32.Text = "Ats";
                            btnAts64Check.Visible = false;
                            btnAts64OpenFile.Visible = false;
                            btnAts64OpenDirectory.Visible = false;
                            tbAts64.Visible=false;
                            gbxBve6.Visible = false;
                            lblAts64.Visible = false;
                            label9.Visible = false;
                            btnAts64RPathGen.Visible = false;
                            btnAts64Add.Visible = false;
                            tbAts64RelatePath.Visible = false;
                        }
                        else
                        {
                            lblVehicleVer.Text = "Vehicle File Ver : " + f_ver.ToString("0.00") + " BVE6 対応車両ファイル";
                            lblAts32.Text = "Ats32";
                        }
                    }
                    if ((line.IndexOf("ATS", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("Ats32", StringComparison.OrdinalIgnoreCase) >= 0) && line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        row = i;
                        if(PathControl_Vehicle(ref tbAts32, ref btnAts32Open, line, _strVehicleFilePath, out strAts32DetailManagerFilePath) <= 0)
                        {
                            error++;
                            btnAts32Check.Enabled = false;
                        }
                    }
                    else if (line.IndexOf("PerformanceCurve", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if(PathControl_Vehicle(ref tbPerfoemanceCurve, ref btnPerfoemanceCurveOpen, line, _strVehicleFilePath, out strPerfoemanceCurveFilePath) <= 0){
                            error++;
                        }
                    }
                    else if (line.IndexOf("Parameters", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if(PathControl_Vehicle(ref tbParameters, ref btnParametersOpen, line, _strVehicleFilePath, out strParametersFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if(PathControl_Vehicle(ref tbPanel, ref btnPanelOpen, line, _strVehicleFilePath, out strPanelFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Sound", StringComparison.OrdinalIgnoreCase) >= 0 && ( line.IndexOf("Sound") < line.IndexOf("=")))
                    {
                        if(PathControl_Vehicle(ref tbSound, ref btnSoundOpen, line, _strVehicleFilePath, out strSoundFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("MotorNoise", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        if(PathControl_Vehicle(ref tbMotorNoise, ref btnMotorNoiseOpen, line, _strVehicleFilePath, out strMotorNoiseFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Ats64",StringComparison.OrdinalIgnoreCase) >= 0)//Ats64が見つかった場合
                    {
                        //ファイルチェック
                        int ret = PathControl_Vehicle(ref tbAts64, ref btnAts64OpenFile, line, _strVehicleFilePath, out strAts64DetailManagerFilePath);
                        if (ret <= 0)
                        {
                            error++;
                            btnBootBVE6.Enabled = false;
                            btnAts64OpenFile.Enabled = false;
                            btnAts64OpenDirectory.Enabled = false;
                            btnAts64Check.Enabled = false;

                            btnAts64Check.Visible = false;
                            btnAts64OpenFile.Visible = false;
                            btnAts64OpenDirectory.Visible = false;
                            tbAts64.Visible = false;
                            gbxBve6.Visible = false;
                            lblAts64.Visible = false;
                        }
                        else if(ret >= 1)
                        {
                            btnBootBVE6.Enabled = true;
                            btnAts64OpenFile.Enabled = true;
                            btnAts64OpenDirectory.Enabled = true;
                            btnAts64Check.Enabled = true;

                            btnAts64Check.Visible = true;
                            btnAts64OpenFile.Visible = true;
                            btnAts64OpenDirectory.Visible = true;
                            tbAts64.Visible = true;
                            gbxBve6.Visible = true;
                            lblAts64.Visible = true;
                            label9.Visible = true;
                            btnAts64RPathGen.Visible = true;
                            btnAts64Add.Visible = true;
                            tbAts64RelatePath.Visible = true;

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

                if (strAts32DetailManagerFilePath.IndexOf("DetailManager") > 0)
                {
                    strDisp += "ATSプラグイン(DetailManager)が見つかりました\n\n";
                    strAts32SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(strAts32DetailManagerFilePath) + "\\detailmodules.txt");
                    OpenNewAtsPluginFile(strAts32SettingTextFilePath);
                }
                else
                {
                    strDisp += "ATSプラグインが見つからないか、対応していません(DetailManager以外)\n\n";
                }
                if (f_ver >= 2.0 && strAts64DetailManagerFilePath.IndexOf("DetailManager") > 0)
                {
                    strDisp += "BVE6 ATSプラグイン(DetailManager)が見つかりました\n\n";
                    strAts64SettingTextFilePath = Path.GetFullPath(Path.GetDirectoryName(strAts64DetailManagerFilePath) + "\\detailmodules.txt");
                    OpenNewAtsPluginFile(strAts64SettingTextFilePath);
                }
                else
                {
                    strDisp += "BVE6 ATSプラグインが見つからないか、対応していません(DetailManager以外)\n\n";
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

        private void OpenNewAtsPluginFile(string strAtsPluginFilePath)
        {
            if (File.Exists(strAtsPluginFilePath))
            {
                btnOpenAts32Directory.Enabled = true;
                btnAts32Check.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strAtsPluginFilePath);
                string line = "";
                int i = 0;
                int row = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("ATSプラグイン：" + strAtsPluginFilePath + "\r\n");
                while ((line = sr.ReadLine()) != null)
                {
                    textBox1.AppendText(line + "\r\n");
                    if (line.IndexOf(Path.GetFileName(this.strAtsPluginFilePath)) > 0)
                    {
                        row = i + 1;
                    }
                    i++;
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
            Process.Start(strRouteFilePath);
        }

        private void btnOpenVehicleFile_Click(object sender, EventArgs e)
        {
            if(listVehicleFilePath.Count > 1)
            {
                Process.Start(listVehicleFilePath[cbxVehicleIndex]);
            }
            else
            {
                Process.Start(strVehicleFilePath);
            }
        }

        private void btnOpenAts32_Click(object sender, EventArgs e)
        {
            Process.Start(strAts32SettingTextFilePath);
        }

        private void btnOpenAts32Directory_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(strAts32DetailManagerFilePath));
        }

        private void btnOpenAts64Directory_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(strAts64SettingTextFilePath));
        }

        private void btnOpenVehicleDirectory_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(strVehicleFilePath));
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

            int iRet = AtsPluginChecker(strAtsPluginFilePath, 300,cbMessageDisp.Checked);
            tbAts32RelatePath.Text = "";
            tbAts64RelatePath.Text = "";
            tbAts32RelatePath.Enabled = false;
            tbAts64RelatePath.Enabled = false;
            if (iRet == 1)
            {
                lblAtsPluginFile.Text = "ATSプラグイン BVE5用 (32bit)ビルド";

                btnAts64Add.Enabled = false;
                btnAts64RPathGen.Enabled = false;
                btnAts32Add.Enabled = false;

                if (tbSeinarioFileName.Text != "")
                {
                    btnAts32RelatePathGen.Enabled = true;
                    tbAts32.Enabled = true;
                }


            }
            else if(iRet == 2)
            {
                lblAtsPluginFile.Text = "ATSプラグイン BVE6用 (64bit)ビルド";
                btnAts32Add.Enabled = false;
                btnAts32RelatePathGen.Enabled = false;
                btnAts64Add.Enabled = false;
                if (tbSeinarioFileName.Text != "")
                {
                    if (f_ver >= 2.0)
                    {
                        btnAts64RPathGen.Enabled = true;
                        tbAts64.Enabled = true;
                    }
                    else
                    {
                        btnAts64RPathGen.Enabled = false;
                        tbAts64.Enabled = false;
                    }
                }
            }

            ofd.Dispose();
        }

        private string GanarateRelativePath(string OriginalFilePath, string TargetPath)
        {
            string relativePath = "";
            if (OriginalFilePath != "" || TargetPath != "")
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
            int iRet = AtsPluginChecker(strAtsPluginFilePath, 300, false);
            string relativePath = "";
            if (iRet == 1)
            {
                relativePath = GanarateRelativePath(strAts32SettingTextFilePath, strAtsPluginFilePath);
            }
            else if(iRet == 2)
            {
                relativePath = GanarateRelativePath(strAts64SettingTextFilePath, strAtsPluginFilePath);
            }

            textBox1.AppendText("\r\n");
            textBox1.AppendText("ATSプラグインファイル(detailmodules.txtに下記ファイルパスを追記して下さい\r\n");
            textBox1.AppendText(relativePath);
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
                "4.ATSプラグインファイルパスを生成し、追記する\r\r" +
                "\r\n" +
                "【できないこと】\r\n" +
                "DetailManager非対応のプラグイン、自動追加削除機能\r\n" +
                "include形式ファイル、パス内変数形式、自動追加削除機能\r\n" +
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

       private void ProcessStart(string FilePath)
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

        private void btnMapOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath);
        }

        private void btnPerfoemanceCurveOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strPerfoemanceCurveFilePath);
        }

        private void btnParametersOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strParametersFilePath);
        }

        private void btnPanelOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strPanelFilePath);
        }

        private void btnSoundOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundFilePath);
        }

        private void btnMotorNoiseOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strMotorNoiseFilePath);
        }

        private void btnAts64Open_Click(object sender, EventArgs e)
        {
            ProcessStart(strAts64SettingTextFilePath);
        }

        private void btnMapOpen_Click_1(object sender, EventArgs e)
        {
            ProcessStart(strMapFilePath);
        }

        private void btnStructureOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strStructureFilePath);
        }

        private void btnStationOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strStationFilePath);
        }

        private void btnSignalOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSignalFilePath);
        }

        private void btnSoundListOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strSoundListFilePath);
        }

        private void btnTrainOpen_Click(object sender, EventArgs e)
        {
            ProcessStart(strTrainFilePath);
        }

        //マップファイルのファイルパスを生成する
        private string PathGenerator_Map (string _line , string _dirPath)
        {
            string s = "";
            if (_line.IndexOf("'") > 0)
            {
                int index = _line.IndexOf("'");
                s = Path.GetFullPath(Path.GetDirectoryName(_dirPath) + "\\" + (_line.Substring(index + 1, _line.IndexOf("'", index + 1) - index - 1)).Trim());
            }else if((_line.IndexOf("(") > 0))
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
        private int PathControl_Map(ref TextBox tb, ref Button btn, string line_, string mapPath_, out string strFilePath_)
        {
            if (line_.Substring(line_.IndexOf("(")).Length > 1)
            {
                strFilePath_ = PathGenerator_Map(line_, mapPath_);
                if (File.Exists(strFilePath_))
                {
                    tb.Text = strFilePath_;
                    btn.Enabled = true;
                    tb.BackColor = SystemColors.Window;
                    return 1;
                }
                else
                {
                    tb.Text = "Not Found or Supported : " + strFilePath_;
                    tb.BackColor= Color.LightYellow;
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
        private int PathControl_Vehicle(ref TextBox tb, ref Button btn, string line_, string vehiclePath_, out string strFilePath_)
        {
            if (line_.Substring(line_.IndexOf("=")).Length > 1)
            {
                strFilePath_ = PathGenerator_Vehicle(line_, vehiclePath_);
                if (File.Exists(strFilePath_))
                {
                    tb.Text = strFilePath_;
                    btn.Enabled = true;
                    tb.BackColor = SystemColors.Window;
                    return 1;
                }
                else
                {
                    tb.Text = "Not Found or Supported : " + strFilePath_;
                    tb.BackColor = SystemColors.Window;
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

        //車両ファイル選択コンボボックス　選択変更イベント発生時
        private void cbxVehicle_SelectionChangeCommitted(object sender, EventArgs e)
        {
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32.Text = "";
            tbAts64.Text = "";
            cbxVehicle.Text = "";
            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
        }

        //リセット
        private void Reset()
        {
            btnOpenSenarioFile.Enabled = false;
            btnOpenVehicleFile.Enabled = false;
            btnOpenVehicleDirectory.Enabled = false;
            btnOpenAts32Directory.Enabled = false;
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
            btnParametersOpen.Enabled = false;
            btnPanelOpen.Enabled = false;
            btnSoundOpen.Enabled = false;
            btnMotorNoiseOpen.Enabled = false;
            btnAts32Open.Enabled = false;
            btnAts64OpenFile.Enabled = false;


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

            textBox1.Clear();

            pictureBox1.Image = null;

        }

        private void btnBootBVE5_Click(object sender, EventArgs e)
        {
            if(File.Exists(strBve5Path)){
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
            if(Settings.Default.strBve5Path != "")
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
            make_newVehicleFile(@strRouteFilePath, @strRouteFilePath + ".tmp.txt");
        }

        private void make_newVehicleFile(string oldFileName_ , string newFileName_)
        {
            File.Copy(oldFileName_, oldFileName_ + ".bak", true);

            //読み込むテキストファイル
            string textFile = oldFileName_;
            //文字コード(ここでは、Shift JIS)
            Encoding enc = Encoding.GetEncoding("utf-8");

            string[] lines = File.ReadAllLines(textFile);

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf(";") != 0 && lines[i].IndexOf("#") != 0) {
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

        private void cbMessage_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.cbMessage = cbMessageDisp.Checked;
            Settings.Default.Save();
        }

        private void btnBveBootChooseVehicle_Click(object sender, EventArgs e)
        {
            string strNewRouteFile = strRouteFilePath + ".tmp.txt";
            make_newVehicleFile(@strRouteFilePath, strNewRouteFile);
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
            //cbxVehicle.Text = "";

            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
        }

        private void btnAts32Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(strAts32DetailManagerFilePath, 300,true);
        }

        private void btnAts64Check_Click(object sender, EventArgs e)
        {
            AtsPluginChecker(strAts64DetailManagerFilePath, 300,true);
        }

        private int AtsPluginChecker(string _FilePath, int _BufferSize, bool IsDisp)
        {
            int iRet = 0;
            byte[] bufr = new byte[_BufferSize];
            if (_FilePath != "")
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
                        if (IsDisp)
                        {
                            MessageBox.Show("BVE5(32bit)プラグイン");
                        }
                        iRet = 1;
                        ret = false;
                    }
                    else if ((i < _BufferSize - 5) && bufr[i] == 0x50 && bufr[i + 1] == 0x45 && bufr[i + 2] == 0x00 && bufr[i + 3] == 0x00 && bufr[i + 4] == 0x64 && bufr[i + 5] == 0x86)
                    {
                        if (IsDisp)
                        {
                            MessageBox.Show("BVE6(64bit)プラグイン");
                        }
                        iRet = 2;
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
                            if (IsDisp)
                            {
                                MessageBox.Show("判定できませんでした");
                            }
                            ret = false;
                        }
                    }
                }
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
            DialogResult result = MessageBox.Show("操作は取り消しできません。続行します。", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                File.AppendAllText(strAts32SettingTextFilePath, Environment.NewLine + tbAts32RelatePath.Text);
            }
        }

        private void btnAts64Add_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("detailM操作は取り消しできません。続行します。", "注意", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.OK)
            {
                File.AppendAllText(strAts64SettingTextFilePath, Environment.NewLine + tbAts64RelatePath.Text);
            }
        }
    }
}
