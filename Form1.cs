using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            lblAtsPluginFileName.Text = "";
        }

        private string strRouteFilePath = "";
        private string strVehicleFilePath = "";
        private List<string> listVehicleFilePath = new List<string>() ;
        private string strAtsPluginSettingFilePath = "";
        private string strAtsPluginFilePath = "";
        private string strMapFilePath = "";
        private string strPerfoemanceCurveFilePath = "";
        private string strParametersFilePath = "";
        private string strPanelFilePath = "";
        private string strSoundFilePath = "";
        private string strMotorNoiseFilePath = "";
        private string strAtsPlugin32SettingFilePath = "";
        private string strAtsPlugin64SettingFilePath = "";
        private string strStructureFilePath = "";
        private string strStationFilePath = "";
        private string strSignalFilePath = "";
        private string strSoundListFilePath = "";
        private string strTrainFilePath = "";
        private bool flgAtsPluginFileOpen = false;
        private bool flgAtsPluginDirectoryOpen = false;
        private int cbxVehicleIndex = 0;

        private void btnOpen_Click(object sender, EventArgs e)
        {

            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd2 = new OpenFileDialog();
            ofd2.Filter = "路線ファイル(*.txt)|*.txt";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd2.InitialDirectory = Properties.Settings.Default.RouteFileDirectory;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                Reset();

                Properties.Settings.Default.RouteFileDirectory = Path.GetDirectoryName(ofd2.FileName);
                //OKボタンがクリックされたとき、選択されたファイルを読み取り専用で開く
                System.IO.Stream stream;
                stream = ofd2.OpenFile();
                strRouteFilePath = ofd2.FileName;
                btnOpenRouteFile.Enabled = true;
                if (stream != null)
                {
                    //内容を読み込み、表示する
                    StreamReader sr =
                        new StreamReader(stream);
                    string line = "";
                    int i = 0;
                    int multiVehicle = 0;
                    int row = 0;
                    bool errflg = false;
                    var listPath = new List<string>();
                    string path = "";
                    string dir = Path.GetDirectoryName(strRouteFilePath);
                    textBox1.AppendText("路線ファイル：" + strRouteFilePath + "\r\n");

                    while ((line = sr.ReadLine()) != null)
                    {
                        textBox1.AppendText(line + "\r\n");
                        line = line.Trim();
                        if (line.IndexOf(";") < 0 && line.IndexOf("#") < 0) {
                            if (line.IndexOf("Vehicle") >= 0 && line.IndexOf("VehicleTitle") < 0)
                            {

                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    row = i;
                                    if (line.IndexOf("|") > 0)
                                    {
                                        path = line.Substring(0, line.IndexOf("|"));
                                        //listPath.Add(path.Substring(line.IndexOf("=") + 1));
                                        string temp_line = line.Substring(line.IndexOf("=") + 1);
                                        //"|"が文字列に無くなるまで
                                        while (temp_line.IndexOf("|") > 0)
                                        {
                                            
                                            path = temp_line.Substring(0, temp_line.IndexOf("|"));
                                            if(path.IndexOf("*") > 0)
                                            {
                                                path = temp_line.Substring(0, temp_line.IndexOf("*"));
                                            }
                                            //パスリストに追加
                                            listPath.Add(path);
                                            //次のループ用に文字列切り抜き
                                            temp_line = temp_line.Substring(temp_line.IndexOf("|") + 1);
                                        };
                                    }
                                    else
                                    {
                                        listPath.Add(line.Substring(line.IndexOf("=") + 1));
                                    }
                                }
                                else
                                {
                                    errflg = true;
                                }
                            } else if (line.IndexOf("Image") >= 0 )
                            {
                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    row = i;
                                    string file = line.Substring(line.IndexOf("=") + 1);

                                    pictureBox1.ImageLocation = Path.GetDirectoryName(strRouteFilePath) + "\\" + file.Trim();
                                }
                            } else if (line.IndexOf("Route") >= 0 && line.IndexOf("RouteTitle") < 0)
                            {

                                if (line.Substring(line.IndexOf("=")).Length > 1)
                                {
                                    if (line.IndexOf("*") > 0)
                                    {
                                        strMapFilePath = Path.GetFullPath(dir + "\\" + line.Substring(0, line.IndexOf("*")).Substring(line.IndexOf("=") + 1).Trim());
                                    }
                                    else if (line.IndexOf("|") > 0)
                                    {
                                        strMapFilePath = Path.GetFullPath(dir + "\\" +  line.Substring(0, line.IndexOf("|")).Substring(line.IndexOf("=") + 1).Trim());
                                    }
                                    else
                                    {
                                        strMapFilePath = Path.GetFullPath(dir + "\\" +  line.Substring(line.IndexOf("=") + 1).Trim());
                                    }
                                    if (File.Exists(strMapFilePath))
                                    {
                                        tbMapFilePath.Text = strMapFilePath;
                                        btnMapOpen.Enabled = true;
                                        OpenNewMapFile(strMapFilePath);
                                    }
                                    else
                                    {
                                        tbMapFilePath.Text = "Cannot Open";
                                    }
                                    
                                }
                                else
                                {
                                    tbMapFilePath.Text = "Not Found";
                                }
                            }
                        }
                        i++;
                    }

                    //閉じる
                    sr.Close();
                    stream.Close();
                    if ((row > 0) && !errflg)
                    {                         
                        strVehicleFilePath = dir + "\\" + listPath[0].Trim();
                        int index_ = 0;
                        while (index_ < listPath.Count)
                        {
                            listVehicleFilePath.Add(dir + "\\" + listPath[index_].Trim());
                            index_++;
                        }
                        if (File.Exists(listVehicleFilePath[0]))
                        {
                            //MessageBox.Show("車両ファイルが見つかりました");
                            if (listPath.Count > 1)
                            {
                                cbxVehicle.Visible = true;
                                tbVehicle.Visible=false;
                                MessageBox.Show("車両ファイルが複数あります、手動で設定してください。データ数：" + listPath.Count);
                                //MessageBox.Show("車両ファイルが複数あります、手動で設定してください");
                                index_ = 0;
                                while(index_ < listVehicleFilePath.Count)
                                {
                                    cbxVehicle.Items.Add(listVehicleFilePath[index_]);
                                    index_++;
                                }
                                //cbxVehicle.SelectedIndex = 0;
                            }
                            else
                            {
                                tbVehicle.Visible = true;
                                cbxVehicle.Visible = false;
                            }
                            cbxVehicle.Text = listVehicleFilePath[0];
                            OpenNewVehicleFile(listVehicleFilePath[0]);
                        }
                        else
                        {
                            MessageBox.Show("車両ファイルが見つかりません。");
                        }
                    }
                    else
                    {
                        MessageBox.Show("車両ファイルが指定されていません。");
                    }
                }
            }
        }

        private void OpenNewMapFile(string strMapFilePath_)
        {
            if (File.Exists(strMapFilePath_))
            {
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strMapFilePath_);
                string line = "";
                int i = 0;
                int error = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("車両ファイル：" + strMapFilePath_ + "\r\n");
                tbMapFilePath.Text = strMapFilePath_;
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
                    MessageBox.Show("いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m");
                }
            }
            else
            {
                MessageBox.Show("マップファイルが指定されていません");
            }
        }

        private void OpenNewVehicleFile(string strVehicleFilePath_)
        {
            if (File.Exists(strVehicleFilePath_))
            {
                btnOpenVehicleFile.Enabled = true;
                btnOpenVehicleDirectory.Enabled = true;
                btnVehicleOpen.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strVehicleFilePath_);
                string line = "";
                int i = 0;
                int row = 0;
                int error = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("車両ファイル：" + strVehicleFilePath_ + "\r\n");
                tbVehicle.Text = strVehicleFilePath_;

                while ((line = sr.ReadLine()) != null)
                {
                    textBox1.AppendText(line+"\r\n");
                    if ((line.IndexOf("ATS") >= 0 || line.IndexOf("Ats") >= 0 || line.IndexOf("Ats32") >= 0) && line.IndexOf("Ats64") < 0)
                    {
                        row = i;
                        if(PathControl_Vehicle(ref tbAts32, ref btnAts32Open, line, strVehicleFilePath_, out strAtsPlugin32SettingFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("PerformanceCurve") >= 0)
                    {
                        if(PathControl_Vehicle(ref tbPerfoemanceCurve, ref btnPerfoemanceCurveOpen, line, strVehicleFilePath_, out strPerfoemanceCurveFilePath) <= 0){
                            error++;
                        }
                    }
                    else if (line.IndexOf("Parameters") >= 0)
                    {
                        if(PathControl_Vehicle(ref tbParameters, ref btnParametersOpen, line, strVehicleFilePath_, out strParametersFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Panel") >= 0)
                    {
                        if(PathControl_Vehicle(ref tbPanel, ref btnPanelOpen, line, strVehicleFilePath_, out strPanelFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Sound") >= 0 && ( line.IndexOf("Sound") < line.IndexOf("=")))
                    {
                        if(PathControl_Vehicle(ref tbSound, ref btnSoundOpen, line, strVehicleFilePath_, out strSoundFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("MotorNoise") >= 0)
                    {
                        if(PathControl_Vehicle(ref tbMotorNoise, ref btnMotorNoiseOpen, line, strVehicleFilePath_, out strMotorNoiseFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    else if (line.IndexOf("Ats64") >= 0)
                    {
                        if(PathControl_Vehicle(ref tbAts64, ref btnAts64Open, line , strVehicleFilePath_, out strAtsPlugin64SettingFilePath) <= 0)
                        {
                            error++;
                        }
                    }
                    i++;
                }
                //閉じる
                sr.Close();

                if (error > 0)
                {
                    MessageBox.Show("いくつかのファイルにエラーがあるか、読込未対応ファイル形式ですm(_ _)m");
                }

                if (strAtsPlugin32SettingFilePath.IndexOf("DetailManager") > 0)
                {
                    MessageBox.Show("ATSプラグイン(DetailManager)が見つかりました");
                    strAtsPluginSettingFilePath = Path.GetFullPath(Path.GetDirectoryName(strAtsPlugin32SettingFilePath) + "\\detailmodules.txt");
                    OpenNewAtsPluginFile(strAtsPluginSettingFilePath);
                }
                else
                {
                    MessageBox.Show("ATSプラグインが見つからないか、対応していません(DetailManager以外)");
                }
            }
            else
            {
                MessageBox.Show("ATSプラグインが指定されていません");
            }
        }

        private void OpenNewAtsPluginFile(string AtsPluginFilePath_)
        {
            if (File.Exists(AtsPluginFilePath_))
            {
                btnOpenAtsPluginFile.Enabled = true;
                btnOpenAtsPluginDirectory.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(AtsPluginFilePath_);
                string line = "";
                int i = 0;
                int row = 0;
                textBox1.AppendText("\r\n");
                textBox1.AppendText("ATSプラグイン：" + AtsPluginFilePath_ + "\r\n");
                while ((line = sr.ReadLine()) != null)
                {
                    textBox1.AppendText(line + "\r\n");
                    if (line.IndexOf(Path.GetFileName(strAtsPluginFilePath)) > 0)
                    {
                        row = i + 1;
                    }
                    i++;
                }
                //閉じる
                sr.Close();
                if (row > 0)
                {
                    MessageBox.Show("既に追記されています。", "SerialOutputプラグインの追加");
                }
                else
                {
                    MessageBox.Show("ATSプラグインのターゲットを確認後、ATSプラグインパス生成ボタンにより相対パスを生成し、追記してください。", "SerialOutputプラグインの追加");
                }
                flgAtsPluginFileOpen = true;
                if (flgAtsPluginDirectoryOpen)
                {
                    btnGenerateRelativePath.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("ATSプラグインが見つかりません:" + AtsPluginFilePath_);
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

        private void btnOpenAtsPluginFile_Click(object sender, EventArgs e)
        {
            Process.Start(strAtsPluginSettingFilePath);
        }

        private void btnOpenAtsPluginDirectory_Click(object sender, EventArgs e)
        {
            Process.Start(Path.GetDirectoryName(strAtsPluginSettingFilePath));
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
            ofd.InitialDirectory = Properties.Settings.Default.AtsPluginFileDirectory;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.AtsPluginFileDirectory = Path.GetDirectoryName(ofd.FileName);
                strAtsPluginFilePath = ofd.FileName;
                lblAtsPluginFileName.Text = "ターゲット:" + ofd.FileName;
                flgAtsPluginDirectoryOpen = true;
                btnOpen.Enabled = true;
                if (flgAtsPluginFileOpen)
                {
                    btnGenerateRelativePath.Enabled = true;
                }
            }
        }

        private void btnGenerateRelativePath_Click(object sender, EventArgs e)
        {
            Uri u1 = new Uri(strAtsPluginSettingFilePath);
            Uri u2 = new Uri(strAtsPluginFilePath);

            Uri relativeUri = u1.MakeRelativeUri(u2);

            string relativePath = relativeUri.ToString();

            relativePath = relativePath.Replace('/', '\\');

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
                "1.ATSプラグインファイルの場所を指定する" +
                "2.路線ファイルを指定する\r\n" +
                "\r\n3.ATSプラグインファイルを開く\r\n" +
                "4.ATSプラグインファイルパスを生成し、追記する\r\r" +
                "\r\n" +
                "【できないこと】\r\n" +
                "車両ファイルを複数している路線データ、DetailManager非対応のプラグイン、自動追加削除機能\r\n" +
                "include形式ファイル、パス内変数形式、自動追加削除機能\r\n" +
                "\r\n" +
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします※", "使い方");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void btnMapOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strMapFilePath);
        }

        private void btnPerfoemanceCurveOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strPerfoemanceCurveFilePath);
        }

        private void btnParametersOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strParametersFilePath);
        }

        private void btnPanelOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strPanelFilePath);
        }

        private void btnSoundOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strSoundFilePath);
        }

        private void btnMotorNoiseOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strMotorNoiseFilePath);
        }

        private void btnAts64Open_Click(object sender, EventArgs e)
        {
            Process.Start(strAtsPlugin64SettingFilePath);
        }

        private void btnMapOpen_Click_1(object sender, EventArgs e)
        {
            Process.Start(strMapFilePath);
        }

        private void btnStructureOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strStructureFilePath);
        }

        private void btnStationOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strStationFilePath);
        }

        private void btnSignalOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strSignalFilePath);
        }

        private void btnSoundListOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strSoundListFilePath);
        }

        private void btnTrainOpen_Click(object sender, EventArgs e)
        {
            Process.Start(strTrainFilePath);
        }

        private string PathGenerator_Map (string line_ , string dirPath_)
        {
            string s = "";
            if (line_.IndexOf("'") > 0)
            {
                int index = line_.IndexOf("'");
                s = Path.GetFullPath(Path.GetDirectoryName(dirPath_) + "\\" + (line_.Substring(index + 1, line_.IndexOf("'", index + 1) - index - 1)).Trim());
            }else if((line_.IndexOf("(") > 0))
            {
                s = Path.GetFullPath(Path.GetDirectoryName(dirPath_) + "\\" + (line_.Substring(line_.IndexOf("(") + 1, line_.IndexOf(")", line_.IndexOf("(") + 1) - line_.IndexOf("(") - 1)).Trim());
            }
            else
            {
                s = "";
            }
            return s;
        }

        private string PathGenerator_Vehicle(string line_, string dirPath_)
        {
            string s = "";
            s = Path.GetFullPath(Path.GetDirectoryName(dirPath_) + "\\" + line_.Substring(line_.IndexOf("=") + 1).Trim());
            return s;
        }

        private int PathControl_Map(ref TextBox tb, ref Button btn, string line_, string mapPath_, out string strFilePath_)
        {
            if (line_.Substring(line_.IndexOf("(")).Length > 1)
            {
                strFilePath_ = PathGenerator_Map(line_, mapPath_);
                if (File.Exists(strFilePath_))
                {
                    tb.Text = strFilePath_;
                    btn.Enabled = true;
                    return 1;
                }
                else
                {
                    tb.Text = "Not Found or Path Error : " + strFilePath_;
                    return -1;
                }
            }
            else
            {
                strFilePath_ = "Not Found : Not declared";
                tb.Text = "Not Found : Not Declared";
                return 0;
            }
        }

        private int PathControl_Vehicle(ref TextBox tb, ref Button btn, string line_, string vehiclePath_, out string strFilePath_)
        {
            if (line_.Substring(line_.IndexOf("=")).Length > 1)
            {
                strFilePath_ = PathGenerator_Vehicle(line_, vehiclePath_);
                if (File.Exists(strFilePath_))
                {
                    tb.Text = strFilePath_;
                    btn.Enabled = true;
                    return 1;
                }
                else
                {
                    tb.Text = "Not Found or Path Error : " + strFilePath_;
                    return -1;
                }

            }
            else
            {
                strFilePath_ = "Not Found : Not declared";
                tb.Text = "Not Found : Not Declared";
                return 0;
            }
        }

        //車両ファイル選択コンボボックス　選択変更イベント発生時
        private void cbxVehicle_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //コンボボックスのインデックスを取得
            cbxVehicleIndex = cbxVehicle.SelectedIndex;
            //コンボボックスのインデックス番号のファイルパスで車両ファイルを開く
            OpenNewVehicleFile(listVehicleFilePath[cbxVehicleIndex]);
        }

        //リセット
        private void Reset()
        {
            btnOpenRouteFile.Enabled = false;
            btnOpenVehicleFile.Enabled = false;
            btnOpenVehicleDirectory.Enabled = false;
            btnOpenAtsPluginFile.Enabled = false;
            btnOpenAtsPluginDirectory.Enabled = false;
            btnMapOpen.Enabled = false;
            strRouteFilePath = "";
            strVehicleFilePath = "";
            strAtsPluginSettingFilePath = "";
            strMapFilePath = "";
            strPerfoemanceCurveFilePath = "";
            strParametersFilePath = "";
            strPanelFilePath = "";
            strSoundFilePath = "";
            strMotorNoiseFilePath = "";
            strAtsPlugin32SettingFilePath = "";
            strAtsPlugin64SettingFilePath = "";
            strStructureFilePath = "";
            strSignalFilePath = "";
            strSoundListFilePath = "";
            strTrainFilePath = "";

            flgAtsPluginFileOpen = false;
            btnGenerateRelativePath.Enabled = false;
            btnVehicleOpen.Enabled = false;
            btnPerfoemanceCurveOpen.Enabled = false;
            btnParametersOpen.Enabled = false;
            btnPanelOpen.Enabled = false;
            btnSoundOpen.Enabled = false;
            btnMotorNoiseOpen.Enabled = false;
            btnAts32Open.Enabled = false;
            btnAts64Open.Enabled = false;

            tbMapFilePath.Text = "";
            tbVehicle.Text = "";
            cbxVehicle.Items.Clear();
            listVehicleFilePath.Clear();
            tbPerfoemanceCurve.Text = "";
            tbParameters.Text = "";
            tbPanel.Text = "";
            tbSound.Text = "";
            tbMotorNoise.Text = "";
            tbAts32.Text = "";
            tbAts64.Text = "";


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
    }
}
