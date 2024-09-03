using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace BilibiliMonitor
{
    public static class Helper
    {
        public static long TimeStamp => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        public static string UA { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36 Edg/111.0.1661.62";

        public static bool CompareNumString(string a, string b)
        {
            if (a.Length != b.Length)
            {
                return a.Length > b.Length;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return a[i] > b[i];
                }
            }
            return false;
        }

        public static Point Copy(this Point p)
        {
            return new Point(p.X, p.Y);
        }

        public static PointF Copy(this PointF p)
        {
            return new PointF(p.X, p.Y);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="path">目标文件夹</param>
        /// <param name="overwrite">重复时是否覆写</param>
        /// <returns></returns>
        public static async Task<bool> DownloadFile(string url, string path, bool overwrite = false)
        {
            using var http = new HttpClient();
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return false;
                }

                string fileName = GetFileNameFromURL(url);
                if (!overwrite && File.Exists(Path.Combine(path, fileName)))
                {
                    return true;
                }

                var r = await http.GetAsync(url);
                byte[] buffer = await r.Content.ReadAsByteArrayAsync();
                Directory.CreateDirectory(path);
                File.WriteAllBytes(Path.Combine(path, fileName), buffer);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        public static async Task<string> Get(string url, string cookie = "")
        {
            try
            {
                HttpClientHandler handler = new()
                {
                    CookieContainer = new CookieContainer()
                };
                if (!string.IsNullOrEmpty(cookie))
                {
                    foreach (var item in cookie.Split(';'))
                    {
                        if (string.IsNullOrEmpty(item) is false)
                        {
                            string[] c = item.Split('=');
                            handler.CookieContainer.Add(new Uri("https://api.bilibili.com/"), new Cookie(c.First(), c.Last()));
                        }
                    }
                }

                using var http = new HttpClient(handler);
                http.DefaultRequestHeaders.Add("user-agent", UA);
                var r = await http.GetAsync(url);
                r.Content.Headers.ContentType.CharSet = "UTF-8";
                return await r.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                LogHelper.Info("Get", e.Message);
                return string.Empty;
            }
        }

        public static string GetFileNameFromURL(this string url)
        {
            return url.Split('/').Last();
        }

        public static bool JudgeEmoji(this char c)
        {
            return c is >= (char)0xD800 and <= (char)0xDBFF;
        }

        public static string ParseLongNumber(long num)
        {
            string numStr = num.ToString();
            int step = 1;
            for (int i = numStr.Length - 1; i > 0; i--)
            {
                if (step % 3 == 0)
                {
                    numStr = numStr.Insert(i, ",");
                }
                step++;
            }
            return numStr;
        }

        public static string ParseNum2Chinese(this int num)
        {
            return num > 10000 ? $"{num / 10000.0:f1}万" : num.ToString();
        }

        public static async Task<string> Post(string url, object payload)
        {
            using var http = new HttpClient();
            var r = await http.PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            return await r.Content.ReadAsStringAsync();
        }

        public static DateTime TimeStamp2DateTime(long timestamp) => new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddSeconds(timestamp);
    }
}