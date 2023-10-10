using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        public List<string> MapFiles { get; private set; }

        public int VehicleFilesCount { get; private set; }
        public int MapFilesCount { get; private set; }

        public Senario(string senarioFilePath)
        {
            FilePath = senarioFilePath;

            //内容を読み込み、表示する
            //string dir = Path.GetDirectoryName(senarioFilePath);
            using (StreamReader sr_temp = new StreamReader(senarioFilePath))
            {

                Encoding enc;
                if (sr_temp.ReadLine().IndexOf("shift_jis", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    enc = Encoding.GetEncoding("shift_jis");
                }
                else
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
                            string item = line.Substring(0, line.IndexOf("=")).Trim();
                            string contents = line.Substring(line.IndexOf("=") + 1).Trim();
                            switch (item)
                            {
                                case "VehicleTitle"://車両タイトル
                                    VehicleTitle = LineAnalysis(contents)[0];
                                    break;

                                case "Vehicle"://車両ファイル
                                    VehicleFilesRel = LineAnalysis(contents);
                                    VehicleFilesAbs = new List<string>();
                                    for (int i = 0; i < VehicleFilesRel.Count; i++)
                                    {
                                        VehicleFilesAbs.Add(Path.GetFullPath(Path.GetDirectoryName(FilePath)) + @"\" + VehicleFilesRel[i]);
                                    }
                            
                                    break;
                                case "Image"://Imageファイル抽出
                                    ImagePath = Path.GetDirectoryName(senarioFilePath) + @"\" + LineAnalysis(contents)[0];
                                    break;

                                case "RouteTitle"://マップタイトル
                                    RouteTitle = LineAnalysis(contents)[0];
                                    break;

                                case "Route":
                                    MapFiles = LineAnalysis(contents);
                                    break;

                                case "Title":
                                    Title = LineAnalysis(contents)[0];
                                    break;

                                case "Author":
                                    Author = LineAnalysis(contents)[0];
                                    break;

                                case "Comment":
                                    Comment = LineAnalysis(contents)[0];
                                    break;
                            }
                        }
                    }
                }
                VehicleFilesCount = VehicleFilesRel.Count;
                MapFilesCount = MapFiles.Count;
            }
        }
        private List<string> LineAnalysis(string contents)
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
