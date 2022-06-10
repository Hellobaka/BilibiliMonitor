using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace BilibiliMonitor
{
    public static class Helper
    {
        public static long TimeStamp => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        public static DateTime TimeStamp2DateTime(long timestamp) => new DateTime(1970, 1, 1, 8, 0, 0, DateTimeKind.Local).AddSeconds(timestamp);
        public static async Task<string> Get(string url)
        {
            using var http = new HttpClient();
            var r = await http.GetAsync(url);
            return await r.Content.ReadAsStringAsync();
        }
        public static async Task<string> Post(string url, object payload)
        {
            using var http = new HttpClient();
            var r = await http.PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            return await r.Content.ReadAsStringAsync();
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
                if (string.IsNullOrWhiteSpace(url)) return false;
                string fileName = GetFileNameFromURL(url);
                if (!overwrite && File.Exists(Path.Combine(path, fileName))) return true;
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
        public static string GetFileNameFromURL(this string url)
        {
            return url.Split('/').Last();
        }
        public static string ParseNum2Chinese(this int num)
        {
            if (num > 10000)
            {
                return $"{num / 10000.0:f1}万";
            }
            return num.ToString();
        }
        public static bool CompareNumString(string a, string b)
        {
            if (a.Length != b.Length)
                return a.Length>b.Length;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return a[i] > b[i];
            }
            return false;
        }
        public static bool JudgeEmoji(this char c)
        {
            return c >= 0xD800 && c <= 0xDBFF;
        }
        public static Point Copy(this Point p)
        {
            return new Point(p.X, p.Y);
        }
        public static PointF Copy(this PointF p)
        {
            return new PointF(p.X, p.Y);
        }
    }
}
