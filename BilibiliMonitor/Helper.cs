using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor
{
    public static class Helper
    {
        public static long TimeStamp => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
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
    }
}
