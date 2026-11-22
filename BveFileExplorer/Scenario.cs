using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BveFileExplorer
{
    public class Scenario
    {
        private readonly StringBuilder _log = new StringBuilder();

        public string FilePath { get; private set; }
        public string Log => _log.ToString();
        public string VehicleTitle { get; private set; }
        public string ImagePath { get; private set; }
        public string RouteTitle { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public string Comment { get; private set; }
        public List<string> VehicleFiles { get; private set; }

        public List<(string FileName, double Ratio)> VehicleFilesList { get; private set; }
        public List<string> VehicleFilesAbs { get; private set; } = new List<string>();

        public List<bool> VehicleFilesExists { get; private set; } = new List<bool>();
        public List<string> MapFiles { get; private set; }

        public List<(string FileName, double Ratio)> MapFilesList { get; private set; }
        public List<string> MapFilesAbs { get; private set; } = new List<string>();

        public int VehicleFilesCount { get; private set; }
        public int MapFilesCount { get; private set; }

        public int VehicleFilesExistsCount { get; private set; }
        public int VehicleFilesNotExistsCount { get; private set; }

        public int encMode { get; private set; } = EncodingDetector.ModeUnknown;

        /// <summary>読み込みに使用したエンコード</summary>
        public Encoding FileEncoding { get; private set; }

        /// <summary>ファイルヘッダーが仕様から外れている場合の警告(手動指定時・問題なしは空文字)</summary>
        public string HeaderWarning { get; private set; } = "";

        public Scenario(string senarioFilePath, Encoding enc = null)
        {
            if (!string.Equals(Path.GetExtension(senarioFilePath), ".txt", StringComparison.OrdinalIgnoreCase)) return;

            FilePath = senarioFilePath;
            string scenarioDir = Path.GetFullPath(Path.GetDirectoryName(FilePath));

            // エンコード: 指定があればそれを使い、なければ1行目のヘッダー(":shift_jis" 等)から判定する
            if (enc == null)
            {
                EncodingDetector.HeaderInfo header = EncodingDetector.Analyze(senarioFilePath);
                enc = header.Encoding;
                HeaderWarning = header.Warning;
            }
            FileEncoding = enc;
            encMode = EncodingDetector.ModeOf(enc);

            using (StreamReader sr = new StreamReader(senarioFilePath, enc))
            {
                _log.Append("路線ファイル：").Append(senarioFilePath).Append("\r\n");

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    _log.Append(line).Append("\r\n");

                    // コメント行は対象外。「=」を含む行のみ
                    if (line.StartsWith(";") || line.StartsWith("#") || !line.Contains("=")) continue;

                    string[] str = line.Split(new[] { '=' }, 2);
                    string item = str[0].Trim().ToLower();
                    string contents = str[1].Trim();
                    if (string.IsNullOrEmpty(contents)) continue;

                    switch (item)
                    {
                        case "vehicletitle": //車両タイトル
                            VehicleTitle = FirstName(contents);
                            break;

                        case "vehicle": //車両ファイル
                            VehicleFilesList = StringLineAnalysis(contents);
                            VehicleFiles = VehicleFilesList.Select(x => x.FileName).ToList();

                            foreach (string vehicleFile in VehicleFiles)
                            {
                                if (string.IsNullOrEmpty(vehicleFile))
                                {
                                    VehicleFilesAbs.Add("");
                                    VehicleFilesExists.Add(false);
                                    VehicleFilesNotExistsCount += 1;
                                }
                                else
                                {
                                    string vehicleAbsPath = scenarioDir + @"\" + vehicleFile;
                                    bool exists = File.Exists(vehicleAbsPath);
                                    VehicleFilesAbs.Add(vehicleAbsPath);
                                    VehicleFilesExists.Add(exists);
                                    if (!exists) VehicleFilesNotExistsCount += 1;
                                }
                            }
                            break;

                        case "image": //Imageファイル
                            ImagePath = Path.GetDirectoryName(senarioFilePath) + @"\" + FirstName(contents);
                            break;

                        case "routetitle": //マップタイトル
                            RouteTitle = FirstName(contents);
                            break;

                        case "route":
                            MapFilesList = StringLineAnalysis(contents);
                            MapFiles = MapFilesList.Select(x => x.FileName).ToList();
                            foreach (string mapFile in MapFiles)
                            {
                                MapFilesAbs.Add(string.IsNullOrEmpty(mapFile) ? "" : scenarioDir + @"\" + mapFile);
                            }
                            break;

                        case "title":
                            Title = FirstName(contents);
                            break;

                        case "author":
                            Author = FirstName(contents);
                            break;

                        case "comment":
                            Comment = FirstName(contents);
                            break;
                    }
                }
            }

            VehicleFilesCount = VehicleFiles?.Count ?? 0;
            VehicleFilesExistsCount = VehicleFilesCount - VehicleFilesNotExistsCount;
            MapFilesCount = MapFiles?.Count ?? 0;
        }

        private string FirstName(string contents) => StringLineAnalysis(contents)[0].FileName;

        /// <summary>
        /// "a.txt * 2 | b.txt" → [(a.txt, 2), (b.txt, 1)]
        /// </summary>
        private List<(string FileName, double Ratio)> StringLineAnalysis(string contents)
        {
            var listStr = new List<(string FileName, double Ratio)>();
            if (contents.Length == 0)
            {
                listStr.Add(("", 1.0));
                return listStr;
            }

            foreach (string path in contents.Split('|'))
            {
                string[] pathAndRatio = path.Split('*');
                double ratio = 1.0;
                if (pathAndRatio.Length > 1 &&
                    !double.TryParse(pathAndRatio[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out ratio))
                {
                    ratio = 0; // 解析できない値(従来どおり0)
                }
                listStr.Add((pathAndRatio[0].Trim(), ratio));
            }
            return listStr;
        }
    }
}
