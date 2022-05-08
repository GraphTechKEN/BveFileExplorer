using System;
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
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            lblAtsPluginFileName.Text = "";
        }

        private string strRouteFilePath = "";
        private string strVehicleFilePath = "";
        private string strAtsPluginSettingFilePath = "";
        private string strAtsPluginFilePath = "";
        private bool flgAtsPluginFileOpen = false;
        private bool flgAtsPluginDirectoryOpen = false;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            btnOpenRouteFile.Enabled = false;
            btnOpenVehicleFile.Enabled = false;
            btnOpenVehicleDirectory.Enabled = false;
            btnOpenAtsPluginFile.Enabled = false;
            btnOpenAtsPluginDirectory.Enabled = false;
            strRouteFilePath = "";
            strVehicleFilePath = "";
            strAtsPluginSettingFilePath = "";
            flgAtsPluginFileOpen = false;
            btnGenerateRelativePath.Enabled = false;

            textBox1.Clear();
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd2 = new OpenFileDialog();
            ofd2.Filter = "路線ファイル(*.txt)|*.txt";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd2.InitialDirectory = Properties.Settings.Default.RouteFileDirectory;
            //ofd.RestoreDirectory = true;

            //ダイアログを表示する
            if (ofd2.ShowDialog() == DialogResult.OK)
            {
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
                    string path = "";
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
                                        path = path.Substring(line.IndexOf("=") + 1);
                                        multiVehicle++;
                                    }
                                    else
                                    {
                                        path = line.Substring(line.IndexOf("=") + 1);
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
                            }
                        }
                        i++;
                    }

                    //閉じる
                    sr.Close();
                    stream.Close();

                    if ((row > 0) && !errflg)
                    {
                        string dir = Path.GetDirectoryName(strRouteFilePath);
                        strVehicleFilePath = dir + "\\" + path.Trim();
                        if (File.Exists(strVehicleFilePath))
                        {
                            MessageBox.Show("車両ファイルが見つかりました");
                            if (multiVehicle > 0)
                            {
                                MessageBox.Show("車両ファイルが複数あります、手動で設定してください。データ数：" + multiVehicle);
                            }
                            OpenNewVehicleFile(strVehicleFilePath);
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

        private void OpenNewVehicleFile(string strVehicleFilePath_)
        {
            if (File.Exists(strVehicleFilePath_))
            {
                btnOpenVehicleFile.Enabled = true;
                btnOpenVehicleDirectory.Enabled = true;
                //内容を読み込み、表示する
                StreamReader sr = new StreamReader(strVehicleFilePath_);
                string line = "";
                int i = 0;
                int row = 0;
                string path = "";
                textBox1.AppendText("\r\n");
                textBox1.AppendText("車両ファイル：" + strVehicleFilePath_ + "\r\n");
                while ((line = sr.ReadLine()) != null)
                {
                    textBox1.AppendText(line+"\r\n");
                    if (line.IndexOf("ATS") >= 0 || line.IndexOf("Ats") >= 0)
                    {
                        row = i;

                        path = line.Substring(line.IndexOf("=") + 1);
                        /*while (line.IndexOf("..") >=0 ){
                            line = line.Substring(line.IndexOf("..") + 1);
                            upDirCount++;
                        }*/

                    }
                    i++;
                }
                //閉じる
                sr.Close();
                //MessageBox.Show(upDirCount.ToString());
                if (row > 0)
                {
                    string dir = Path.GetDirectoryName(strVehicleFilePath_);
                    strAtsPluginSettingFilePath = Path.GetFullPath(Path.GetDirectoryName(dir + "\\" + path.Trim()) + "\\detailmodules.txt");
                    if (File.Exists(strAtsPluginSettingFilePath))
                    {
                        MessageBox.Show("ATSプラグインが見つかりました");
                        OpenNewAtsPluginFile(strAtsPluginSettingFilePath);
                    }
                    else
                    {
                        MessageBox.Show("ATSプラグインが見つからないか、対応していません(DetailModule以外)");
                    }

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
                string dir = "";
                textBox1.AppendText("\r\n");
                textBox1.AppendText("ATSプラグイン：" + AtsPluginFilePath_ + "\r\n");
                while ((line = sr.ReadLine()) != null)
                {
                    textBox1.AppendText(line + "\r\n");
                    if (line.IndexOf(Path.GetFileName(strAtsPluginFilePath)) >= 0)
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
                    MessageBox.Show("ATSプラグインファイルを開き、ATSプラグインパス生成ボタンによりパスを生成し、追記してください。", "SerialOutputプラグインの追加");
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
            Process.Start(strVehicleFilePath);
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
                lblAtsPluginFileName.Text = "ターゲット:" + Path.GetFileName(ofd.FileName);
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
                "\r\n" + 
                "【注意事項】\r\n※動作保証なし、自己責任かつ個人使用でお願いします※", "使い方");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
