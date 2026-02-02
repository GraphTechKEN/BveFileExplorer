using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BveFileExplorer
{
    public class Senario
    {
        public string FilePath { get; private set; }
        public string Log { get; private set; }
        public string VehicleTitle { get; private set; }
        public string ImagePath { get; private set; }
        public string RouteTitle { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public string Comment { get; private set; }
        public List<string> VehicleFilesRel { get; private set; }
        public List<string> VehicleFilesAbs { get; private set; }

        public List<bool> VehicleFilesExists { get; private set; }
        public List<string> MapFiles { get; private set; }

        public int VehicleFilesCount { get; private set; }
        public int MapFilesCount { get; private set; }

        public int VehicleFilesExistsCount { get; private set; }
        public int VehicleFilesNotExistsCount { get; private set; }

        public Senario(string senarioFilePath)
        {
            FilePath = senarioFilePath;
            VehicleFilesAbs = new List<string>();
            VehicleFilesExists = new List<bool>();

            //内容を読み込み、表示する
            //string dir = Path.GetDirectoryName(senarioFilePath);
            using (StreamReader sr_temp = new StreamReader(senarioFilePath))
            {

                Encoding enc = Encoding.GetEncoding("shift_jis");
                string tmp_str = sr_temp.ReadLine();
                if (tmp_str.IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) <= 0 && tmp_str.IndexOf("shift-jis", StringComparison.OrdinalIgnoreCase) <= 0)
                {
                    enc = Encoding.GetEncoding("utf-8");
                }

                using (StreamReader sr = new StreamReader(senarioFilePath, enc))
                {
                    string line = "";
                    Log += "路線ファイル：" + senarioFilePath + "\r\n";

                    //最後まで読込
                    while ((line = sr.ReadLine()) != null)
                    {
                        //余計な文字列をトリム
                        line = line.Trim();
                        //読込ログに追記
                        Log += line + "\r\n";

                        //先頭文字が「;」と「#」でないときで「=」を含むとき
                        if ((!line.StartsWith(";") || !line.StartsWith("#")) && line.Contains("="))
                        {
                            //"="より前の項目
                            string item = line.Substring(0, line.IndexOf("=")).Trim().ToLower();
                            //"="より後の内容
                            string contents = line.Substring(line.IndexOf("=") + 1).Trim();
                            switch (item)
                            {
                                case "vehicletitle"://車両タイトル
                                    VehicleTitle = StringLineAnalysis(contents)[0];
                                    break;

                                case "vehicle"://車両ファイル
                                    VehicleFilesRel = StringLineAnalysis(contents);
                                    
                                    for (int i = 0; i < VehicleFilesRel.Count; i++)
                                    {
                                        string vehicleAbsPath = Path.GetFullPath(Path.GetDirectoryName(FilePath)) + @"\" + VehicleFilesRel[i];
                                        if (VehicleFilesRel[i] == "" || VehicleFilesRel[i] == null)
                                        {
                                            vehicleAbsPath = "";
                                        }
                                        VehicleFilesAbs.Add(vehicleAbsPath);
                                        if (VehicleFilesRel[i] != "" && VehicleFilesRel[i] != null)
                                        {
                                            VehicleFilesExists.Add(File.Exists(vehicleAbsPath));
                                            VehicleFilesNotExistsCount += !File.Exists(vehicleAbsPath) ? 1 : 0;
                                        }
                                        else
                                        {
                                            VehicleFilesExists.Add(false);
                                            VehicleFilesNotExistsCount += 1;
                                        }
                                        
                                    }
                            
                                    break;
                                case "image"://Imageファイル抽出
                                    ImagePath = Path.GetDirectoryName(senarioFilePath) + @"\" + StringLineAnalysis(contents)[0];
                                    break;

                                case "routetitle"://マップタイトル
                                    RouteTitle = StringLineAnalysis(contents)[0];
                                    break;

                                case "route":
                                    MapFiles = StringLineAnalysis(contents);
                                    break;

                                case "title":
                                    Title = StringLineAnalysis(contents)[0];
                                    break;

                                case "author":
                                    Author = StringLineAnalysis(contents)[0];
                                    break;

                                case "comment":
                                    Comment = StringLineAnalysis(contents)[0];
                                    break;
                            }
                        }
                    }
                }
                if (VehicleFilesRel != null)
                {
                    VehicleFilesCount = VehicleFilesRel.Count;
                    VehicleFilesExistsCount = VehicleFilesRel.Count - VehicleFilesNotExistsCount;
                }
                else
                {
                    VehicleFilesCount = 0;
                    VehicleFilesExistsCount = 0;
                }
                
                if (MapFiles != null)
                {
                    MapFilesCount = MapFiles.Count;
                }
                else
                {
                    MapFilesCount = 0;
                }
            }
        }
        

        private List<string> StringLineAnalysis(string contents)
        {
            List<string> listStr = new List<string>();
            //文字列中に「=」以降文字列が存在するとき
            if (contents.Length > 0)
            {
                string path;
                //文字列に「|」があるとき
                if (contents.IndexOf("|") > 0)
                {
                    //"|"が文字列に無くなるまで
                    while (contents.IndexOf("|") > 0)
                    {
                        //パスを文字列から「|」まで切り抜き
                        path = contents.Substring(0, contents.IndexOf("|")).Trim();
                        //パスの中にもし「*」が含まれていたらそこまで切り抜く処理
                        if (path.IndexOf("*") > 0)
                        {
                            path = path.Substring(0, path.IndexOf("*")).Trim();
                        }
                        //パスリストに追加
                        listStr.Add(path);
                        //次のループ用に文字列切り抜き
                        contents = contents.Substring(contents.IndexOf("|") + 1).Trim();

                    }
                    //もし最終文字列に「*」が含まれていた場合は除く
                    if (contents.IndexOf("*") > 0)
                    {
                        path = contents.Substring(0, contents.IndexOf("*")).Trim();
                    }
                    else
                    {
                        path = contents;
                    }
                    //パスリストに追加
                    listStr.Add(path);

                }
                //文字列に「|」がないとき
                else
                {
                    path = contents;
                    listStr.Add(path);
                }
            }
            //文字列がない時空白
            else
            {
                listStr.Add("");
            }
            return listStr;
        }

    }
}
