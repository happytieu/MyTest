using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;

namespace Spider
{
    class Program
    {
        private const string URL = @"https://www.basketball-reference.com/players/";
        private const string FILE_PATH = @"C:\Answer\";
        private const string TITLE = "Player,G,PTS,TRB,AST,FG(%),FG3(%),FT(%),eFG(%),PER,WS";
        private readonly string[] _letters = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        private static Program _program = new Program();

        /// <summary>
        /// 抓取URL所有字母球員戰績寫入csv檔。 例:A.csv B.csv....
        /// </summary>
        static void Main(string[] args)
        {
            _program.Process();
            Console.ReadKey();
        }

        /// <summary>
        /// 把每個字母的球員寫入csv
        /// </summary>
        private void Process()
        {
            foreach (var letter in _program._letters)
            {
                StringBuilder answer = new StringBuilder(TITLE);
                SavePlayersRecord(letter, answer);
                File.WriteAllText(FILE_PATH + letter + ".csv", answer.ToString()); // 寫入csv檔
                Console.WriteLine($"{letter.ToUpper()} is OK");
            }
        }

        /// <summary>
        /// 儲存Players數據到answer
        /// </summary>
        private static void SavePlayersRecord(string letter, StringBuilder answer)
        {
            var letterPage = _program.GetHtmlByURL(URL + letter + "/"); // 個別字母球員列表頁面
            var rows = Regex.Split(letterPage, @"<tr ><th"); // 擷取每個球員
            for (int j = 1; j < rows.Length; j++)
            {
                _program.GetPlayerContent(rows[j], letter, answer);
            }
        }

        /// <summary>
        /// 取網頁原始碼
        /// </summary>
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

        /// <summary>
        /// 擷取LetterPlayer數據
        /// </summary>
        private void GetPlayerContent(string row, string i, StringBuilder answer)
        {
            GetNameAndSite(row, out string playerSite, out string playerName);
            var values = GetRecord(i, playerSite);

            string dataOne = "";
            string dataTwo = "";
            foreach (Match value in values)
            {
                var valueOne = value.Groups["valueOne"].Value;
                dataOne += "," + valueOne;
                var valueTwo = value.Groups["valueTwo"].Value;
                dataTwo += "," + valueTwo;
            }

            var ans = playerName;
            if (dataOne.Length > 10) // 判斷個人數據是否為兩行
            {
                answer.AppendLine(ans + dataOne + "\n" + " " + dataTwo);
            }
            else
            {
                answer.AppendLine(ans + dataTwo);
            }
            Console.WriteLine($"{playerName} is join");
        }

        /// <summary>
        /// 擷取個人數據
        /// </summary>
        private MatchCollection GetRecord(string i, string playerSite)
        {
            var playerRecord = _program.GetHtmlByURL(URL + i + "/" + playerSite + ".html"); // 字母球員個人資料頁面
            var values = Regex.Matches(playerRecord, @"<p>(?<valueOne>[^<]*)</p>\n<p>(?<valueTwo>[^<]*)</p>"); // 擷取個人數據
            return values;
        }

        /// <summary>
        /// 提取個人頁面 & 個人名稱
        /// </summary>
        private void GetNameAndSite(string row, out string playerSite, out string playerName)
        {
            var playerContent = Regex.Match(row, @"<a href=""/players/\w/(?<name>[^.]*).html"">(?<baller>[^<]*)<"); // 擷取球員
            playerName = playerContent.Groups["baller"].Value;
            playerSite = playerContent.Groups["name"].Value;
            if (string.IsNullOrEmpty(playerSite) || string.IsNullOrEmpty(playerName))
                return;
        }
    }
}
