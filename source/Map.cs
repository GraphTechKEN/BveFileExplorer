using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

        public Map(string mapFilePath)
        {

            FilePath = mapFilePath;

            string line = "";
            int error = 0;
            int i = 0;
            Train = new List<Contents_Map>();
            Log += "車両ファイル：" + mapFilePath + "\r\n";

            using (StreamReader sr = new StreamReader(mapFilePath))

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
                        if (line.IndexOf("Bvets Map", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            int index_colon = line.IndexOf(":");
                            if (index_colon < 0)
                            {
                                FileVersion = float.Parse(line.Substring(line.IndexOf("Bvets Map", StringComparison.OrdinalIgnoreCase) + 9).Trim());
                            }
                            else
                            {
                                FileVersion = float.Parse(line.Substring(line.IndexOf("Bvets Map", StringComparison.OrdinalIgnoreCase) + 9, index_colon - 9).Trim());
                            }
                        }
                        if (line.IndexOf("Structure.Load", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Structure = new Contents_Map(line, FilePath);
                        }
                        else if (line.IndexOf("Station.Load", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Station = new Contents_Map(line, FilePath);
                        }
                        else if (line.IndexOf("Signal.Load", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Signal = new Contents_Map(line, FilePath);
                        }
                        else if (line.IndexOf("Sound.Load", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            SoundList = new Contents_Map(line, FilePath);
                        }
                        else if (line.IndexOf("Sound3D.Load", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Sound3DList = new Contents_Map(line, FilePath);
                        }
                        else if (line.IndexOf("Train.Add", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Train.Add(new Contents_Map(line, FilePath));
                        }
                        i++;
                    }

                    if (error > 0)
                    {
                        Log += "いくつかのファイルにエラーがあるか、読込未対応ファイル形式です。\n\n";
                    }
                }

        }

    }
    public class Contents_Map
    {
        public bool IsExist { get; private set; } = false;
        public string FilePath { get; private set; } = "";
        public int Ret { get; private set; } = 0;
        public string Message { get; private set; } = "";
        public Color Color { get; private set; } = SystemColors.Window;

        public Contents_Map(string line, string filePath)
        {
            PathControl(line, filePath);
            IsExist = File.Exists(FilePath);
        }

        private void PathControl(string line, string filePath)
        {
            if (line.Substring(line.IndexOf("(")).Length > 1)
            {
                FilePath = PathGenerator_Map(line, filePath);
                if (File.Exists(FilePath))
                {
                    Ret = 1;
                }
                else
                {
                    Message = "Not Found or Supported" + filePath;
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

        //マップファイルのファイルパスを生成する
        private string PathGenerator_Map(string line, string dirPath)
        {
            line = line.Trim();
            line = line.Substring(line.IndexOf("(")+1).Trim();
            line = line.Substring(0, line.IndexOf(")")).Trim();
            line = line.Replace("'", "").Trim();
            if (line.Contains(","))//'Train.Add'の場合
            {
                var lines = line.Split(',');
                line = lines[1].Trim();

            }
            string path = Path.GetFullPath(Path.GetDirectoryName(dirPath) + @"\" + line); 
            return path;
        }
    }
}
