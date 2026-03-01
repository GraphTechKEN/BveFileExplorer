using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BveFileExplorer
{
    public class Map
    {
        public string Log { get; private set; } = "";
        public string FilePath { get; private set; }
        public float FileVersion { get; private set; }
        public Contents_Map Structure { get; private set; }
        public Contents_Map Station { get; private set; }
        public Contents_Map Signal { get; private set; }
        public Contents_Map SoundList { get; private set; }
        public Contents_Map Sound3DList { get; private set; }
        public List<Contents_Map> Train { get; private set; } 

        public int encMode { get; private set; } = 0; // 0:未判定, 1:utf-8, 2:shift_jis

        public Map(string mapFilePath, bool IsReadIndexOnly = false,Encoding enc = null)
        {
            if (File.Exists(mapFilePath))
            {
                FilePath = mapFilePath;

                string line = "";
                int error = 0;
                Train = new List<Contents_Map>();
                Log += "車両ファイル：" + mapFilePath + "\r\n";
;
                //内容を読み込み、表示する
                //エンコードを判定するために一度読み込む
                using (StreamReader sr_temp = new StreamReader(mapFilePath))
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
                using (StreamReader sr = new StreamReader(mapFilePath))

                    //最後まで読込
                    while ((line = sr.ReadLine()) != null)
                    {
                        //余計な文字列をトリム
                        line = line.Trim();
                        //読込ログに追記
                        Log += line + "\r\n";

                        //先頭文字が「;」と「#」でないとき
                        if (!line.StartsWith(";") && !line.StartsWith("#"))
                        {
                            if (line.IndexOf("Bvets Map", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //BveTs map 2.02:utf-8;//基本形
                                // 正規表現パターン: "BveTs Map " の後ろにある数字とドットの組み合わせをグループ化
                                // [0-9.]+ は、数字またはドットが1回以上続くことを意味します
                                string pattern = @"BveTs Map\s+([0-9.]+)";

                                Match match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    string versionStr = match.Groups[1].Value;

                                    // float型に変換 (カルチャの影響を避けるため InvariantCulture を推奨)
                                    if (float.TryParse(versionStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float version))
                                    {
                                        FileVersion = version;
                                    }
                                }
                                if (IsReadIndexOnly) break;
                                continue;
                            }
                            ParseLine(line);
                        }

                        if (error > 0)
                        {
                            Log += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式です。\n";
                        }

                    }
            }
        }
        private void ParseLine(string line)
        {
            // 大文字小文字を区別せずに判定
            if (ContainsCommand(line, "Structure.Load")) Structure = new Contents_Map(line, FilePath);
            else if (ContainsCommand(line, "Station.Load")) Station = new Contents_Map(line, FilePath);
            else if (ContainsCommand(line, "Signal.Load")) Signal = new Contents_Map(line, FilePath);
            else if (ContainsCommand(line, "Sound.Load")) SoundList = new Contents_Map(line, FilePath);
            else if (ContainsCommand(line, "Sound3D.Load")) Sound3DList = new Contents_Map(line, FilePath);
            else if (ContainsCommand(line, "Train.Add")) Train.Add(new Contents_Map(line, FilePath));
        }

        private bool ContainsCommand(string line, string command) => line.IndexOf(command, StringComparison.OrdinalIgnoreCase) >= 0;

    }
    public class Contents_Map
    {
        public bool IsExist { get; private set; } = false;
        public string FilePathAbs { get; private set; } = "";

        public string FilePath { get; private set; } = "";

        // タプルのリストを初期化
        public List<(string trainKey, string filePath, string trackKey, string direction)> TrainFilesList { get; private set; }
            = new List<(string trainKey, string filePath, string trackKey, string direction)>();


        public int Ret { get; private set; } = 0;
        public string Message { get; private set; } = "";
        public Color Color { get; private set; } = SystemColors.Window;

        public Contents_Map(string line, string filePath)
        {
            PathControl(line, filePath);
            IsExist = File.Exists(FilePathAbs);
        }

        private void PathControl(string line, string filePath)
        {
            // カッコ内の値を取得
            var match = Regex.Match(line, @"\((.*)\)");
            if (match.Success)
            {
                // カンマで分割した後、各要素から引用符を取り除く
                string[] parts = match.Groups[1].Value
                    .Split(',')
                    .Select(p => p.Trim().Trim('\'', '\"'))
                    .ToArray();

                // 4つの引数がある場合 (Train.Add または Train[].Load)
                if (parts.Length >= 4)
                {
                    string tKey = parts[0];
                    string fPath = parts[1];
                    string trKey = parts[2];
                    string dir = parts[3];

                    FilePath = fPath;
                    TrainFilesList.Add((tKey, fPath, trKey, dir));
                }
                else if (parts.Length > 0)
                {
                    // 引数が1つの場合 (通常の Load 形式)
                    FilePath = parts[0];
                }

                // パス解決と存在確認
                FilePathAbs = PathCombineAbs(filePath, FilePath);
                if (File.Exists(FilePathAbs))
                {
                    Ret = 1;
                }
                else
                {
                    Message = "Not Found or Supported: " + FilePathAbs;
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

        private string PathCombineAbs(string directory, string path)
        {
            string filePath = "";
            try
            {
                filePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(directory.Trim()), path.Trim()));
            }
            catch (Exception ex){
                MessageBox.Show($"{ ex.Message} dir:{directory} file:{path}");
            }
            return filePath;
        }
    }
}
