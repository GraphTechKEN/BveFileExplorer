using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    public class Vehicle
    {
        public string Log { get; private set; } = "";
        public string FilePath { get; private set; }


        /// <summary>
        /// Vehicle File Version 0.03 or 2.00
        /// </summary>
        public float FileVersion { get; private set; }
        public Contents_Vehicle Ats32 { get; private set; }
        public Contents_Vehicle PerformanceCurve { get; private set; }
        public Contents_Vehicle Parameters { get; private set; }
        public Contents_Vehicle Panel { get; private set; }
        public Contents_Vehicle Sound { get; private set; }
        public Contents_Vehicle MotorNoise { get; private set; }
        public Contents_Vehicle Ats64 { get; private set; }

        public int encMode { get; private set; } = 0; // 0:未判定, 1:utf-8, 2:shift_jis

        public Vehicle(string vehicleFilePath, bool IsReadIndexOnly = false, Encoding enc = null)
        {

            FilePath = vehicleFilePath;

            string line = "";
            int error = 0;
            int i = 0;
            Log += "車両ファイル：" + vehicleFilePath + "\r\n";

            if (File.Exists(vehicleFilePath))
            {
                //内容を読み込み、表示する
                //エンコードを判定するために一度読み込む
                using (StreamReader sr_temp = new StreamReader(vehicleFilePath))
                {
                    if (enc == null)
                    {
                        enc = Encoding.GetEncoding("utf-8"); encMode = 1;
                        string tmp_str = sr_temp.ReadLine();
                        if (tmp_str != null)
                        {
                            if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) > 0 || tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                enc = Encoding.GetEncoding("shift_jis");
                                encMode = 2;
                            }
                        }
                    }
                }
                using (StreamReader sr = new StreamReader(vehicleFilePath,enc))
                {
                    //最後まで読込
                    while ((line = sr.ReadLine()) != null)
                    {
                        //余計な文字列をトリム
                        line = line.Trim();
                        //読込ログに追記
                        Log += line + "\r\n";

                        //先頭文字が「;」と「#」でないときで「=」を含むとき
                        if (!line.StartsWith(";") || !line.StartsWith("#"))
                        {
                            if (line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                int index_colon = line.IndexOf(":");
                                if (index_colon < 0)
                                {
                                    FileVersion = float.Parse(line.Substring(line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) + 13).Trim());
                                }
                                else
                                {
                                    FileVersion = float.Parse(line.Substring(line.IndexOf("Bvets Vehicle", StringComparison.OrdinalIgnoreCase) + 13, index_colon - 13).Trim());
                                }
                            }
                            if (IsReadIndexOnly)
                            {
                                break;
                            }
                            if ((line.IndexOf("ATS", StringComparison.OrdinalIgnoreCase) >= 0 || line.IndexOf("Ats32", StringComparison.OrdinalIgnoreCase) >= 0) && line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) < 0)
                            {
                                Ats32 = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("PerformanceCurve", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                PerformanceCurve = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("Parameters", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Parameters = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("Panel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Panel = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("Sound", StringComparison.OrdinalIgnoreCase) >= 0 && (line.IndexOf("Sound") < line.IndexOf("=")))
                            {
                                Sound = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("MotorNoise", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                MotorNoise = new Contents_Vehicle(line, FilePath);
                            }
                            else if (line.IndexOf("Ats64", StringComparison.OrdinalIgnoreCase) >= 0)//Ats64が見つかった場合
                            {
                                Ats64 = new Contents_Vehicle(line, FilePath);
                            }
                            i++;
                        }

                        if (error > 0)
                        {
                            Log += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式です。\n";
                        }
                    }
                }
            }
            else
            {
                Log = "車両ファイル：" + vehicleFilePath + "が見つかりません";
            }
        }

    }

    public class Contents_Vehicle
    {
        public bool IsExist { get; private set; } = false;
        public string FilePath { get; private set; } = "";
        public int Ret { get; private set; } = 0;
        public string Message { get; private set; } = "";
        public Color Color { get; private set; } = SystemColors.Window;

        public Contents_Vehicle(string line, string filePath)
        {
            PathControl(line, filePath);
            IsExist = File.Exists(FilePath);
        }

        private void PathControl(string line, string filePath)
        {
            if (line.Contains("="))
            {
                if (line.Substring(line.IndexOf("=")).Length > 1)
                {
                    FilePath = PathGenerator_Vehicle(line, filePath);
                    if (File.Exists(FilePath))
                    {
                        Ret = 1;
                    }
                    else
                    {
                        Message = "Not Found or Supported";
                        Color = Color.LightYellow;
                        Ret = -1;
                    }

                }
                else
                {
                    Message = "Not Found : Not declared";
                    Color = Color.LightYellow;
                    Ret = 0;
                }
            }
            else
            {
                Message = "Not Found or Supported";
                Color = Color.LightYellow;
                Ret = -1;
            }
        }

        //車両ファイルのファイルパスを生成する
        private string PathGenerator_Vehicle(string line, string filePath)
        {
            return Path.GetFullPath(Path.GetDirectoryName(filePath) + @"\" + line.Substring(line.IndexOf("=") + 1).Trim());
        }
    }


    public class AtsPlugin
    {
        public bool IsDetailManager { get; private set; } = false;
        public string FilePath { get; private set; } = "";
        public string DetailManagerFilePath { get; private set; } = "";
        public string DetailManagerSettingFilePath { get; private set; } = "";

        public string Message { get; private set; } = "";
        public string Log { get; private set; } = "";

        public List<AtsList> AtsList { get; private set; }

        public AtsPlugin(string filePath)
        {
            if (File.Exists(filePath))
            {
                FilePath = filePath;
                if (filePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    //Message += "ATSプラグイン(DetailManager)が見つかりました\n";
                    DetailManagerFilePath = filePath;
                    DetailManagerSettingFilePath = Path.GetFullPath(Path.GetDirectoryName(DetailManagerFilePath) + @"\detailmodules.txt");
                    //MessageBox.Show($"hit" + " AtsPlugin\r\n" + DetailManagerFilePath);
                    OpenNewAtsPluginFile(DetailManagerSettingFilePath, Checker(DetailManagerFilePath, 300, false));
                    IsDetailManager = true;
                }
                else
                {
                    Message += "ATSプラグインが対応していません(DetailManager以外)\n";
                    IsDetailManager = false;
                }
            }
            else
            {
                Message += "ATSプラグインが見つかりません";
                IsDetailManager = false;
            }
        }

        public AtsPlugin()
        {
        }


        private void OpenNewAtsPluginFile(string strAtsPluginFilePath, BVE_Version bve_ver)
        {
            if (!File.Exists(strAtsPluginFilePath))
            {
                Message += $"ATSプラグインが見つかりません: {strAtsPluginFilePath}\n";
                return;
            }

            AtsList = new List<AtsList>();
            string strBveVer = (bve_ver == BVE_Version.BVE6) ? "BVE6用(64bit)" : "BVE5用(32bit)";
            Log += $"ATSプラグイン {strBveVer}: {strAtsPluginFilePath}\r\n";

            try
            {
                using (StreamReader sr = new StreamReader(strAtsPluginFilePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        // 空行やコメント行をスキップ
                        if (string.IsNullOrEmpty(line) || line.StartsWith(";")) continue;

                        // "Ats32=" または "Ats64=" で始まる場合の処理
                        if (line.Contains("="))
                        {
                            line = line.Substring(line.IndexOf("=") + 1).Trim();
                        }

                        Log += line + "\r\n";
                        AtsList.Add(new AtsList(line, strAtsPluginFilePath));

                        // ファイルパスの正規化
                        string cleanLine = line.Replace("#", "").Replace(";", "").Trim();
                        string fullPath;
                        try
                        {
                            string directory = Path.GetDirectoryName(strAtsPluginFilePath);
                            fullPath = Path.GetFullPath(Path.Combine(directory, cleanLine));
                        }
                        catch
                        {
                            fullPath = cleanLine;
                        }

                        // Checker(fullPath, 300, false); // 必要に応じて有効化
                    }
                }
            }
            catch (Exception ex)
            {
                Message += $"ファイル読み込みエラー: {ex.Message}\n";
            }
        }

        /// <summary>
        /// バイナリファイル(プラグイン等)のアーキテクチャ(32bit or 64bit)をチェックします
        /// </summary>
        /// <param name="_FilePath">ファイルパス</param>
        /// <param name="_BufferSize">読込サイズ(300)</param>
        /// <param name="IsDisplayChecked">結果表示(True)</param>
        /// <returns>アーキテクチャおよび結果(Enum BVE_Version)</returns>
        public BVE_Version Checker(string _FilePath, int _BufferSize = 300, bool IsDisplayChecked = true)
        {
            //MessageBox.Show($"hit" + " Checker\r\n" + _FilePath);
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
                    }
                    bool ret = true;
                    int i = 0;
                    while (ret)
                    {
                        if ((i < _BufferSize - 5) && bufr[i] == 0x50 && bufr[i + 1] == 0x45 && bufr[i + 2] == 0x00 && bufr[i + 3] == 0x00 && ((bufr[i + 4] == 0x4C && bufr[i + 5] == 0x01) || (bufr[i + 4] == 0x64 && bufr[i + 5] == 0x86)))
                        {
                            if (bufr[i + 4] == 0x4C && bufr[i + 5] == 0x01)
                            {
                                if (IsDisplayChecked)
                                {
                                    MessageBox.Show("BVE5用(32bit)にビルドされたプラグインです");
                                }
                                iRet = BVE_Version.BVE5;
                                ret = false;
                                return iRet;
                            }
                            else
                            {
                                if (IsDisplayChecked)
                                {
                                    MessageBox.Show("BVE6用(64bit)にビルドされたプラグインです");
                                }
                                iRet = BVE_Version.BVE6; ;
                                ret = false;
                                return iRet;
                            }
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
                                return iRet;
                            }
                        }
                    }
                }
            }
            return iRet;
        }
    }
    public class AtsList
    {
        public string FileName { get; set; } = "";
        public BVE_Version Version { get; set; } = BVE_Version.Null;
        public string RerativePath { get; set; } = "";
        public string AbsolutePath { get; set; } = "";

        public AtsList(string line, string filePath)
        {
            AtsPlugin checker = new AtsPlugin();
            RerativePath = line;
            string marge_line = line.Replace("#", "").Trim();
            marge_line = marge_line.Replace(";", "").Trim();
            try
            {
                AbsolutePath = Path.GetFullPath(Path.GetDirectoryName(filePath) + @"\" + marge_line);
            }
            catch
            {
                AbsolutePath = marge_line;
            }
            FileName = Path.GetFileName(line);
            Version = checker.Checker(AbsolutePath, 300, false);
            //MessageBox.Show($"hit"+" AtsList" + AbsolutePath);
        }
    }
}
