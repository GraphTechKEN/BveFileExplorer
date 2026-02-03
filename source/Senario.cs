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

        public List<(string FileName, double Ratio)> VehicleFilesRelList { get; private set; }
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
                                    List<(string FileName, double Ratio)> VehicleTitleList = new List<(string, double)>();
                                    VehicleTitleList = StringLineAnalysis(contents);
                                    VehicleTitle = VehicleTitleList.Select(x => x.FileName).ToList()[0];

                                    break;

                                case "vehicle"://車両ファイル
                                    VehicleFilesRelList = new List<(string, double)>();
                                    VehicleFilesRelList = StringLineAnalysis(contents);
                                    VehicleFilesRel = VehicleFilesRelList.Select(x => x.FileName).ToList();

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
                                    ImagePath = Path.GetDirectoryName(senarioFilePath) + @"\" + StringLineAnalysis(contents).Select(x => x.Item1).ToList()[0];
                                    break;

                                case "routetitle"://マップタイトル
                                    List<(string FileName, double Ratio)> RouteTitleTitleList = new List<(string, double)>();
                                    RouteTitleTitleList = StringLineAnalysis(contents);
                                    RouteTitle = RouteTitleTitleList.Select(x => x.FileName).ToList()[0];
                                    break;

                                case "route":
                                    List<(string FileName, double Ratio)> RouteMapFileList = new List<(string, double)>();
                                    RouteMapFileList = StringLineAnalysis(contents);
                                    MapFiles = RouteMapFileList.Select(x => x.FileName).ToList();
                                    break;

                                case "title":
                                    List<(string FileName, double Ratio)> TitleList = new List<(string, double)>();
                                    TitleList = StringLineAnalysis(contents);
                                    Title = TitleList.Select(x => x.FileName).ToList()[0];
                                    break;

                                case "author":
                                    List<(string FileName, double Ratio)> AuthorList = new List<(string, double)>();
                                    AuthorList = StringLineAnalysis(contents);
                                    Author = AuthorList.Select(x => x.FileName).ToList()[0];
                                    break;

                                case "comment":
                                    List<(string FileName, double Ratio)> CommentList = new List<(string, double)>();
                                    CommentList = StringLineAnalysis(contents);
                                    Comment = CommentList.Select(x => x.FileName).ToList()[0];
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
        

        private List<(string,double)> StringLineAnalysis(string contents)
        {
            List<(string,double)> listStr = new List<(string, double)>();
            double ratio = 1.0;
            //文字列中に「=」以降文字列が存在するとき
            if (contents.Length > 0)
            {
                List<string> result = contents.Split('|').ToList();
                foreach (string path in result)
                {
                    ratio = 1.0;
                    List<string> result2 = path.Split('*').ToList();
                    string path2 = result2[0].Trim();
                    if (result2.Count > 1)
                    {
                        double.TryParse(result2[1], out ratio);
                    }
                    //パスリストに追加
                    listStr.Add((path2, ratio));
                    //MessageBox.Show(ratio.ToString());
                }
            }
            //文字列がない時空白
            else
            {
                listStr.Add(("", ratio));
            }
            return listStr;
        }

    }
}
