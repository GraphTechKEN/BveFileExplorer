using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BveFileExplorer
{
    public class Vehicle
    {
        private readonly StringBuilder _log = new StringBuilder();
        public string Log => _log.ToString();
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

        public int encMode { get; private set; } = EncodingDetector.ModeUnknown; // 0:未判定, 1:utf-8, 2:shift_jis

        /// <summary>読み込みに使用したエンコード</summary>
        public Encoding FileEncoding { get; private set; }

        /// <summary>ファイルヘッダーが仕様から外れている場合の警告(手動指定時・問題なしは空文字)</summary>
        public string HeaderWarning { get; private set; } = "";

        private static readonly Regex VersionPattern = new Regex(@"BveTs Vehicle\s+([0-9.]+)", RegexOptions.IgnoreCase);

        public Vehicle(string vehicleFilePath, bool IsReadIndexOnly = false, Encoding enc = null)
        {
            FilePath = vehicleFilePath;

            if (!File.Exists(vehicleFilePath))
            {
                _log.Append("車両ファイル：").Append(vehicleFilePath).Append("が見つかりません");
                return;
            }

            _log.Append("車両ファイル：").Append(vehicleFilePath).Append("\r\n");

            // エンコードを判定する
            // エンコード: 指定があればそれを使い、なければ1行目のヘッダー(":shift_jis" 等)から判定する
            if (enc == null)
            {
                EncodingDetector.HeaderInfo header = EncodingDetector.Analyze(vehicleFilePath);
                enc = header.Encoding;
                HeaderWarning = header.Warning;
            }
            FileEncoding = enc;
            encMode = EncodingDetector.ModeOf(enc);

            using (StreamReader sr = new StreamReader(vehicleFilePath, enc))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    _log.Append(line).Append("\r\n");

                    // 空行・コメント行は対象外
                    if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#")) continue;

                    if (line.IndexOf("BveTs Vehicle", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Match match = VersionPattern.Match(line);
                        if (match.Success &&
                            float.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float version))
                        {
                            FileVersion = version;
                        }
                    }

                    // ヘッダだけ読めばよい場合は先頭行で終了
                    if (IsReadIndexOnly) break;

                    ParseLine(line);
                }
            }
        }

        /// <summary>
        /// "Key = Value" の Key で判定する(値やパスに含まれる文字列で誤判定しない)
        /// </summary>
        private void ParseLine(string line)
        {
            int eq = line.IndexOf('=');
            if (eq < 0) return;

            string key = line.Substring(0, eq).Trim();

            if (KeyIs(key, "Ats") || KeyIs(key, "Ats32")) Ats32 = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "Ats64")) Ats64 = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "PerformanceCurve")) PerformanceCurve = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "Parameters")) Parameters = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "Panel")) Panel = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "Sound")) Sound = new Contents_Vehicle(line, FilePath);
            else if (KeyIs(key, "MotorNoise")) MotorNoise = new Contents_Vehicle(line, FilePath);
        }

        private static bool KeyIs(string key, string name) => string.Equals(key, name, StringComparison.OrdinalIgnoreCase);
    }

    public class Contents_Vehicle : IFileReference
    {
        public bool IsExist { get; private set; } = false;

        /// <summary>絶対パス</summary>
        public string FilePath { get; private set; } = "";

        string IFileReference.AbsolutePath => FilePath;

        public int Ret { get; private set; } = 0;
        public string Message { get; private set; } = "";
        public Color Color { get; private set; } = SystemColors.Window;

        public Contents_Vehicle(string line, string filePath)
        {
            PathControl(line, filePath);
            IsExist = FilePath != "" && File.Exists(FilePath);
        }

        private void PathControl(string line, string vehicleFilePath)
        {
            int eq = line.IndexOf('=');
            if (eq < 0)
            {
                Message = "Not Found or Supported";
                Color = Color.LightYellow;
                Ret = -1;
                return;
            }

            string value = line.Substring(eq + 1).Trim();
            if (value.Length == 0)
            {
                Message = "Not Found : Not declared";
                Color = Color.LightYellow;
                Ret = 0;
                return;
            }

            FilePath = PathUtil.CombineAbs(vehicleFilePath, value);
            if (FilePath != "" && File.Exists(FilePath))
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
    }


    public class AtsPlugin
    {
        public bool IsDetailManager { get; private set; } = false;
        public string FilePath { get; private set; } = "";
        public string DetailManagerFilePath { get; private set; } = "";
        public string DetailManagerSettingFilePath { get; private set; } = "";

        public string Message { get; private set; } = "";
        public string Log { get; private set; } = "";

        public List<AtsList> AtsList { get; private set; } = new List<AtsList>();

        public AtsPlugin(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Message += "ATSプラグインが見つかりません";
                return;
            }

            FilePath = filePath;
            if (filePath.IndexOf("DetailManager", StringComparison.OrdinalIgnoreCase) > 0)
            {
                DetailManagerFilePath = filePath;
                DetailManagerSettingFilePath = Path.Combine(Path.GetDirectoryName(DetailManagerFilePath), "detailmodules.txt");
                OpenNewAtsPluginFile(DetailManagerSettingFilePath, Checker(DetailManagerFilePath));
                IsDetailManager = true;
            }
            else
            {
                Message += "ATSプラグインが対応していません(DetailManager以外)\n";
            }
        }

        private void OpenNewAtsPluginFile(string settingFilePath, BVE_Version bve_ver)
        {
            if (!File.Exists(settingFilePath))
            {
                Message += $"ATSプラグインが見つかりません: {settingFilePath}\n";
                return;
            }

            string strBveVer = (bve_ver == BVE_Version.BVE6) ? "BVE6用(64bit)" : "BVE5用(32bit)";
            Log += $"ATSプラグイン {strBveVer}: {settingFilePath}\r\n";

            try
            {
                foreach (string rawLine in File.ReadAllLines(settingFilePath))
                {
                    string line = rawLine.Trim();

                    // 空行やコメント行をスキップ
                    if (string.IsNullOrEmpty(line) || line.StartsWith(";")) continue;

                    // "Ats32=" または "Ats64=" で始まる場合の処理
                    if (line.Contains("="))
                    {
                        line = line.Substring(line.IndexOf("=") + 1).Trim();
                    }

                    Log += line + "\r\n";
                    AtsList.Add(new AtsList(line, settingFilePath));
                }
            }
            catch (IOException ex)
            {
                Message += $"ファイル読み込みエラー: {ex.Message}\n";
            }
            catch (UnauthorizedAccessException ex)
            {
                Message += $"ファイル読み込みエラー: {ex.Message}\n";
            }
        }

        /// <summary>
        /// バイナリファイル(プラグイン/実行ファイル)のアーキテクチャ(32bit or 64bit)を判定する。
        /// PEヘッダの位置(e_lfanew)を読み、Machine フィールドで判定する。
        /// </summary>
        /// <returns>BVE5:32bit / BVE6:64bit / Null:判定不可 / NotFound:ファイルなし / Comment:コメントアウト行</returns>
        public static BVE_Version Checker(string filePath)
        {
            string fileName;
            try
            {
                fileName = Path.GetFileName(filePath ?? "");
            }
            catch (ArgumentException)
            {
                return BVE_Version.NotFound;
            }

            if (fileName.StartsWith("#") || fileName.StartsWith(";")) return BVE_Version.Comment;
            if (fileName == "" || !File.Exists(filePath)) return BVE_Version.NotFound;

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    const ushort DosSignature = 0x5A4D;      // "MZ"
                    const uint PeSignature = 0x00004550;     // "PE\0\0"
                    const ushort MachineI386 = 0x014C;
                    const ushort MachineAmd64 = 0x8664;

                    if (fs.Length < 0x40 || br.ReadUInt16() != DosSignature) return BVE_Version.Null;

                    fs.Position = 0x3C;
                    int peOffset = br.ReadInt32();
                    if (peOffset < 0 || peOffset + 6 > fs.Length) return BVE_Version.Null;

                    fs.Position = peOffset;
                    if (br.ReadUInt32() != PeSignature) return BVE_Version.Null;

                    ushort machine = br.ReadUInt16();
                    if (machine == MachineI386) return BVE_Version.BVE5;
                    if (machine == MachineAmd64) return BVE_Version.BVE6;
                    return BVE_Version.Null;
                }
            }
            catch (IOException)
            {
                return BVE_Version.Null;
            }
            catch (UnauthorizedAccessException)
            {
                return BVE_Version.Null;
            }
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
            RerativePath = line;
            string cleanLine = line.Replace("#", "").Replace(";", "").Trim();
            AbsolutePath = PathUtil.CombineAbs(filePath, cleanLine);
            if (AbsolutePath == "") AbsolutePath = cleanLine;

            try
            {
                FileName = Path.GetFileName(line);
            }
            catch (ArgumentException)
            {
                FileName = line;
            }
            // コメントアウト行も記号を除いた実ファイルで判定する(行の灰色表示は呼び出し側で行う)
            Version = AtsPlugin.Checker(AbsolutePath);
        }
    }
}
