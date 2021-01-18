using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

namespace Spider
{
    class Program
    {
        private const string URL = @"https://www.basketball-reference.com/players/";
        private const string FILE_PATE = @"C:\Answer\";
        private const string TITLE = "Player,G,PTS,TRB,AST,FG(%),FG3(%),FT(%),eFG(%),PER,WS";
        private readonly string[] _letter = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        private static Program _program = new Program();


        static void Main(string[] args)
        {
            foreach (var i in _program._letter)
            {
                TaskFactory factory = new TaskFactory();
                StringBuilder answer = new StringBuilder();
                List<Task> taskList = new List<Task>();
                answer.AppendLine(TITLE);
                var letterPage = _program.GetHtmlByURL(URL + i + "/"); // 個別字母球員列表頁面

                if (string.IsNullOrEmpty(letterPage)) 
                    continue;

                var rows = Regex.Split(letterPage, @"<tr ><th");
                for (int x = 1; x < rows.Length; x++)
                {
                    var count = x;
                    taskList.Add(factory.StartNew(() => _program.GetLetterPlayer(rows[count], i, answer)));
                }

                if (taskList.Count > 0)
                {
                    factory.ContinueWhenAll(taskList.ToArray(), taskArr =>
                    {
                        File.WriteAllText(FILE_PATE + i + ".csv", answer.ToString());
                        Console.WriteLine($"{i.ToUpper()} is OK");
                    });
                }
            }
            Console.ReadKey();
        }

        private string GetHtmlByURL(string url)
        {
            var html = string.Empty;
            try
            {
                var request = WebRequest.Create(url);
                var response = request.GetResponse();
                var stream = new StreamReader(response.GetResponseStream());
                html = Nancy.Helpers.HttpUtility.HtmlDecode(stream.ReadToEnd());
                response.Close();
                stream.Close();
            }
            catch (Exception)
            {
                Console.WriteLine($"{url} is failed.");
            }
            return html;
        }

        private void GetLetterPlayer(string row, string i, StringBuilder answer)
        {
            var ans = string.Empty;
            var nameSite = Regex.Match(row, @"<a href=""/players/\w/(?<name>[^.]*).html"">(?<baller>[^<]*)<"); //擷取個人數據domain
            var baller = nameSite.Groups["baller"].Value; // 球員姓名
            if (string.IsNullOrEmpty(nameSite.Groups["name"].Value) || string.IsNullOrEmpty(nameSite.Groups["baller"].Value)) 
                return; 

            var ballerRecord = _program.GetHtmlByURL(URL + i + "/" + nameSite.Groups["name"].Value + ".html"); // 字母球員個人資料頁面
            if (string.IsNullOrEmpty(ballerRecord)) return;

            var values = Regex.Matches(ballerRecord, @"<p>(?<valueOne>[^<]*)</p>\n<p>(?<valueTwo>[^<]*)</p>"); // 擷取個人數據

            string dataOne = "";
            string dataTwo = "";
            ans += baller;
            foreach (Match value in values)
            {
                var valueOne = value.Groups["valueOne"].Value;
                dataOne += "," + valueOne;
                var valueTwo = value.Groups["valueTwo"].Value;
                dataTwo += "," + valueTwo;
            }

            if (dataOne.Length > 10) // 判斷個人數據是否為兩行
            {
                answer.AppendLine(ans + dataOne + "\n" + " " + dataTwo); // 加入兩行個人數據
            }
            else
            {
                answer.AppendLine(ans + dataTwo);
            }
            Console.WriteLine($"{baller} is join");
        }
    }
}
