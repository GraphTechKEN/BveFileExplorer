using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BveFileExplorer
{
    public class Map
    {
        private readonly StringBuilder _log = new StringBuilder();
        public string Log => _log.ToString();
        public string FilePath { get; private set; }
        public float FileVersion { get; private set; }
        public Contents_Map Structure { get; private set; }
        public Contents_Map Station { get; private set; }
        public Contents_Map Signal { get; private set; }
        public Contents_Map SoundList { get; private set; }
        public Contents_Map Sound3DList { get; private set; }
        public List<Contents_Map> Train { get; private set; } = new List<Contents_Map>();

        public int encMode { get; private set; } = EncodingDetector.ModeUnknown; // 0:未判定, 1:utf-8, 2:shift_jis

        /// <summary>読み込みに使用したエンコード</summary>
        public Encoding FileEncoding { get; private set; }

        /// <summary>ファイルヘッダーが仕様から外れている場合の警告(手動指定時・問題なしは空文字)</summary>
        public string HeaderWarning { get; private set; } = "";

        private static readonly Regex VersionPattern = new Regex(@"BveTs Map\s+([0-9.]+)", RegexOptions.IgnoreCase);

        public Map(string mapFilePath, bool IsReadIndexOnly = false, Encoding enc = null)
        {
            if (!File.Exists(mapFilePath)) return;

            FilePath = mapFilePath;
            _log.Append("マップファイル：").Append(mapFilePath).Append("\r\n");

            // エンコードを判定する
            // エンコード: 指定があればそれを使い、なければ1行目のヘッダー(":shift_jis" 等)から判定する
            if (enc == null)
            {
                EncodingDetector.HeaderInfo header = EncodingDetector.Analyze(mapFilePath);
                enc = header.Encoding;
                HeaderWarning = header.Warning;
            }
            FileEncoding = enc;
            encMode = EncodingDetector.ModeOf(enc);

            using (StreamReader sr = new StreamReader(mapFilePath, enc))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    _log.Append(line).Append("\r\n");

                    // コメント行は対象外
                    if (line.StartsWith(";") || line.StartsWith("#")) continue;

                    if (line.IndexOf("BveTs Map", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // BveTs Map 2.02:utf-8 → 2.02
                        Match match = VersionPattern.Match(line);
                        if (match.Success &&
                            float.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float version))
                        {
                            FileVersion = version;
                        }
                        if (IsReadIndexOnly) break;
                        continue;
                    }
                    ParseLine(line);
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

        private static bool ContainsCommand(string line, string command) => line.IndexOf(command, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public class Contents_Map : IFileReference
    {
        public bool IsExist { get; private set; } = false;
        public string FilePathAbs { get; private set; } = "";

        /// <summary>マップファイルに記述された(相対)パス</summary>
        public string FilePath { get; private set; } = "";

        string IFileReference.AbsolutePath => FilePathAbs;

        public List<(string trainKey, string filePath, string trackKey, string direction)> TrainFilesList { get; private set; }
            = new List<(string trainKey, string filePath, string trackKey, string direction)>();

        public int Ret { get; private set; } = 0;
        public string Message { get; private set; } = "";
        public Color Color { get; private set; } = SystemColors.Window;

        private static readonly Regex ArgumentPattern = new Regex(@"\((.*)\)");

        public Contents_Map(string line, string filePath)
        {
            PathControl(line, filePath);
            IsExist = File.Exists(FilePathAbs);
        }

        private void PathControl(string line, string filePath)
        {
            // カッコ内の値を取得
            Match match = ArgumentPattern.Match(line);
            if (!match.Success)
            {
                Message = "Not Found : Not declared";
                Color = Color.LightYellow;
                Ret = 0;
                return;
            }

            // カンマで分割した後、各要素から引用符を取り除く
            string[] parts = match.Groups[1].Value
                .Split(',')
                .Select(p => p.Trim().Trim('\'', '\"'))
                .ToArray();

            if (parts.Length >= 4)
            {
                // Train.Add(trainKey, filePath, trackKey, direction)
                FilePath = parts[1];
                TrainFilesList.Add((parts[0], parts[1], parts[2], parts[3]));
            }
            else if (parts.Length > 0)
            {
                // 通常の Load(filePath)
                FilePath = parts[0];
            }

            // パス解決と存在確認(不正なパスは空文字になる)
            FilePathAbs = PathUtil.CombineAbs(filePath, FilePath);
            if (FilePathAbs != "" && File.Exists(FilePathAbs))
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
    }
}
