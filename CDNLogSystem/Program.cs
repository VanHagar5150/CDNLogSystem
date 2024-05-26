using System;
using System.Net;
using System.Text;

namespace MyConsoleApp
{
    class CDNLogSystem
    {
        static void Main(string[] args)
        {
            string logPath = string.Empty;

            try
            {
                logPath = args[0];

                string logContent = ReadLog(logPath);
                ProccessLog(logContent);
            }
            catch (Exception ex)
            {
                Console.Write("Não foi possível ler o log " + logPath + ". Exceção: " + ex.Message + ". StackTrace: " + ex.StackTrace);
            }
        }

        private static bool PathIsUrl(string path)
        {
            if (File.Exists(path))
                return false;
            try
            {
                Uri uri = new(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string ReadLog(string logUri)
        {
            if (PathIsUrl(logUri))
            {
                var webRequest = WebRequest.Create(logUri);

                using var response = webRequest.GetResponse();
                using var content = response.GetResponseStream();
                using var reader = new StreamReader(content);
                var strContent = reader.ReadToEnd();

                return strContent;
            }
            else
            {
                string contents = File.ReadAllText(logUri);

                return contents;
            }
        }

        public static void ProccessLog(string input)
        {
            var lines = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 0)
            {
                Console.Write("Log is empty");
            }
            else
            {
                var result = new List<string>
                {
                    // Cabeçalho
                    "#Version: 1.0",
                    "#Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    "#Fields: provider http-method status-code uri-path time-taken response-size cache-status"
                };

                foreach (var logEntry in lines)
                {
                    var logEntryParts = logEntry.Split('|');

                    var provider = "\"MINHA CDN\"";
                    var httpMethod = logEntryParts[3].Split(' ')[0].Trim('"');
                    var statusCode = logEntryParts[1];
                    var uriPath = logEntryParts[3].Split(' ')[1];
                    var timeTaken = logEntryParts[0];
                    var responseSize = Convert.ToInt32(Math.Round(Convert.ToDouble(logEntryParts[4].Replace("\r", ""))));
                    var cacheStatus = logEntryParts[2];

                    result.Add($"{provider} {httpMethod} {statusCode} {uriPath} {responseSize} {timeTaken} {cacheStatus}");
                }

                Console.Write(string.Join(Environment.NewLine, result));
            }
        }
    }
}